using Gclopes.FinVault.Investments.Data;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.Investments.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring database persistence.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Investments database context.
    /// </summary>
    public static IServiceCollection AddInvestmentsPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is missing.");

        services.AddDbContext<FinVaultInvestmentsDb>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        return services;
    }
}