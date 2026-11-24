using Gclopes.FinVault.DocumentIngestion.Hubs;

namespace Gclopes.FinVault.DocumentIngestion.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring the HTTP request pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the DocumentIngestion HTTP request pipeline.
    /// </summary>
    public static WebApplication UseDocumentIngestionPipeline(
        this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.MapHub<DocumentProcessingHub>(
            "/api/document/hub");

        return app;
    }
}