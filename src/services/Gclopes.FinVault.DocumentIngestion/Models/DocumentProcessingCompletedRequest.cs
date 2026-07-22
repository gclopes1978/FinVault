namespace Gclopes.FinVault.DocumentIngestion.Models;

// Payload the PdfParser (Python) service posts to POST /api/document/{id}/completed once it has
// finished attempting to process a document.
public record DocumentProcessingCompletedRequest(bool Success, string? DocumentType, string? ErrorMessage);
