using Gclopes.FinVault.MarketPrices.Endpoints;
using Gclopes.FinVault.MarketPrices.Middlewares;

namespace Gclopes.FinVault.MarketPrices.DependencyInjection;

/// <summary>
/// Provides web application pipeline extensions.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the MarketPrices HTTP request pipeline.
    /// </summary>
    public static WebApplication UseMarketPricesPipeline(
        this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapHealthChecks("/health");

        app.UseHttpsRedirection();
        app.UseCors(
            CorsServiceCollectionExtensions.GetMarketPricesCorsPolicyName());

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapMarketPricesEndpoints();

        return app;
    }
}