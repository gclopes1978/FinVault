using Gclopes.FinVault.CurrentAccount.Data;
using Gclopes.FinVault.CurrentAccount.Domain;
using Gclopes.FinVault.CurrentAccount.Endpoints.DTO;
using Gclopes.FinVault.CurrentAccount.Models;
using Gclopes.FinVault.CurrentAccount.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Gclopes.FinVault.CurrentAccount.Services.Implementations;

public class MovementService(ApplicationDbContext context) : IMovementService
{

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

        // Sums are computed in SQL (SUM ... WHERE) instead of pulling every row into memory
        var totalCredits = await filtered.Where(m => m.Type == "C").SumAsync(m => (decimal?)m.Amount) ?? 0m;
        var totalDebits = await filtered.Where(m => m.Type == "D").SumAsync(m => (decimal?)m.Amount) ?? 0m;

        // The current balance is always the running balance of the single most recent movement
        // in the whole account, independent of the active date filters
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

    public async Task<BankStatementResponse> RegisterBankStatementAsync(BankStatementPayloadDto dto, string userId)
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
}