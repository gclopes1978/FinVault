namespace Gclopes.FinVault.Gateway.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring the reverse proxy.
/// </summary>
public static class ReverseProxyServiceCollectionExtensions
{
    /// <summary>
    /// Registers and configures the YARP reverse proxy.
    /// </summary>
    public static IServiceCollection AddGatewayReverseProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        return services;
    }
}