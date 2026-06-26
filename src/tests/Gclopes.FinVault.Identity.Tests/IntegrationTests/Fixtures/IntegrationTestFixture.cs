using Gclopes.FinVault.Identity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gclopes.FinVault.Identity.Tests.IntegrationTests.Fixtures;

public class IntegrationTestFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Adiciona valores em memória que substituem o que estaria no appsettings.json
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "SuperSecretKeyMustBeAtLeast16CharactersLong!!!" },
                { "Jwt:Issuer", "FinVaultTest" },
                { "Jwt:Audience", "FinVaultTest" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove TUDO o que for DbContextOptions, incluindo as factory registrations
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType.Name.Contains("DbContextOptions")).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Adiciona o SQLite
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connection);
            }, ServiceLifetime.Singleton); // Singleton ajuda a manter a conexão aberta

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        });
    }
}