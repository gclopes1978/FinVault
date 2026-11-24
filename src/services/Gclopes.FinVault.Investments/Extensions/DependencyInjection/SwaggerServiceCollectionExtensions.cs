using Microsoft.OpenApi;

namespace Gclopes.FinVault.Investments.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring Swagger.
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    /// Registers Swagger with JWT bearer authentication support.
    /// </summary>
    public static IServiceCollection AddInvestmentsSwagger(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            const string schemeName = "Bearer";

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FinVault Investments API",
                Version = "v1"
            });

            options.AddSecurityDefinition(schemeName, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token."
            });

            options.AddSecurityRequirement(document =>
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(schemeName, document)] = []
                });
        });

        return services;
    }
}