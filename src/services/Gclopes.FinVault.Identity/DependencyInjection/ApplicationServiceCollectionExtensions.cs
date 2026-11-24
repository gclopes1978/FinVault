using FluentValidation;
using Gclopes.FinVault.Identity.Application.Behaviors;
using Gclopes.FinVault.Identity.Services;
using MediatR;

namespace Gclopes.FinVault.Identity.DependencyInjection;

/// <summary>
/// Provides extension methods for registering application services.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers application services and dependencies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFinVaultApplication(
        this IServiceCollection services)
    {
        services.AddScoped<TokenService>();
        services.AddScoped<ClientSecretHasherService>();

        services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddControllers();

        return services;
    }
}