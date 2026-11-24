using Gclopes.FinVault.Gateway.DependencyInjection;

namespace Gclopes.FinVault.Gateway;

/// <summary>
/// Main entry point for the FinVault Gateway.
/// </summary>
public static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddGatewayCors(builder.Configuration)
            .AddGatewaySwagger()
            .AddGatewayAuthentication(builder.Configuration)
            .AddGatewayReverseProxy(builder.Configuration)
            .AddGatewayResilience();

        var app = builder.Build();

        app.UseGatewayPipeline();

        app.Run();
    }
}