using Gclopes.FinVault.Investments.Services.Implementations;
using Gclopes.FinVault.Investments.Services.Interfaces;

namespace Gclopes.FinVault.Investments.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering application services.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers Investments application services.
    /// </summary>
    public static IServiceCollection AddInvestmentsApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IInvestmentsService, InvestmentsService>();

        services.AddScoped<IXtbExcelImporter, XtbExcelImporter>();

        return services;
    }
}