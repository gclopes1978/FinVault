using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Gclopes.FinVault.DocumentIngestion.Models;
using Gclopes.FinVault.DocumentIngestion.Services;

namespace Gclopes.FinVault.DocumentIngestion.Controllers;

[ApiController]
[Route("api/document")]
public class DocumentController(
    IPublishEndpoint publishEndpoint,
    IPdfProcessingQueuePublisher pdfProcessingQueuePublisher,
    ILogger<DocumentController> logger,
    IConfiguration configuration) : ControllerBase
{
    private readonly string _storagePath = configuration["StorageSettings:Path"] ?? "uploads";
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Single identifier for this document, used both as the stored filename prefix and as
        // the idempotency key downstream services (e.g. PayrollAnalysis) key off.
        var documentId = Guid.NewGuid();

        // Define a storage path
        var filePath = Path.Combine(_storagePath, $"{documentId}_{file.FileName}");

        // Ensure directory exists
        Directory.CreateDirectory(_storagePath);

        byte[] fileBytes;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            fileBytes = memoryStream.ToArray();
        }

        await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

        // Publish event to RabbitMQ via MassTransit (kept for future consumers / audit trail)
        var eventMessage = new DocumentUploadedEvent(documentId, filePath, Guid.NewGuid(), DateTime.UtcNow);
        await publishEndpoint.Publish(eventMessage);

        // Bridge into the raw queue the PdfParser Python service actually consumes from, carrying
        // the same documentId so it is forwarded downstream as sourceDocumentId.
        await pdfProcessingQueuePublisher.PublishAsync(documentId, file.FileName, fileBytes);

        logger.LogInformation("File {FileName} uploaded (documentId {DocumentId}) and dispatched for processing.", file.FileName, documentId);

        return Ok(new { Message = "File uploaded successfully", DocumentId = documentId, Path = filePath });
    }
}