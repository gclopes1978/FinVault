namespace Gclopes.FinVault.MarketPrices.Interfaces;

public interface IMarketPriceService
{
    Task<string> GetRawAsync(string assetType, string symbol, CancellationToken cancellationToken);
}
