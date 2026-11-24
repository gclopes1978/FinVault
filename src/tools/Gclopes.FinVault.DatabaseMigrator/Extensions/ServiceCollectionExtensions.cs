using Gclopes.FinVault.DatabaseMigrator.Interfaces;
using Gclopes.FinVault.DatabaseMigrator.Migrations;
using Gclopes.FinVault.DatabaseMigrator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gclopes.FinVault.DatabaseMigrator.Extensions;

/// <summary>
/// Provides dependency injection extensions for the database migrator.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers FinVault database migration services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddFinVaultDatabaseMigrator(
        this IServiceCollection services)
    {
        services.AddTransient<IDatabaseMigration, IdentityDatabaseMigration>();
        services.AddTransient<IDatabaseMigration, CurrentAccountDatabaseMigration>();
        services.AddTransient<IDatabaseMigration, InvestmentsDatabaseMigration>();
        services.AddTransient<IDatabaseMigration, PensionPlansDatabaseMigration>();
        services.AddTransient<IDatabaseMigration, SavingsCertificatesDatabaseMigration>();
        services.AddTransient<IDatabaseMigration, DocumentIngestionDatabaseMigration>();

        services.AddTransient<DatabaseMigrationRunner>();

        return services;
    }
}