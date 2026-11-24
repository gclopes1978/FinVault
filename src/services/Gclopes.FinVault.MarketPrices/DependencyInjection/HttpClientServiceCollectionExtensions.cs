using Gclopes.FinVault.MarketPrices.Configurations;
using Polly;
using Polly.Extensions.Http;

namespace Gclopes.FinVault.MarketPrices.DependencyInjection;

/// <summary>
/// Provides HTTP client and resilience service registration extensions.
/// </summary>
public static class HttpClientServiceCollectionExtensions
{
    /// <summary>
    /// Registers external HTTP clients with their configuration and resilience policies.
    /// </summary>
    public static IServiceCollection AddMarketPricesHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var yahooSettings = GetHttpClientSettings(
            configuration, "HttpClients:YahooFinance");

        var yahooSearchSettings = GetHttpClientSettings(
            configuration, "HttpClients:YahooFinanceSearch");

        var coingeckoSettings = GetHttpClientSettings(
            configuration, "HttpClients:Coingecko");

        var pollyOptions = new PollyOptions
        {
            RetryCount = configuration.GetRequiredSection("Polly")
                .GetValue<int>("RetryCount"),

            RetryBaseDelaySeconds = configuration.GetRequiredSection("Polly")
                .GetValue<int>("RetryBaseDelaySeconds"),

            TimeoutSeconds = configuration.GetRequiredSection("Polly")
                .GetValue<int>("TimeoutSeconds"),

            CircuitBreaker = new CircuitBreakerOptions
            {
                ExceptionsAllowedBeforeBreaking = configuration
                    .GetRequiredSection("Polly:CircuitBreaker")
                    .GetValue<int>("ExceptionsAllowedBeforeBreaking"),

                DurationOfBreakSeconds = configuration
                    .GetRequiredSection("Polly:CircuitBreaker")
                    .GetValue<int>("DurationOfBreakSeconds")
            }
        };

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                pollyOptions.RetryCount,
                attempt => TimeSpan.FromSeconds(
                    Math.Pow(pollyOptions.RetryBaseDelaySeconds, attempt)));

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
            pollyOptions.TimeoutSeconds);

        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                pollyOptions.CircuitBreaker.ExceptionsAllowedBeforeBreaking,
                TimeSpan.FromSeconds(
                    pollyOptions.CircuitBreaker.DurationOfBreakSeconds));

        services.AddHttpClient("Yahoo", client =>
        {
            client.BaseAddress = new Uri(yahooSettings.BaseUrl);
            client.DefaultRequestHeaders.Add(
                "User-Agent", yahooSettings.UserAgent);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(timeoutPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        services.AddHttpClient("YahooFinanceSearch", client =>
        {
            client.BaseAddress = new Uri(yahooSearchSettings.BaseUrl);
            client.DefaultRequestHeaders.Add(
                "User-Agent", yahooSearchSettings.UserAgent);
        });

        services.AddHttpClient("Coingecko", client =>
        {
            client.BaseAddress = new Uri(coingeckoSettings.BaseUrl);
            client.DefaultRequestHeaders.Add(
                "User-Agent", coingeckoSettings.UserAgent);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(timeoutPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }

    private static HttpClientSettings GetHttpClientSettings(
        IConfiguration configuration,
        string sectionName)
    {
        return configuration.GetSection(sectionName).Get<HttpClientSettings>()
            ?? throw new InvalidOperationException(
                $"Missing or invalid configuration for {sectionName}.");
    }
}