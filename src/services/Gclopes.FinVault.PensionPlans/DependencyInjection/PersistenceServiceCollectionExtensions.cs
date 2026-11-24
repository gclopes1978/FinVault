using Gclopes.FinVault.PensionPlans.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Gclopes.FinVault.PensionPlans.DependencyInjection;

/// <summary>
/// Provides persistence service registration extensions.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers the PensionPlans database context.
    /// </summary>
    public static IServiceCollection AddPensionPlansPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);

            options.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        return services;
    }
}