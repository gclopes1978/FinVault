namespace Gclopes.FinVault.DocumentIngestion.Services;

public interface IPdfProcessingQueuePublisher
{
    // Publishes a document straight onto the raw "pdf_processing_queue" that the Python
    // PdfParser service consumes with its own lightweight aio_pika consumer.
    Task PublishAsync(Guid documentId, string fileName, byte[] fileBytes, CancellationToken cancellationToken = default);
}
