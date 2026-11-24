using Gclopes.FinVault.Investments.Endpoints;

namespace Gclopes.FinVault.Investments.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring the HTTP request pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the Investments HTTP request pipeline.
    /// </summary>
    public static WebApplication UseInvestmentsPipeline(
        this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(
            CorsServiceCollectionExtensions.GetCorsPolicyName());

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapInvestmentsEndpoints();

        return app;
    }
}