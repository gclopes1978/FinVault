namespace Gclopes.FinVault.MarketPrices.Configurations;

public class PollyOptions
{
    public required int RetryCount { get; set; } 
    public required int RetryBaseDelaySeconds { get; set; } 
    public required CircuitBreakerOptions CircuitBreaker { get; set; } = new()
    {
        ExceptionsAllowedBeforeBreaking = 0,
        DurationOfBreakSeconds = 0
    };
    public required int TimeoutSeconds { get; set; }
}
