namespace Gclopes.FinVault.CurrentAccount.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring CORS.
/// </summary>
public static class CorsServiceCollectionExtensions
{
    private const string PolicyName = "Default";

    /// <summary>
    /// Registers the CurrentAccount CORS policy.
    /// </summary>
    public static IServiceCollection AddCurrentAccountCors(
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