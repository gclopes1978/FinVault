using Gclopes.FinVault.MarketPrices.Interfaces;
using Gclopes.FinVault.MarketPrices.Providers;
using Gclopes.FinVault.MarketPrices.Resolver;
using Gclopes.FinVault.MarketPrices.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gclopes.FinVault.MarketPrices.DependencyInjection;

/// <summary>
/// Provides application service registration extensions.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers application services, providers, resolver and health checks.
    /// </summary>
    public static IServiceCollection AddMarketPricesApplication(
        this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient("Yahoo");

            return new YahooFinancePriceProvider(client);
        });

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient("Coingecko");

            return new CoingeckoPriceProvider(client);
        });

        services.AddSingleton<IMarketPriceResolver>(sp =>
        {
            var yahoo = sp.GetRequiredService<YahooFinancePriceProvider>();
            var crypto = sp.GetRequiredService<CoingeckoPriceProvider>();
            var factory = sp.GetRequiredService<IHttpClientFactory>();

            return new MarketPriceResolver(yahoo, crypto, factory);
        });

        services.AddScoped<IMarketPriceService, MarketPriceService>();

        services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy("Service is running"));

        return services;
    }
}