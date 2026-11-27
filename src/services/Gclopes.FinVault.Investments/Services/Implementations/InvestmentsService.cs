using Gclopes.FinVault.Investments.Data;
using Gclopes.FinVault.Investments.Data.Entities;
using Gclopes.FinVault.Investments.DTO;
using Gclopes.FinVault.Investments.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Gclopes.FinVault.Investments.Services.Implementations;

public class InvestmentsService(
    FinVaultInvestmentsDb dbContext,
    IXtbExcelImporter xtbExcelImporter,
    ILogger<InvestmentsService> logger) : IInvestmentsService
{

    public async Task<List<PortfolioDto>> GetPortfolioAsync(CancellationToken cancellationToken = default)
    {
        var xtbPositions = await CalculateXtbPositionsAsync(
            cancellationToken);

        var trading212Positions = await CalculateTrading212PositionsAsync(
            cancellationToken);

        var cryptoPositions = await CalculateTrading212CryptoPositionsAsync(cancellationToken);

        var positions = xtbPositions
            .Concat(trading212Positions)
            .Concat(cryptoPositions)
            .ToList();


        return [.. positions.Select(p => new PortfolioDto
        {
            Id = p.Id,
            Symbol = p.Symbol,
            Isin = p.Isin,
            Source = p.Source.ToString(),
            AssetClass = p.AssetClass,
            Quantity = p.Quantity,
            AverageCost = p.AverageCost,
            CurrentPrice = p.CurrentPrice,
            CurrentValue = p.CurrentValue,
            LastUpdated = p.LastUpdated
        })];
    }

    public async Task<List<TransactionDto>> GetPortfolioHistoryAsync(CancellationToken cancellationToken = default)
    {
        // 1. Buscar transações da XTB (Tabela Raw)
        var xtbTransactions = await dbContext.XtbTransactions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // 2. Buscar transações da Trading 212 (Tabela Raw)
        var t212Transactions = await dbContext.Trading212Transactions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // 3. Mapear XTB para o DTO comum
        var xtbDtos = xtbTransactions.Select(t => new TransactionDto
        {
            Id = t.Id.ToString(), // O ID da XTB é um int, passamos a string
            Symbol = t.Ticker,
            Type = t.Type.ToUpperInvariant(),
            Quantity = 0, // ⚠️ O CSV da XTB não tem quantidade, fica a 0
            Price = 0,    // ⚠️ O CSV da XTB não tem preço unitário, fica a 0
            Total = Math.Abs(t.Amount), // O Amount da XTB costuma vir negativo nas compras
            Source = BrokerSource.XTB,
            TransactionDate = t.TransactionDate
        });

        // 4. Mapear Trading 212 para o DTO comum
        var t212Dtos = t212Transactions.Select(t => new TransactionDto
        {
            Id = t.T212Id, // O ID da T212 já é uma string
            Symbol = t.Ticker,
            Type = t.Action.ToUpperInvariant(), // Ex: "MARKET BUY", "LIMIT SELL"
            Quantity = t.NumberOfShares, // A T212 tem a quantidade exata!
            Price = t.PricePerShare,    // A T212 tem o preço unitário!
            Total = Math.Abs(t.Total),  // Garantir que fica positivo
            Source = BrokerSource.Trading212,
            TransactionDate = t.Time
        });

        // 5. JUNTAR TUDO e ordenar por data (da mais recente para a mais antiga)
        var fullHistory = xtbDtos
            .Concat(t212Dtos)
            .OrderByDescending(t => t.TransactionDate)
            .ToList();

        return fullHistory;
    }

    public async Task<int> ImportXtbExcelAsync(
        Stream xlsxStream,
        CancellationToken cancellationToken = default)
    {
        var workbook = await xtbExcelImporter.ParseAsync(
            xlsxStream,
            cancellationToken);

        var existingOperationIds = await dbContext.XtbCashOperations
            .Select(x => x.OperationId)
            .ToHashSetAsync(cancellationToken);

        var existingPositionIds = await dbContext.XtbClosedPositions
            .Select(x => x.PositionId)
            .ToHashSetAsync(cancellationToken);

        var existingInstrumentPositions = await dbContext.XtbOpenPositions
            .Select(x => x.InstrumentPosition)
            .ToHashSetAsync(cancellationToken);

        var newCashOperations = workbook.CashOperations
            .Where(x => !existingOperationIds.Contains(x.OperationId))
            .ToList();

        var newClosedPositions = workbook.ClosedPositions
            .Where(x => !existingPositionIds.Contains(x.PositionId))
            .ToList();

        var newOpenPositions = workbook.OpenPositions
            .Where(x => !existingInstrumentPositions.Contains(x.InstrumentPosition))
            .ToList();


        foreach (var operation in newCashOperations)
            operation.CreatedAt = DateTime.UtcNow;

        foreach (var position in newClosedPositions)
            position.CreatedAt = DateTime.UtcNow;

        await dbContext.XtbCashOperations.AddRangeAsync(
            newCashOperations,
            cancellationToken);

        await dbContext.XtbClosedPositions.AddRangeAsync(
            newClosedPositions,
            cancellationToken);

        foreach (var position in newOpenPositions)
            position.CreatedAt = DateTime.UtcNow;

        await dbContext.XtbOpenPositions.AddRangeAsync(newOpenPositions, cancellationToken);


        var importedCount = await dbContext.SaveChangesAsync(
            cancellationToken);
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Successfully imported {Count} new records from XTB Excel",
                importedCount);
        }

        return importedCount;
    }

    public async Task<int> ImportTrading212StatementAsync(
        Trading212StatementImportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.SourceDocumentId) ||
            request.SourceDocumentId.Length > 128)
        {
            throw new ArgumentException(
                "sourceDocumentId must contain between 1 and 128 characters.",
                nameof(request));
        }

        var recordCount = CountTrading212Records(request.Data);
        var dataJson = request.Data.GetRawText();
        var statement = await dbContext.Trading212PdfStatements
            .SingleOrDefaultAsync(
                item => item.SourceDocumentId == request.SourceDocumentId,
                cancellationToken);

        if (statement is null)
        {
            statement = new Trading212PdfStatement
            {
                SourceDocumentId = request.SourceDocumentId,
                DataJson = dataJson,
                ImportedAtUtc = DateTime.UtcNow
            };
            dbContext.Trading212PdfStatements.Add(statement);
        }
        else
        {
            statement.DataJson = dataJson;
            statement.ImportedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Persisted Trading 212 statement {SourceDocumentId} with {RecordCount} extracted records.",
            request.SourceDocumentId,
            recordCount);

        return recordCount;
    }

    private static int CountTrading212Records(JsonElement data)
    {
        string[] requiredSections =
        [
            "investTransactions",
            "investPositions",
            "investCashTransactions",
            "dividends",
            "cryptoTransactions",
            "cryptoPositions",
            "cryptoCashTransactions"
        ];

        if (data.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("data must be a JSON object.");
        }

        var recordCount = 0;
        foreach (var section in requiredSections)
        {
            if (!data.TryGetProperty(section, out var tables) ||
                tables.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException(
                    $"data must contain the '{section}' array.");
            }

            foreach (var table in tables.EnumerateArray())
            {
                if (table.ValueKind != JsonValueKind.Object ||
                    !table.TryGetProperty("records", out var records) ||
                    records.ValueKind != JsonValueKind.Array)
                {
                    throw new ArgumentException(
                        $"Every table in '{section}' must contain a records array.");
                }

                if (records.EnumerateArray().Any(record =>
                        record.ValueKind != JsonValueKind.Object))
                {
                    throw new ArgumentException(
                        $"Every record in '{section}' must be a JSON object.");
                }

                recordCount += records.GetArrayLength();
            }
        }

        return recordCount;
    }

    private async Task<List<Position>> CalculateTrading212PositionsAsync(
    CancellationToken cancellationToken)
    {
        var transactions = await dbContext.Trading212Transactions
            .AsNoTracking()
            .Where(t =>
                t.Action == "Market buy" ||
                t.Action == "Market sell")
            .OrderBy(t => t.Time)
            .ToListAsync(cancellationToken);

        var positions = new List<Position>();

        foreach (var group in transactions.GroupBy(t => new { t.Ticker, t.Isin }))
        {
            decimal quantity = 0;
            decimal totalCost = 0;

            foreach (var transaction in group)
            {
                if (transaction.Action == "Market buy")
                {
                    quantity += transaction.NumberOfShares;
                    totalCost += transaction.Total;
                }
                else if (transaction.Action == "Market sell")
                {
                    var averageCost = quantity > 0
                        ? totalCost / quantity
                        : 0;

                    quantity -= transaction.NumberOfShares;
                    totalCost -=
                        transaction.NumberOfShares *
                        averageCost;
                }
            }

            if (quantity <= 0 || string.IsNullOrWhiteSpace(group.Key.Ticker))
                continue;

            positions.Add(new Position
            {
                Symbol = group.Key.Ticker,
                Isin = group.Key.Isin,
                Source = BrokerSource.Trading212,
                Quantity = quantity,
                AverageCost = totalCost / quantity,
                CurrentPrice = 0,
                LastUpdated = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        return positions;
    }

    private async Task<List<Position>> CalculateTrading212CryptoPositionsAsync(
    CancellationToken cancellationToken)
    {
        var transactions = await dbContext.Trading212CryptoTransactions
            .AsNoTracking()
            .OrderBy(t => t.Time)
            .ToListAsync(cancellationToken);

        var positions = new List<Position>();

        foreach (var group in transactions.GroupBy(t => t.AssetSymbol))
        {
            decimal quantity = 0;
            decimal totalCost = 0;

            foreach (var transaction in group)
            {
                if (transaction.Direction.Equals("Buy", StringComparison.OrdinalIgnoreCase))
                {
                    quantity += transaction.Quantity;
                    totalCost += transaction.Quantity * transaction.ExecutionPrice;
                }
                else if (transaction.Direction.Equals("Sell", StringComparison.OrdinalIgnoreCase))
                {
                    var averageCost = quantity > 0
                        ? totalCost / quantity
                        : 0;

                    quantity -= transaction.Quantity;
                    totalCost -= transaction.Quantity * averageCost;
                }
            }

            if (quantity <= 0 || string.IsNullOrWhiteSpace(group.Key))
                continue;

            positions.Add(new Position
            {
                Symbol = group.Key,
                Source = BrokerSource.Trading212,
                AssetClass = AssetClass.Crypto,
                Quantity = quantity,
                AverageCost = totalCost / quantity,
                CurrentPrice = 0,
                LastUpdated = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        return positions;
    }

    private async Task<List<Position>> CalculateXtbPositionsAsync(CancellationToken cancellationToken)
    {
        var positions = await dbContext.XtbOpenPositions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return [.. positions
            .Where(p => !string.IsNullOrWhiteSpace(p.Ticker))
            .GroupBy(p => p.Ticker)
            .Select(group => new Position
            {
                Symbol = group.Key,
                Source = BrokerSource.XTB,
                Quantity = group.Sum(p => p.Volume),
                AverageCost = group.Sum(p => p.Volume * p.OpenPrice) /
                              group.Sum(p => p.Volume),
                CurrentPrice = group.First().CurrentPrice,
                LastUpdated = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            })];
    }
}