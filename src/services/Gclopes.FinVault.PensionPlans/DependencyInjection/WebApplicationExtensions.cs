using Gclopes.FinVault.PensionPlans.Endpoints;

namespace Gclopes.FinVault.PensionPlans.DependencyInjection;

/// <summary>
/// Provides web application pipeline extensions.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the PensionPlans HTTP request pipeline.
    /// </summary>
    public static WebApplication UsePensionPlansPipeline(
        this WebApplication app)
    {
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors(
            CorsServiceCollectionExtensions.GetPensionPlansCorsPolicyName());

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapPensionPlansEndpoints();

        return app;
    }
}