using Gclopes.FinVault.MarketPrices.Interfaces;

namespace Gclopes.FinVault.MarketPrices.Providers;

public class CoingeckoPriceProvider(HttpClient http) : IMarketPriceProvider
{
    public async Task<string> GetRawAsync(string symbol, CancellationToken cancellationToken)
    {
        var url = $"simple/price?ids={symbol}&vs_currencies=eur";
        Console.WriteLine($"Fetching market price for {symbol} from Coingecko API...");
        Console.WriteLine($"Request URL: {url}");
        var response = await http.GetAsync(url, cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}

