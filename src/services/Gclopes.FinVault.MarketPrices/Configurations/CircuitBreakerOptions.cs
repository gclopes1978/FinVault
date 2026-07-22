namespace Gclopes.FinVault.MarketPrices.Configurations
{
    public class CircuitBreakerOptions
    {
        public required int ExceptionsAllowedBeforeBreaking { get; set; }
        public required int DurationOfBreakSeconds { get; set; } 
    }
}
