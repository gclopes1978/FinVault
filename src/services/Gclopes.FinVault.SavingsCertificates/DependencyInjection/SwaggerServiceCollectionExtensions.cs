namespace Gclopes.FinVault.SavingsCertificates.DependencyInjection;

/// <summary>
/// Provides Swagger service registration extensions.
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    /// Registers Swagger documentation services.
    /// </summary>
    public static IServiceCollection AddSavingsCertificatesSwagger(
        this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
            {
                Title = "FinVault Savings Certificates API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer",
                new Microsoft.OpenApi.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.ParameterLocation.Header,
                    Description = "Enter your JWT token."
                });

            options.AddSecurityRequirement(document =>
                new Microsoft.OpenApi.OpenApiSecurityRequirement
                {
                    [new Microsoft.OpenApi.OpenApiSecuritySchemeReference(
                        "Bearer", document)] = []
                });
        });

        return services;
    }
}