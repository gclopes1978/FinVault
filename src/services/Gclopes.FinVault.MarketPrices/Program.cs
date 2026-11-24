using Gclopes.FinVault.MarketPrices.DependencyInjection;

namespace Gclopes.FinVault.MarketPrices;

/// <summary>
/// Main entry point for the MarketPrices microservice.
/// </summary>
public class Program
{
    protected Program()
    {
    }

    /// <summary>
    /// Application entry point.
    /// </summary>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddMarketPricesSwagger()
            .AddMarketPricesCors(builder.Configuration)
            .AddMarketPricesAuthentication(builder.Configuration)
            .AddMarketPricesHttpClients(builder.Configuration)
            .AddMarketPricesApplication();

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var app = builder.Build();

        app.UseMarketPricesPipeline();

        await app.RunAsync();
    }
}