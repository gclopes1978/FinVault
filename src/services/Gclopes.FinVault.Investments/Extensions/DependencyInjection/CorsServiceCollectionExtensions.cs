namespace Gclopes.FinVault.Investments.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring CORS.
/// </summary>
public static class CorsServiceCollectionExtensions
{
    private const string PolicyName = "Frontend";

    /// <summary>
    /// Registers the Investments CORS policy.
    /// </summary>
    public static IServiceCollection AddInvestmentsCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

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
    public static string GetCorsPolicyName() => PolicyName;
}