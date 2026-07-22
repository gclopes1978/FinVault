
using Gclopes.FinVault.MarketPrices.Interfaces;
using System.Text.Json;

namespace Gclopes.FinVault.MarketPrices.Resolver;
public class MarketPriceResolver(
    IMarketPriceProvider yahoo,
    IMarketPriceProvider crypto,
    IHttpClientFactory httpClientFactory) : IMarketPriceResolver
{
    private readonly HttpClient _searchClient = httpClientFactory.CreateClient("YahooFinanceSearch");

    public IMarketPriceProvider GetProvider(string assetType)
    {
        return assetType.ToLower() switch
        {
            "equity" => yahoo,
            "stock" => yahoo,
            "etf" => yahoo,
            "crypto" => crypto,
            _ => yahoo
        };
    }

    public async Task<string?> ResolveTickerFromIsinAsync(string isin)
    {
        var response = await _searchClient.GetAsync($"?q={isin}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var quotes = doc.RootElement.GetProperty("quotes");

        if (quotes.GetArrayLength() == 0)
            return null;

        return quotes[0].GetProperty("symbol").GetString();
    }
}
