using Gclopes.FinVault.DocumentIngestion.DependencyInjection;

namespace Gclopes.FinVault.DocumentIngestion;

/// <summary>
/// Main entry point for the DocumentIngestion microservice.
/// </summary>
public static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddDocumentIngestionSwagger()
            .AddDocumentIngestionPersistence(builder.Configuration)
            .AddDocumentIngestionMessaging(builder.Configuration)
            .AddDocumentIngestionApplication();

        var app = builder.Build();

        app.UseDocumentIngestionPipeline();

        await app.RunAsync();
    }
}