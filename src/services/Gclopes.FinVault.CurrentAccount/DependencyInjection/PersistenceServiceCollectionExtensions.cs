using Gclopes.FinVault.CurrentAccount.Data;
using Gclopes.FinVault.CurrentAccount.Data.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.CurrentAccount.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring database persistence.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers the CurrentAccount database context and audit interceptor.
    /// </summary>
    public static IServiceCollection AddCurrentAccountPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is missing.");

        services.AddHttpContextAccessor();

        services.AddScoped<AuditLoggingInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString);

            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditLoggingInterceptor>());
        });

        return services;
    }
}