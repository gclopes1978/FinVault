namespace Gclopes.FinVault.MarketPrices.Interfaces
{
    public interface IMarketPriceResolver
    {
        IMarketPriceProvider GetProvider(string assetType);
        public string? ResolveYahooSymbol(string symbol);
        Task<string?> ResolveTickerFromIsinAsync(string isin);
    }

}
