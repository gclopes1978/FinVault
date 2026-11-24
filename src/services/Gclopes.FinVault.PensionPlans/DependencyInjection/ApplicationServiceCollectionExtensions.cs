using Gclopes.FinVault.PensionPlans.Middlewares;
using Gclopes.FinVault.PensionPlans.Services.Implementations;
using Gclopes.FinVault.PensionPlans.Services.Interfaces;

namespace Gclopes.FinVault.PensionPlans.DependencyInjection;

/// <summary>
/// Provides application service registration extensions.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers application services and exception handling.
    /// </summary>
    public static IServiceCollection AddPensionPlansApplication(
        this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddScoped<IPensionInsuranceService, PensionInsuranceService>();
        services.AddScoped<IPensionFundService, PensionFundService>();

        return services;
    }
}