namespace Gclopes.FinVault.MarketPrices.Interfaces
{
    public interface IMarketPriceResolver
    {
        IMarketPriceProvider GetProvider(string assetType);
        Task<string?> ResolveTickerFromIsinAsync(string isin);
    }

}
