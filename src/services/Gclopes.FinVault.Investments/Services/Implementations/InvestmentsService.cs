using Gclopes.FinVault.Investments.Data;
using Gclopes.FinVault.Investments.Data.Entities;
using Gclopes.FinVault.Investments.DTO;
using Gclopes.FinVault.Investments.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.Investments.Services.Implementations;

public class InvestmentsService(
    FinVaultInvestmentsDb dbContext,
    IXtbCsvImporter xtbCsvImporter,
    IXtbExcelImporter xtbExcelImporter,
    ITrading212CsvImporter trading212CsvImporter,
    ILogger<InvestmentsService> logger) : IInvestmentsService
{

    public async Task<List<PortfolioDto>> GetPortfolioAsync(CancellationToken cancellationToken = default)
    {
        var positions = await dbContext.Positions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return [.. positions.Select(p => new PortfolioDto
        {
            Id = p.Id,
            Symbol = p.Symbol,
            Source = p.Source.ToString(),
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

    public async Task<int> ImportXtbCsvAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        var records = await xtbCsvImporter.ParseCsvAsync(csvStream, cancellationToken);
        int importedCount = 0;

        // 1. Buscar todos os IDs que já existem na tabela XtbTransactions
        var existingXtbIds = await dbContext.XtbTransactions
            .Select(t => t.Id)
            .ToHashSetAsync(cancellationToken);

        // 2. Pré-carregar todas as Positions existentes da XTB para um dicionário em memória
        var existingPositions = await dbContext.Positions
            .Where(p => p.Source == BrokerSource.XTB)
            .ToDictionaryAsync(p => p.Symbol, cancellationToken);

        foreach (var record in records)
        {
            if (existingXtbIds.Contains(record.Id))
                continue;

            var xtbTransaction = new XtbTransaction
            {
                Id = record.Id,
                Type = record.Type,
                Ticker = record.Ticker,
                Instrument = record.Instrument,
                TransactionDate = record.TransactionDate,
                Amount = record.Amount,
                Comment = record.Comment,
                Product = record.Product,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.XtbTransactions.Add(xtbTransaction);
            existingXtbIds.Add(record.Id);

            // 3. Consultar o dicionário em memória em vez da BD
            if (!existingPositions.TryGetValue(record.Ticker, out var position))
            {
                position = new Position
                {
                    Symbol = record.Ticker,
                    Source = BrokerSource.XTB,
                    Quantity = 0,
                    AverageCost = 0,
                    CurrentPrice = 0,
                    LastUpdated = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Positions.Add(position);
                existingPositions[record.Ticker] = position; // regista já no dicionário
            }
            else
            {
                position.LastUpdated = DateTime.UtcNow;
            }

            importedCount++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Successfully imported {Count} NEW transactions from XTB CSV", importedCount);

        return importedCount;
    }

    public async Task<int> ImportTrading212CsvAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        var records = await trading212CsvImporter.ParseCsvAsync(csvStream, cancellationToken);
        int importedCount = 0;

        HashSet<string> existingT212Ids = await dbContext.Trading212Transactions
            .Select(t => t.T212Id)
            .ToHashSetAsync(cancellationToken);

        var existingPositions = await dbContext.Positions
            .Where(p => p.Source == BrokerSource.Trading212)
            .ToDictionaryAsync(p => p.Symbol, cancellationToken);
        foreach (Trading212CsvRecord record in records)
        {
            if (existingT212Ids.Contains(record.Id))
                continue;

            var t212Transaction = new Trading212Transaction
            {
                T212Id = record.Id,
                Action = record.Action,
                Time = record.Time,
                Isin = record.Isin,
                Ticker = record.Ticker,
                Name = record.Name,
                Notes = record.Notes,
                NumberOfShares = record.NumberOfShares,
                PricePerShare = record.PricePerShare,
                CurrencyPricePerShare = record.CurrencyPricePerShare,
                ExchangeRate = record.ExchangeRate,
                Total = record.Total,
                CurrencyTotal = record.CurrencyTotal,
                WithholdingTax = record.WithholdingTax,
                CurrencyWithholdingTax = record.CurrencyWithholdingTax,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Trading212Transactions.Add(t212Transaction);

            if (!existingPositions.TryGetValue(record.Ticker, out var position))
            {
                position = new Position
                {
                    Symbol = record.Ticker,
                    Source = BrokerSource.Trading212,
                    Quantity = 0,
                    AverageCost = 0,
                    CurrentPrice = 0,
                    LastUpdated = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Positions.Add(position);
                existingPositions[record.Ticker] = position; // regista já no dicionário
            }
            else
            {
                position.LastUpdated = DateTime.UtcNow;
            }

            importedCount++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Successfully imported {Count} NEW transactions from Trading212 CSV", importedCount);

        return importedCount;
    }

    public async Task<int> ImportXtbExcelAsync(Stream xlsxStream, CancellationToken cancellationToken = default)
    {
        var workbook = await xtbExcelImporter.ParseAsync(
            xlsxStream,
            cancellationToken);

        await dbContext.XtbClosedPositions.AddRangeAsync(
            workbook.ClosedPositions,
            cancellationToken);

        await dbContext.XtbCashOperations.AddRangeAsync(
            workbook.CashOperations,
            cancellationToken);

        await dbContext.XtbOpenPositions.AddRangeAsync(
            workbook.OpenPositions,
            cancellationToken);


        var importedCount = await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully imported {Count} records from XTB Excel", importedCount);

        return importedCount;
    }
}