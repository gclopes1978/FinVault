using Gclopes.FinVault.MarketPrices.Interfaces;

namespace Gclopes.FinVault.MarketPrices.Services;

public class MarketPriceService(IMarketPriceResolver resolver) : IMarketPriceService
{
    private readonly IMarketPriceResolver _resolver = resolver;

    public Task<string> GetRawAsync(string assetType, string symbol, CancellationToken cancellationToken)
    {
        var provider = _resolver.GetProvider(assetType);
        return provider.GetRawAsync(symbol, cancellationToken);
    }
}
