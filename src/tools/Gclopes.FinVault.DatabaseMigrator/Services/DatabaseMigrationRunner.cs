using Gclopes.FinVault.DatabaseMigrator.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gclopes.FinVault.DatabaseMigrator.Services;

/// <summary>
/// Coordinates database migrations across FinVault microservices.
/// </summary>
public sealed class DatabaseMigrationRunner(
    IEnumerable<IDatabaseMigration> migrations,
    ILogger<DatabaseMigrationRunner> logger)
{
    /// <summary>
    /// Executes all registered database migrations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Zero if all migrations succeed; otherwise, one.</returns>
    public async Task<int> RunAsync(
        CancellationToken cancellationToken = default)
    {
        var failures = 0;

        foreach (var migration in migrations)
        {
            try
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Starting database migration for {ServiceName}.",
                        migration.ServiceName);
                }

                await migration.MigrateAsync(cancellationToken);

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Database migration completed for {ServiceName}.",
                        migration.ServiceName);
                }
            }
            catch (Exception exception)
            {
                failures++;

                logger.LogError(
                    exception,
                    "Database migration failed for {ServiceName}.",
                    migration.ServiceName);
            }
        }

        return failures == 0 ? 0 : 1;
    }
}