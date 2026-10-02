using System.Text.Json;
using Gclopes.FinVault.MarketPrices.Interfaces;
using Gclopes.FinVault.MarketPrices.Models;

namespace Gclopes.FinVault.MarketPrices.Providers;

public class YahooFinancePriceProvider(HttpClient http) : IMarketPriceProvider
{
    public async Task<AssetQuoteDto> GetQuoteAsync(
        string symbol,
        CancellationToken cancellationToken)
    {
        var url = $"{symbol}?interval=1d&range=1d";

        Console.WriteLine(
            $"Fetching data from Yahoo Finance API: {url}");

        var response = await http.GetAsync(
            url,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(
            cancellationToken);

        using var document = JsonDocument.Parse(json);

        var result = document.RootElement
            .GetProperty("chart")
            .GetProperty("result")[0];

        var meta = result.GetProperty("meta");

        var resolvedSymbol = meta
            .GetProperty("symbol")
            .GetString() ?? symbol;

        var currency = meta
            .GetProperty("currency")
            .GetString() ?? string.Empty;

        var price = meta
            .GetProperty("regularMarketPrice")
            .GetDecimal();

        var timestamp = result
            .GetProperty("timestamp")[0]
            .GetInt64();

        var dateTime = DateTimeOffset
            .FromUnixTimeSeconds(timestamp)
            .UtcDateTime;

        return new AssetQuoteDto(
            resolvedSymbol,
            price,
            currency,
            dateTime);
    }
}