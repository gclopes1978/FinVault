using Gclopes.FinVault.SyntheticPortfolio.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gclopes.FinVault.SyntheticPortfolio;

/// <summary>
/// Main entry point for the synthetic portfolio application.
/// </summary>
public static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSyntheticPortfolioConfiguration(
            builder.Configuration);

        builder.Services.AddSyntheticPortfolioInfrastructure();

        using var host = builder.Build();

        await host.RunAsync();
    }
}
