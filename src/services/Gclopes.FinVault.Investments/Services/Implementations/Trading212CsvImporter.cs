using Gclopes.FinVault.Investments.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace Gclopes.FinVault.Investments.Services.Implementations;

public class Trading212CsvImporter(ILogger<Trading212CsvImporter> logger) : ITrading212CsvImporter
{
    public async Task<List<Trading212CsvRecord>> ParseCsvAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        var records = new List<Trading212CsvRecord>();

        using var reader = new StreamReader(csvStream, Encoding.UTF8);

        // A T212 costuma ter o header limpo na primeira linha, mas se tiveres lixo no topo, 
        // podes adicionar um while aqui como fizemos na XTB. Assumindo que a 1ª linha é o header:
        string? headerLine = await reader.ReadLineAsync(cancellationToken);
        if (headerLine == null) return records;

        var headers = SplitCsvLine(headerLine);

        // Mapear os índices das colunas
        var actionIdx = Array.IndexOf(headers, "Action");
        var timeIdx = Array.IndexOf(headers, "Time");
        var isinIdx = Array.IndexOf(headers, "ISIN");
        var tickerIdx = Array.IndexOf(headers, "Ticker");
        var nameIdx = Array.IndexOf(headers, "Name");
        var notesIdx = Array.IndexOf(headers, "Notes");
        var idIdx = Array.IndexOf(headers, "ID");
        var sharesIdx = Array.IndexOf(headers, "No. of shares");
        var priceIdx = Array.IndexOf(headers, "Price / share");
        var currencyPriceIdx = Array.IndexOf(headers, "Currency (Price / share)");
        var exchangeIdx = Array.IndexOf(headers, "Exchange rate");
        var totalIdx = Array.IndexOf(headers, "Total");
        var currencyTotalIdx = Array.IndexOf(headers, "Currency (Total)");
        var taxIdx = Array.IndexOf(headers, "Withholding tax");
        var currencyTaxIdx = Array.IndexOf(headers, "Currency (Withholding tax)");

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var columns = SplitCsvLine(line);

                // A T212 às vezes deixa linhas em branco no final ou separadores estranhos
                if (columns.Length < 15 || string.IsNullOrEmpty(columns[idIdx])) continue;

                records.Add(new Trading212CsvRecord
                {
                    Action = columns[actionIdx].Trim().Replace("\"", ""),
                    Time = DateTime.Parse(columns[timeIdx].Trim().Replace("\"", ""), CultureInfo.InvariantCulture),
                    Isin = columns[isinIdx].Trim().Replace("\"", ""),
                    Ticker = columns[tickerIdx].Trim().Replace("\"", ""),
                    Name = columns[nameIdx].Trim().Replace("\"", ""),
                    Notes = columns[notesIdx].Trim().Replace("\"", ""),
                    Id = columns[idIdx].Trim().Replace("\"", ""),
                    NumberOfShares = ParseDecimal(columns[sharesIdx]),
                    PricePerShare = ParseDecimal(columns[priceIdx]),
                    CurrencyPricePerShare = columns[currencyPriceIdx].Trim().Replace("\"", ""),
                    ExchangeRate = ParseDecimal(columns[exchangeIdx]),
                    Total = ParseDecimal(columns[totalIdx]),
                    CurrencyTotal = columns[currencyTotalIdx].Trim().Replace("\"", ""),
                    WithholdingTax = ParseDecimal(columns[taxIdx]),
                    CurrencyWithholdingTax = columns[currencyTaxIdx].Trim().Replace("\"", "")
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Erro ao processar linha da T212: {Line}. A saltar.", line);
            }
        }

        return records;
    }

    // MÉTODO AUXILIAR: Resolve o problema de "1,234.56" que o Split(',') estragaria
    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString()); // Adicionar o último campo
        return [.. result];
    }

    // MÉTODO AUXILIAR: Limpa as aspas e vírgulas de milhares antes de converter para decimal
    private static decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;

        // Remove aspas e vírgulas de milhar (ex: "1,234.56" -> "1234.56")
        var cleaned = value.Trim().Replace("\"", "").Replace(",", "");
        return decimal.Parse(cleaned, CultureInfo.InvariantCulture);
    }
}