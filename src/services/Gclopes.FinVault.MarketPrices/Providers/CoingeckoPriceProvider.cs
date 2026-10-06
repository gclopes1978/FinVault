using Gclopes.FinVault.MarketPrices.Interfaces;
using Gclopes.FinVault.MarketPrices.Models;

namespace Gclopes.FinVault.MarketPrices.Providers;

public class CoingeckoPriceProvider(HttpClient http) : IMarketPriceProvider
{
    public Task<AssetQuoteDto> GetQuoteAsync(string symbol, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<string> GetRawAsync(string symbol, CancellationToken cancellationToken)
    {
        var url = $"simple/price?ids={symbol}&vs_currencies=eur";
        Console.WriteLine($"Fetching market price for {symbol} from Coingecko API...");
        Console.WriteLine($"Request URL: {url}");
        var response = await http.GetAsync(url, cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}

