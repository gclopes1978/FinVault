using Microsoft.OpenApi;

namespace Gclopes.FinVault.DocumentIngestion.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring Swagger.
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    /// Registers Swagger services.
    /// </summary>
    public static IServiceCollection AddDocumentIngestionSwagger(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Gclopes.FinVault.DocumentIngestion API",
                Version = "v1"
            });
        });

        return services;
    }
}