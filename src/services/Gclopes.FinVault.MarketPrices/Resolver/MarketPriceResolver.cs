using Gclopes.FinVault.MarketPrices.Interfaces;

namespace Gclopes.FinVault.MarketPrices.Resolver
{
    public class MarketPriceResolver(
        IMarketPriceProvider yahoo,
        IMarketPriceProvider crypto) : IMarketPriceResolver
    {
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

        
    }

}
