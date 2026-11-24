using Gclopes.FinVault.SavingsCertificates.Data;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.SavingsCertificates.DependencyInjection;

/// <summary>
/// Provides persistence service registration extensions.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SavingsCertificates database context.
    /// </summary>
    public static IServiceCollection AddSavingsCertificatesPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<SavingsBondDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

        return services;
    }
}