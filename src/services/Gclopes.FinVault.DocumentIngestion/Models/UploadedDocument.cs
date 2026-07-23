namespace Gclopes.FinVault.DocumentIngestion.Models;

// Tracks a single manually uploaded PDF from the moment it is received until the PdfParser
// (Python service) reports back whether it was successfully processed. Success is null while
// the document is still queued/being processed; ProcessedAt/ErrorMessage are only populated
// once the PdfParser callback (POST /api/document/{id}/completed) is received.
public class UploadedDocument
{
    public Guid Id { get; set; }

    public required string FileName { get; set; }

    public long FileSizeBytes { get; set; }

    // Initially the type hint selected by the user in the PDF Upload Centre (e.g. "ActivoBankStatement").
    // Overwritten with the classifier's own type (e.g. "bank_statement") once processing completes,
    // so a mismatch between what the user picked and what the PdfParser actually detected is visible.
    public string? DocumentType { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    // Null = still pending/processing. True = parsed and forwarded successfully. False = failed.
    public bool? Success { get; set; }

    public string? ErrorMessage { get; set; }
}
