namespace Gclopes.FinVault.DatabaseMigrator.Interfaces;

/// <summary>
/// Defines the contract for applying database migrations for a microservice.
/// </summary>
public interface IDatabaseMigration
{
    /// <summary>
    /// Gets the name of the microservice.
    /// </summary>
    string ServiceName { get; }

    /// <summary>
    /// Applies pending database migrations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task MigrateAsync(CancellationToken cancellationToken = default);
}