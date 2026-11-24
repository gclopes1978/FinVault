using Gclopes.FinVault.CurrentAccount.Endpoints;

namespace Gclopes.FinVault.CurrentAccount.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring the HTTP request pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the CurrentAccount HTTP request pipeline.
    /// </summary>
    public static WebApplication UseCurrentAccountPipeline(
        this WebApplication app)
    {
        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(
            CorsServiceCollectionExtensions.GetCorsPolicyName());

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapHealthChecks("/health")
            .AllowAnonymous();

        app.MapMovementEndpoints();

        return app;
    }
}