using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Gclopes.FinVault.DocumentIngestion.Data;

// Used only by `dotnet ef` design-time tooling (migrations add/update/etc). Program.cs normally
// builds ApplicationDbContext through the app's own DI container, but that requires the full
// WebApplication host to start — which throws if RABBITMQ_USER/RABBITMQ_PASS aren't set as
// environment variables. This factory builds a minimal, standalone DbContextOptions straight
// from appsettings.json (same ConnectionStrings:DefaultConnection key Program.cs reads), so
// generating/applying migrations never depends on RabbitMQ being configured.
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
