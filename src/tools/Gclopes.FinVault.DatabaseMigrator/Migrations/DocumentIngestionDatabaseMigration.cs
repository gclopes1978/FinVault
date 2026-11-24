using Gclopes.FinVault.DatabaseMigrator.Interfaces;
using Gclopes.FinVault.DocumentIngestion.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Gclopes.FinVault.DatabaseMigrator.Migrations;

/// <summary>
/// Applies database migrations for the DocumentIngestion microservice.
/// </summary>
public sealed class DocumentIngestionDatabaseMigration(
    IConfiguration configuration) : IDatabaseMigration
{
    /// <inheritdoc />
    public string ServiceName => "DocumentIngestion";

    /// <inheritdoc />
    public async Task MigrateAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            configuration.GetConnectionString("DocumentIngestion");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DocumentIngestion was not configured.");
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var dbContext = new ApplicationDbContext(options);

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}