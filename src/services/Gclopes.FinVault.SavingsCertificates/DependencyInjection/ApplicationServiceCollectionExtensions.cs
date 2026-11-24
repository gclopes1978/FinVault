using Gclopes.FinVault.SavingsCertificates.Services.Implementations;
using Gclopes.FinVault.SavingsCertificates.Services.Interfaces;
using Gclopes.FinVault.SavingsCertificates.Services.Strategies;

namespace Gclopes.FinVault.SavingsCertificates.DependencyInjection;

/// <summary>
/// Provides application service registration extensions.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers SavingsCertificates application services.
    /// </summary>
    public static IServiceCollection AddSavingsCertificatesApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ISeriesStrategy, SeriesEStrategy>();
        services.AddScoped<ISeriesStrategy, SeriesFStrategy>();

        services.AddScoped<IInterestCalculationService, InterestCalculationService>();

        return services;
    }
}