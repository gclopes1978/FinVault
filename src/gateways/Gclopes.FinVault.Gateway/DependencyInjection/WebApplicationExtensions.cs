namespace Gclopes.FinVault.Gateway.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring the Gateway HTTP pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the Gateway HTTP request pipeline.
    /// </summary>
    public static WebApplication UseGatewayPipeline(
        this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    "/api/v1/accounts/swagger/v1/swagger.json",
                    "Current Account API");

                options.SwaggerEndpoint(
                    "/api/v1/document/swagger/v1/swagger.json",
                    "Document Ingestion API");

                options.SwaggerEndpoint(
                    "/api/v1/investments/swagger/v1/swagger.json",
                    "Investments API");

                options.SwaggerEndpoint(
                    "/api/v1/pension-plans/swagger/v1/swagger.json",
                    "Pension Plans API");

                options.RoutePrefix = "swagger";
            });
        }

        app.UseRouting();

        app.UseCors(
            CorsServiceCollectionExtensions.GetGatewayCorsPolicyName());

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapReverseProxy();

        return app;
    }
}