using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Gclopes.FinVault.Identity.Data;

/// <summary>
/// Creates ApplicationDbContext instances during Entity Framework Core design-time operations.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <summary>
    /// Creates a database context using the configured connection string.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>A configured ApplicationDbContext instance.</returns>
    /// <summary>
    /// Creates a database context using the configured connection string.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>A configured ApplicationDbContext instance.</returns>
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Read the connection string from environment variables.
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__DefaultConnection environment variable is required.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}