using Gclopes.FinVault.MarketPrices.Models;

namespace Gclopes.FinVault.MarketPrices.Interfaces;

public interface IMarketPriceProvider
{
    Task<AssetQuoteDto> GetQuoteAsync(
        string symbol,
        CancellationToken cancellationToken);
}