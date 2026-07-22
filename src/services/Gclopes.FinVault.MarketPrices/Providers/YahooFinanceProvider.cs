using Gclopes.FinVault.MarketPrices.Interfaces;

namespace Gclopes.FinVault.MarketPrices.Providers;

public class YahooFinancePriceProvider(HttpClient http) : IMarketPriceProvider
{
    public async Task<string> GetRawAsync(string symbol, CancellationToken cancellationToken)
    {
        var url = $"{symbol}?interval=1d&range=1d";
        Console.WriteLine($"Fetching data from Yahoo Finance API: {url}");
        var response = await http.GetAsync(url, cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string> GetTicker(string symbol, CancellationToken cancellationToken)
    {
        var url = $"{symbol}?interval=1d&range=1d";

        var response = await http.GetAsync(url, cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

}
