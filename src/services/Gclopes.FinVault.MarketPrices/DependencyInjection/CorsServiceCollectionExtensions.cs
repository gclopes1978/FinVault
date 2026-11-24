namespace Gclopes.FinVault.MarketPrices.DependencyInjection;

/// <summary>
/// Provides CORS service registration extensions.
/// </summary>
public static class CorsServiceCollectionExtensions
{
    private const string PolicyName = "AllowAll";

    /// <summary>
    /// Registers the CORS policy for the MarketPrices service.
    /// </summary>
    public static IServiceCollection AddMarketPricesCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? throw new InvalidOperationException(
                "Cors:AllowedOrigins configuration is missing.");

        if (allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException(
                "At least one CORS allowed origin must be configured.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }

    /// <summary>
    /// Gets the configured CORS policy name.
    /// </summary>
    public static string GetMarketPricesCorsPolicyName() => PolicyName;
}