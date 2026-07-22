namespace Gclopes.FinVault.MarketPrices.Interfaces;

public interface ICryptoPriceService
{
    Task<decimal> GetCryptoPriceAsync(string symbol, CancellationToken cancellationToken);
}