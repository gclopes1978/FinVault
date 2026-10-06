using Gclopes.FinVault.MarketPrices.Models;

namespace Gclopes.FinVault.MarketPrices.Interfaces;

public interface IMarketPriceService
{
    Task<AssetQuoteDto> GetQuoteAsync(
        string assetType,
        string symbol,
        CancellationToken cancellationToken);

    Task<decimal> GetExchangeRateAsync(
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken);
}