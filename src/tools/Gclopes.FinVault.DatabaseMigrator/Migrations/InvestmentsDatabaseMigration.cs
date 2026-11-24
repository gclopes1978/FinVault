using Gclopes.FinVault.DatabaseMigrator.Interfaces;
using Gclopes.FinVault.Investments.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Gclopes.FinVault.DatabaseMigrator.Migrations;

/// <summary>
/// Applies database migrations for the Investments microservice.
/// </summary>
public sealed class InvestmentsDatabaseMigration(
    IConfiguration configuration) : IDatabaseMigration
{
    /// <inheritdoc />
    public string ServiceName => "Investments";

    /// <inheritdoc />
    public async Task MigrateAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            configuration.GetConnectionString("Investments");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Investments was not configured.");
        }

        var options = new DbContextOptionsBuilder<FinVaultInvestmentsDb>()
            .UseSqlServer(connectionString)
            .Options;

        await using var dbContext =
            new FinVaultInvestmentsDb(options);

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}