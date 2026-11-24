using Gclopes.FinVault.DatabaseMigrator.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Gclopes.FinVault.DatabaseMigrator.Migrations;

/// <summary>
/// Applies database migrations for the CurrentAccount microservice.
/// </summary>
public sealed class CurrentAccountDatabaseMigration(
    IConfiguration configuration) : IDatabaseMigration
{
    /// <inheritdoc />
    public string ServiceName => "CurrentAccount";

    /// <inheritdoc />
    public async Task MigrateAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            configuration.GetConnectionString("CurrentAccount");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:CurrentAccount was not configured.");
        }

        var options = new DbContextOptionsBuilder<CurrentAccount.Data.ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var dbContext = new CurrentAccount.Data.ApplicationDbContext(options);

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}