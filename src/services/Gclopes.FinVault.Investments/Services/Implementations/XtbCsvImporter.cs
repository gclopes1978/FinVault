using Gclopes.FinVault.Investments.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace Gclopes.FinVault.Investments.Services.Implementations;

public class XtbCsvImporter(ILogger<XtbCsvImporter> logger) : IXtbCsvImporter
{

    public async Task<List<XtbCsvRecord>> ParseCsvAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        var records = new List<XtbCsvRecord>();

        using var reader = new StreamReader(csvStream, Encoding.UTF8);
        string? line;

        int? typeIdx = null, tickerIdx = null, instrumentIdx = null, timeIdx = null;
        int? amountIdx = null, idIdx = null, commentIdx = null, productIdx = null;

        // 1. SALTAR O LIXO: Ler linhas até encontrar o cabeçalho correto
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // A XTB tem várias secções. Procuramos a linha que começa exatamente com "Type,Ticker"
            if (line.StartsWith("Type,Ticker"))
            {
                var headers = SplitCsvLine(line);
                typeIdx = Array.IndexOf(headers, "Type");
                tickerIdx = Array.IndexOf(headers, "Ticker");
                instrumentIdx = Array.IndexOf(headers, "Instrument");
                timeIdx = Array.IndexOf(headers, "Time");
                amountIdx = Array.IndexOf(headers, "Amount");
                idIdx = Array.IndexOf(headers, "ID");
                commentIdx = Array.IndexOf(headers, "Comment");
                productIdx = Array.IndexOf(headers, "Product");
                break; // Saímos do loop, encontramos o que interessava
            }
        }

        if (typeIdx == null || idIdx == null || amountIdx == null || tickerIdx==null || instrumentIdx==null || timeIdx==null || commentIdx==null || productIdx==null)
        {
            logger.LogError("XTB CSV headers are missing or invalid.");
            throw new InvalidOperationException("Ficheiro CSV da XTB inválido. Não foram encontradas as colunas necessárias.");
        }

        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var columns = SplitCsvLine(line);

                var rawId = columns[idIdx.Value].Trim().Replace("\"", "");
                if (string.IsNullOrEmpty(rawId)) continue;

                records.Add(new XtbCsvRecord
                {
                    Id = int.Parse(rawId, CultureInfo.InvariantCulture),
                    Type = columns[typeIdx.Value].Trim().Replace("\"", ""),
                    Ticker = columns[tickerIdx.Value].Trim().Replace("\"", ""),
                    Instrument = columns[instrumentIdx.Value].Trim().Replace("\"", ""),
                    TransactionDate = DateTime.ParseExact(
                        columns[timeIdx.Value].Trim().Replace("\"", ""),
                        "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.InvariantCulture),
                    Amount = ParseDecimal(columns[amountIdx.Value]),
                    Comment = columns[commentIdx.Value].Trim().Replace("\"", ""),
                    Product = columns[productIdx.Value].Trim().Replace("\"", "")
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao processar linha do CSV XTB: {Line}. A saltar.", line);
            }
        }

        logger.LogInformation("Successfully parsed {Count} records from XTB CSV", records.Count);
        return records;
    }
   
    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in line)
        {
            if (c == '"') inQuotes = !inQuotes;
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
        result.Add(current.ToString());
        return [.. result];
    }

    private static decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        var cleaned = value.Trim().Replace("\"", "").Replace(" ", "");
        return decimal.Parse(cleaned, CultureInfo.InvariantCulture);
    }
}