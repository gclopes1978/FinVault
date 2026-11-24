using FluentValidation;
using Gclopes.FinVault.CurrentAccount.Endpoints;
using Gclopes.FinVault.CurrentAccount.Middlewares;
using Gclopes.FinVault.CurrentAccount.Services.Implementations;
using Gclopes.FinVault.CurrentAccount.Services.Interfaces;
using System.Reflection;

namespace Gclopes.FinVault.CurrentAccount.DependencyInjection;

/// <summary>
/// Provides extension methods for registering application services.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers CurrentAccount application services.
    /// </summary>
    public static IServiceCollection AddCurrentAccountApplication(
        this IServiceCollection services)
    {
        services.AddProblemDetails();

        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly());

        services.AddScoped<IMovementService, MovementService>();

        services.AddMemoryCache();

        services.AddHealthChecks();

        return services;
    }
}