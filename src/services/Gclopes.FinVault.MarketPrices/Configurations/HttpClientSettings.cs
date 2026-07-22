namespace Gclopes.FinVault.MarketPrices.Configurations
{
    public class HttpClientSettings
    {
        public required string BaseUrl { get; set; } = string.Empty;
        public required string UserAgent { get; set; } = string.Empty;
    }
}
