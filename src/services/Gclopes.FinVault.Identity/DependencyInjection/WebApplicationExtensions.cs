using Gclopes.FinVault.Identity.Data;
using Gclopes.FinVault.Identity.Middlewares;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.Identity.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring the web application pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The configured web application.</returns>
    public static WebApplication UseFinVaultPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}