using Gclopes.FinVault.CurrentAccount.Data;
using Gclopes.FinVault.CurrentAccount.Domain;
using Gclopes.FinVault.CurrentAccount.Endpoints.DTO;
using Gclopes.FinVault.CurrentAccount.Models;
using Gclopes.FinVault.CurrentAccount.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace Gclopes.FinVault.CurrentAccount.Services.Implementations;

public class MovementService(ApplicationDbContext context, IMemoryCache cache) : IMovementService
{
    // The list of years only changes when a new bank statement is imported, so it is served
    // from memory instead of re-querying the Movements table on every dropdown load.
    private const string AvailableYearsCacheKey = "movements:available-years";

    private static readonly TimeSpan AvailableYearsCacheDuration = TimeSpan.FromMinutes(30);

    // An empty result usually means the query ran before the schema or data was ready, so it is
    // retried aggressively instead of being pinned for the full duration
    private static readonly TimeSpan EmptyYearsCacheDuration = TimeSpan.FromSeconds(15);


    public async Task<PagedResult<MovementResponse>> GetMovementsAsync(MovementQueryParameters query)
    {
        // AsNoTracking: this is a read-only projection, so skip EF's change-tracking overhead
        var filtered = ApplyDateFilters(context.Movements.AsNoTracking(), query);

        // COUNT is done against the filtered set in SQL, not by materializing rows
        var totalCount = await filtered.CountAsync();

        // Default sort: TransactionDate desc, ValueDate desc, applied before paging so the
        // right page of results is fetched directly from the database instead of in memory
        var items = await filtered
            .OrderByDescending(m => m.TransactionDate)
            .ThenByDescending(m => m.ValueDate)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(m => new MovementResponse(
                m.Id,
                m.Amount,
                m.Type,
                m.Balance,
                m.Description,
                m.UserNote,
                m.TransactionDate,
                m.ValueDate,
                m.CreatedAt))
            .ToListAsync();

        return new PagedResult<MovementResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<MovementTotalsResponse> GetMovementTotalsAsync(MovementQueryParameters query)
    {
        var filtered = ApplyDateFilters(context.Movements.AsNoTracking(), query);

        // Credits and debits are grouped by Type in a single round trip instead of one SUM query
        // per type. Type is only ever "C" or "D", so this returns at most two rows.
        var totalsByType = await filtered
            .GroupBy(m => m.Type)
            .Select(g => new
            {
                Type = g.Key,
                // Cast to nullable decimal so a type with no matching rows produces a NULL SUM
                // that is coalesced to 0, instead of failing on a non-nullable decimal
                Total = g.Sum(m => (decimal?)m.Amount) ?? 0m
            })
            .ToListAsync();

        var totalCredits = totalsByType.FirstOrDefault(x => x.Type == "C")?.Total ?? 0m;
        var totalDebits = totalsByType.FirstOrDefault(x => x.Type == "D")?.Total ?? 0m;

        // The current balance is always the running balance of the single most recent movement
        // in the whole account, independent of the active date filters. It cannot be folded into
        // the query above because that one is filtered, so it stays a separate round trip.
        var currentBalance = await context.Movements
            .AsNoTracking()
            .OrderByDescending(m => m.TransactionDate)
            .ThenByDescending(m => m.ValueDate)
            .Select(m => (decimal?)m.Balance)
            .FirstOrDefaultAsync();

        return new MovementTotalsResponse(totalCredits, totalDebits, currentBalance);
    }

    // Shared WHERE clause for both the paginated list and the totals endpoint, so the two
    // stay consistent for the same filter selection
    private static IQueryable<Movement> ApplyDateFilters(IQueryable<Movement> source, MovementQueryParameters query)
    {
        if (query.TransactionDateFrom.HasValue)
            source = source.Where(m => m.TransactionDate >= query.TransactionDateFrom.Value.Date);

        if (query.TransactionDateTo.HasValue)
            source = source.Where(m => m.TransactionDate < query.TransactionDateTo.Value.Date.AddDays(1));

        if (query.ValueDateFrom.HasValue)
            source = source.Where(m => m.ValueDate >= query.ValueDateFrom.Value.Date);

        if (query.ValueDateTo.HasValue)
            source = source.Where(m => m.ValueDate < query.ValueDateTo.Value.Date.AddDays(1));

        return source;
    }

    public async Task<BankStatementResponse> RegisterBankStatementAsync(BankStatementPayloadDto dto)
    {
        // 1. Validação de Domínio (usando o BalanceCalculator que criámos)
        // Extraímos os dados para uma forma simples (tupla) que o Domain entende
        var movementsForCalc = dto.Data.Transactions.Select(t => (t.Amount, t.Type));

        var calculatedFinalBalance = BalanceCalculator.CalculateFinalBalance(dto.Data.InitialBalance, movementsForCalc);

        // Verificação matemática de segurança
        if (Math.Abs(calculatedFinalBalance - dto.Data.FinalBalance) > 0.01m)
        {
            throw new InvalidOperationException($"Inconsistência financeira: O saldo calculado ({calculatedFinalBalance}) não bate com o saldo final fornecido ({dto.Data.FinalBalance}).");
        }

        // 2. Criação do Objeto (Infraestrutura)
        var statement = new BankStatement
        {
            Id = Guid.NewGuid(),
            InitialBalance = dto.Data.InitialBalance,
            FinalBalance = dto.Data.FinalBalance,
        };

        foreach (var t in dto.Data.Transactions)
        {
            var movement = new Movement
            {
                Id = Guid.NewGuid(),
                Amount = t.Amount,
                Type = t.Type,
                Description = t.Description,
                Balance = t.Balance,
                BankStatementId = statement.Id,
                // DateTime.ParseExact é perigoso sem try-catch, mas se já validaste no FluentValidation, aqui é seguro
                TransactionDate = DateTime.ParseExact(t.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                ValueDate = DateTime.ParseExact(t.ValueDate, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                CreatedAt = DateTime.UtcNow
            };
            statement.Movements.Add(movement);
        }

        // 3. Persistência
        context.BankStatements.Add(statement);
        await context.SaveChangesAsync();

        // A new statement can introduce a year that was not in the dropdown before
        cache.Remove(AvailableYearsCacheKey);

        // 4. Retorno (Mapeamento para o Response)
        return new BankStatementResponse(statement.Id, "Bank statement processed successfully.", statement.ProcessedAt);
    }

    public async Task<MovementResponse?> UpdateNoteAsync(Guid movementId, string note)
    {
        // Procura o movimento, mas garante que o utilizador é o dono!
        var movement = await context.Movements
            .FirstOrDefaultAsync(m => m.Id == movementId);

        if (movement == null) return null;

        // Atualiza apenas a nota
        movement.UserNote = note;

        // Grava no banco
        await context.SaveChangesAsync();

        return new MovementResponse(
            movement.Id,
            movement.Amount,
            movement.Type,
            movement.Balance,
            movement.Description,
            movement.UserNote,
            movement.TransactionDate,
            movement.ValueDate,
            movement.CreatedAt);
    }

    /// <summary>
    /// Aggregates credit/debit totals per month for a single year.
    /// The filter is expressed as a half-open date range instead of <c>TransactionDate.Year == year</c>:
    /// wrapping the column in YEAR() makes the predicate non-sargable, which forced SQL Server to
    /// scan the whole Movements table. A range lets IX_Movements_TransactionDate_ValueDate be seeked.
    /// The grouping and the conditional sums also run in SQL (GROUP BY + SUM(CASE WHEN)) rather than
    /// materialising every matching row into memory and re-enumerating each group twice.
    /// </summary>
    public async Task<List<MovementTotalsByYearMonthResponse>> GetTotalsByYearMonthResponse(int year)
    {
        var rangeStart = new DateTime(year, 1, 1);
        var rangeEnd = rangeStart.AddYears(1);

        // Projected into an anonymous type first: EF Core cannot translate a positional record
        // constructor here. Only one row per month ever crosses the wire, so mapping to the
        // response record in memory costs nothing.
        var aggregated = await context.Movements
            .AsNoTracking()
            .Where(m => m.TransactionDate >= rangeStart && m.TransactionDate < rangeEnd)
            .GroupBy(m => new { m.TransactionDate.Year, m.TransactionDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                // Cast to nullable decimal so a month with no movements of that type produces a
                // NULL SUM that is coalesced to 0, instead of failing on a non-nullable decimal
                Credit = g.Where(m => m.Type == "C").Sum(m => (decimal?)m.Amount) ?? 0m,
                Debit = g.Where(m => m.Type == "D").Sum(m => (decimal?)m.Amount) ?? 0m
            })
            .OrderBy(x => x.Month)
            .ToListAsync();

        return
        [
            .. aggregated.Select(x => new MovementTotalsByYearMonthResponse(
                x.Year,
                x.Month,
                x.Credit,
                x.Debit))
        ];
    }

    /// <summary>
    /// Returns the distinct years that actually contain movements, most recent first.
    /// Deliberately kept to a single round trip. An earlier version probed one range query per
    /// candidate year, which multiplied the round trips by the number of years spanned and turned
    /// a single index-only scan into N of them when the index is missing.
    /// The big win here is the cache: after the first call the dropdown costs zero queries.
    /// </summary>
    public async Task<List<int>> GetAvailableYearsAsync()
    {
        if (cache.Get<List<int>>(AvailableYearsCacheKey) is List<int> cachedYears)
        {
            return cachedYears;
        }

        // YEAR() makes this non-sargable, but the (TransactionDate, ValueDate) index lets SQL
        // Server satisfy it from a narrow index-only scan, which is cheap for a single account.
        // Beating it with per-year range seeks would cost more round trips than it saves.
        var years = await context.Movements
            .AsNoTracking()
            .Select(m => m.TransactionDate.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();

        // Never let an empty result stick around: it would keep serving a blank dropdown long
        // after the underlying problem (missing index, schema not migrated yet) is gone
        cache.Set(
            AvailableYearsCacheKey,
            years,
            years.Count > 0 ? AvailableYearsCacheDuration : EmptyYearsCacheDuration);

        return years;
    }
}
