namespace Gclopes.FinVault.MarketPrices.Interfaces;

public interface IMarketPriceProvider
{
    Task<string> GetRawAsync(string symbol, CancellationToken cancellationToken);
}
