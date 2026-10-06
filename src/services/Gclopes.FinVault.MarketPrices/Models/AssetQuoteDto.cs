namespace Gclopes.FinVault.MarketPrices.Models
{
    public record AssetQuoteDto(string Symbol, decimal Price, string Currency, DateTime Timestamp);
}
