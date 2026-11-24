using Gclopes.FinVault.DatabaseMigrator.Interfaces;
using Gclopes.FinVault.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Gclopes.FinVault.DatabaseMigrator.Migrations;

/// <summary>
/// Applies database migrations for the Identity microservice.
/// </summary>
public sealed class IdentityDatabaseMigration(
    IConfiguration configuration) : IDatabaseMigration
{
    /// <inheritdoc />
    public string ServiceName => "Identity";

    /// <inheritdoc />
    public async Task MigrateAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            configuration.GetConnectionString("Identity");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Identity was not configured.");
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(
                    typeof(ApplicationDbContext).Assembly.FullName))
            .Options;

        await using var dbContext = new ApplicationDbContext(options);

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}