using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Gclopes.FinVault.Gateway.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring resilience services.
/// </summary>
public static class ResilienceServiceCollectionExtensions
{
    /// <summary>
    /// Registers the default resilience pipeline.
    /// </summary>
    public static IServiceCollection AddGatewayResilience(
        this IServiceCollection services)
    {
        services.AddResiliencePipeline("default", pipeline =>
        {
            pipeline.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2
            });

            pipeline.AddCircuitBreaker(
                new CircuitBreakerStrategyOptions());
        });

        return services;
    }
}