using Gclopes.FinVault.MarketPrices.Interfaces;

namespace Gclopes.FinVault.MarketPrices.Services;

public class MarketPriceService(IMarketPriceResolver resolver) : IMarketPriceService
{
    private readonly IMarketPriceResolver _resolver = resolver;

    public async Task<string> GetRawAsync(string assetType, string symbol, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Getting raw data for {assetType} - {symbol}");

        // 1. Resolver ISIN ou ticker incompleto
        var resolvedSymbol = (assetType.Equals("crypto", StringComparison.CurrentCultureIgnoreCase) || symbol.Contains('.')
            ? symbol
            : await _resolver.ResolveTickerFromIsinAsync(symbol)) ?? throw new Exception($"Unable to resolve ticker for symbol/ISIN '{symbol}'");

        // 2. Obter provider correto (Yahoo / Crypto)
        var provider = _resolver.GetProvider(assetType);

        // 3. Chamar o provider com o ticker final (ex.: 4GLD.DE)
        return await provider.GetRawAsync(resolvedSymbol, cancellationToken);
    }
}
