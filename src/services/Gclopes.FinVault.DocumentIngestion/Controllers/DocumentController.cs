using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Gclopes.FinVault.DocumentIngestion.Data;
using Gclopes.FinVault.DocumentIngestion.Hubs;
using Gclopes.FinVault.DocumentIngestion.Models;
using Gclopes.FinVault.DocumentIngestion.Services;

namespace Gclopes.FinVault.DocumentIngestion.Controllers;

[ApiController]
[Route("api/document")]
public class DocumentController(
    IPublishEndpoint publishEndpoint,
    IPdfProcessingQueuePublisher pdfProcessingQueuePublisher,
    ApplicationDbContext dbContext,
    IHubContext<DocumentProcessingHub> hubContext,
    ILogger<DocumentController> logger,
    IConfiguration configuration) : ControllerBase
{
    private readonly string _storagePath = configuration["StorageSettings:Path"] ?? "uploads";

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string? documentType)
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

        var uploadedAt = DateTime.UtcNow;

        // Persist the upload immediately, before processing even starts, so it shows up in the
        // import history straight away with a pending (Success == null) status. DocumentType holds
        // the user-selected hint for now; it's overwritten with the PdfParser's own classification
        // once processing completes (see MarkCompleted below).
        dbContext.UploadedDocuments.Add(new UploadedDocument
        {
            Id = documentId,
            FileName = file.FileName,
            FileSizeBytes = fileBytes.LongLength,
            DocumentType = documentType,
            UploadedAt = uploadedAt
        });
        await dbContext.SaveChangesAsync();

        // Publish event to RabbitMQ via MassTransit (kept for future consumers / audit trail)
        var eventMessage = new DocumentUploadedEvent(documentId, filePath, Guid.NewGuid(), uploadedAt);
        await publishEndpoint.Publish(eventMessage);

        // Bridge into the raw queue the PdfParser Python service actually consumes from, carrying
        // the same documentId so it is forwarded downstream as sourceDocumentId and reported back
        // to MarkCompleted below once processing finishes.
        await pdfProcessingQueuePublisher.PublishAsync(documentId, file.FileName, fileBytes);

        logger.LogInformation("File {FileName} uploaded (documentId {DocumentId}) and dispatched for processing.", file.FileName, documentId);

        return Ok(new { Message = "File uploaded successfully", DocumentId = documentId, Path = filePath });
    }

    // Called by the PdfParser (Python) service once it has finished processing a document —
    // whether it succeeded or failed. Not routed through the Gateway: PdfParser calls this
    // directly over the internal Docker network.
    [HttpPost("{id:guid}/completed")]
    public async Task<IActionResult> MarkCompleted(Guid id, [FromBody] DocumentProcessingCompletedRequest request)
    {
        var document = await dbContext.UploadedDocuments.FindAsync(id);
        if (document is null)
        {
            logger.LogWarning("Received completion callback for unknown documentId {DocumentId}.", id);
            return NotFound();
        }

        document.ProcessedAt = DateTime.UtcNow;
        document.Success = request.Success;
        document.ErrorMessage = request.ErrorMessage;

        // Only overwrite the type with the classifier's result when one was actually returned
        // (a failed/unclassified document may not have one), so the user's original selection stays visible.
        if (!string.IsNullOrWhiteSpace(request.DocumentType))
        {
            document.DocumentType = request.DocumentType;
        }

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Document {DocumentId} processing completed. Success: {Success}.",
            id, request.Success);

        // Push the updated record to every connected PDF Upload Centre client so the import
        // history table updates live, without the frontend needing to poll for it.
        await hubContext.Clients.All.SendAsync("documentProcessed", new
        {
            document.Id,
            document.FileName,
            document.DocumentType,
            document.FileSizeBytes,
            document.UploadedAt,
            document.ProcessedAt,
            document.Success,
            document.ErrorMessage
        });

        return Ok();
    }

    // Backs the import history table in the PDF Upload Centre: filename, upload/processing dates,
    // document type, size and success flag for the most recent uploads.
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int take = 20)
    {
        var boundedTake = Math.Clamp(take, 1, 100);

        var items = await dbContext.UploadedDocuments
            .OrderByDescending(d => d.UploadedAt)
            .Take(boundedTake)
            .Select(d => new
            {
                d.Id,
                d.FileName,
                d.DocumentType,
                d.FileSizeBytes,
                d.UploadedAt,
                d.ProcessedAt,
                d.Success,
                d.ErrorMessage
            })
            .ToListAsync();

        return Ok(items);
    }
}
