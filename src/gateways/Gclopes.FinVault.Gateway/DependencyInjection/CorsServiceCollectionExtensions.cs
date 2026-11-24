namespace Gclopes.FinVault.Gateway.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring CORS services.
/// </summary>
public static class CorsServiceCollectionExtensions
{
    private const string PolicyName = "AngularFrontendPolicy";

    /// <summary>
    /// Registers the Gateway CORS policy.
    /// </summary>
    public static IServiceCollection AddGatewayCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var frontendOrigin = configuration["Cors:FrontendOrigin"]
            ?? throw new InvalidOperationException(
                "Cors:FrontendOrigin configuration is missing.");

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(frontendOrigin)
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }

    /// <summary>
    /// Gets the configured CORS policy name.
    /// </summary>
    public static string GetGatewayCorsPolicyName()
    {
        return PolicyName;
    }
}