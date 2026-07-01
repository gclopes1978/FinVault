using Gclopes.FinVault.Investments.Data;
using Gclopes.FinVault.Investments.Data.Entities;
using Gclopes.FinVault.Investments.DTO;
using Gclopes.FinVault.Investments.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.Investments.Services.Implementations;

public class InvestmentsService(
    FinVaultInvestmentsDb dbContext,
    IXtbCsvImporter xtbCsvImporter,
    ITrading212CsvImporter trading212CsvImporter,
    ILogger<InvestmentsService> logger) : IInvestmentsService
{
    private readonly FinVaultInvestmentsDb _dbContext = dbContext;
    private readonly IXtbCsvImporter _xtbCsvImporter = xtbCsvImporter;
    private readonly ITrading212CsvImporter _trading212CsvImporter = trading212CsvImporter;
    private readonly ILogger<InvestmentsService> _logger = logger;

    public async Task<List<PortfolioDto>> GetPortfolioAsync(CancellationToken cancellationToken = default)
    {
        var positions = await _dbContext.Positions
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
        var xtbTransactions = await _dbContext.XtbTransactions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // 2. Buscar transações da Trading 212 (Tabela Raw)
        var t212Transactions = await _dbContext.Trading212Transactions
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
        var records = await _xtbCsvImporter.ParseCsvAsync(csvStream, cancellationToken);
        int importedCount = 0;

        // 1. Buscar todos os IDs que já existem na NOVA tabela XtbTransactions
        var existingXtbIds = await _dbContext.XtbTransactions
            .Select(t => t.Id)
            .ToHashSetAsync(cancellationToken);

        foreach (var record in records)
        {
            // 2. DEDUPLICAÇÃO: Se o ID do CSV já existe na BD, saltamos a linha
            if (existingXtbIds.Contains(record.Id))
                continue;

            // 3. GUARDAR NA TABELA DE RAW DATA (XtbTransaction)
            var xtbTransaction = new XtbTransaction
            {
                Id = record.Id, // O ID que vem do CSV da XTB
                Type = record.Type,
                Ticker = record.Ticker,
                Instrument = record.Instrument,
                TransactionDate = record.TransactionDate,
                Amount = record.Amount,
                Comment = record.Comment,
                Product = record.Product,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.XtbTransactions.Add(xtbTransaction);

            // Adicionamos ao HashSet em memória para o caso de o CSV ter o mesmo ID repetido nas linhas
            existingXtbIds.Add(record.Id);

            // 4. ATUALIZAR A TABELA POSITION (O teu Dashboard)
            var position = await _dbContext.Positions
                .FirstOrDefaultAsync(p => p.Symbol == record.Ticker && p.Source == BrokerSource.XTB, cancellationToken);

            if (position == null)
            {
                position = new Position
                {
                    Symbol = record.Ticker,
                    Source = BrokerSource.XTB,
                    // ATENÇÃO: O teu CSV da XTB não tem "Quantidade" nem "Preço Unitário". 
                    // Fica a 0 por agora. Só poderás preencher isto se importares outro ficheiro da XTB que tenha esses dados.
                    Quantity = 0,
                    AverageCost = 0,
                    CurrentPrice = 0,
                    LastUpdated = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Positions.Add(position);
            }
            else
            {
                // Se a posição já existe, apenas atualizamos a data para saber que foi sincronizada recentemente
                position.LastUpdated = DateTime.UtcNow;
            }

            importedCount++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully imported {Count} NEW transactions from XTB CSV", importedCount);

        return importedCount;
    }

    public async Task<int> ImportTrading212CsvAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        var records = await _trading212CsvImporter.ParseCsvAsync(csvStream, cancellationToken);
        int importedCount = 0;

        HashSet<string> existingT212Ids = await _dbContext.Trading212Transactions
            .Select(t => t.T212Id)
            .ToHashSetAsync(cancellationToken);

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
            _dbContext.Trading212Transactions.Add(t212Transaction);

            existingT212Ids.Add(record.Id);

            var position = await _dbContext.Positions
                .FirstOrDefaultAsync(p => p.Symbol == record.Ticker && p.Source == BrokerSource.Trading212, cancellationToken);

            if (position == null)
            {
                position = new Position
                {
                    Symbol = record.Ticker,
                    Source = BrokerSource.Trading212,
                    Quantity = record.NumberOfShares,
                    CurrentPrice = record.PricePerShare,
                    AverageCost = record.PricePerShare,
                    LastUpdated = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Positions.Add(position);
            }
            else
            {
                // ATENÇÃO À LÓGICA AQUI:
                // Se estás a importar um histórico antigo, NÃO deves sobrescrever a Quantity atual.
                // Se estás a importar o estado mais recente, deves sobrescrever.
                // Por segurança (para não estragar o teu portfolio atual com imports de 2022), 
                // apenas atualizamos a data da última sincronização, mantendo os dados que lá estão.
                position.LastUpdated = DateTime.UtcNow;

                // Se quiseres que o import substitua SEMPRE o estado atual, descomenta as 3 linhas abaixo:
                // position.Quantity = record.NumberOfShares;
                // position.CurrentPrice = record.PricePerShare;
                // position.AverageCost = record.PricePerShare; 
            }

            importedCount++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully imported {Count} NEW transactions from Trading212 CSV", importedCount);

        return importedCount;
    }
    
}