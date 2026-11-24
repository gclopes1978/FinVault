using Gclopes.FinVault.DatabaseMigrator.Extensions;
using Gclopes.FinVault.DatabaseMigrator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gclopes.FinVault.DatabaseMigrator;

/// <summary>
/// Main entry point for the database migration application.
/// </summary>
public static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>The application exit code.</returns>
    public static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddFinVaultDatabaseMigrator();

        using var host = builder.Build();

        var runner = host.Services
            .GetRequiredService<DatabaseMigrationRunner>();

        return await runner.RunAsync();
    }
}