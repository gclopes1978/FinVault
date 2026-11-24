using Gclopes.FinVault.DatabaseMigrator.Interfaces;
using Gclopes.FinVault.SavingsCertificates.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Gclopes.FinVault.DatabaseMigrator.Migrations;

/// <summary>
/// Applies database migrations for the SavingsCertificates microservice.
/// </summary>
public sealed class SavingsCertificatesDatabaseMigration(
    IConfiguration configuration) : IDatabaseMigration
{
    /// <inheritdoc />
    public string ServiceName => "SavingsCertificates";

    /// <inheritdoc />
    public async Task MigrateAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            configuration.GetConnectionString("SavingsCertificates");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:SavingsCertificates was not configured.");
        }

        var options = new DbContextOptionsBuilder<SavingsBondDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var dbContext = new SavingsBondDbContext(options);

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}