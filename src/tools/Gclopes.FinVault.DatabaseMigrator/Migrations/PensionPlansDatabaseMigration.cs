using Gclopes.FinVault.DatabaseMigrator.Interfaces;
using Gclopes.FinVault.PensionPlans.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Gclopes.FinVault.DatabaseMigrator.Migrations;

/// <summary>
/// Applies database migrations for the PensionPlans microservice.
/// </summary>
public sealed class PensionPlansDatabaseMigration(
    IConfiguration configuration) : IDatabaseMigration
{
    /// <inheritdoc />
    public string ServiceName => "PensionPlans";

    /// <inheritdoc />
    public async Task MigrateAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            configuration.GetConnectionString("PensionPlans");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:PensionPlans was not configured.");
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var dbContext = new ApplicationDbContext(options);

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}