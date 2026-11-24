using Gclopes.FinVault.DocumentIngestion.Hubs;
using Gclopes.FinVault.DocumentIngestion.Services;

namespace Gclopes.FinVault.DocumentIngestion.DependencyInjection;

/// <summary>
/// Provides extension methods for registering application services.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers DocumentIngestion application services.
    /// </summary>
    public static IServiceCollection AddDocumentIngestionApplication(
        this IServiceCollection services)
    {
        services.AddControllers();

        services.AddSignalR();

        services.AddSingleton<
            IPdfProcessingQueuePublisher,
            PdfProcessingQueuePublisher>();

        services.AddAuthorization();

        return services;
    }
}