namespace Gclopes.FinVault.SyntheticPortfolio.Abstractions;

/// <summary>
/// Defines the contract for extracting structured data from local PDF documents.
/// </summary>
public interface IPdfExtractionService
{
    /// <summary>
    /// Extracts structured financial data from a PDF document.
    /// </summary>
    Task<PdfExtractionResult> ExtractAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of extracting a financial PDF document.
/// </summary>
public sealed record PdfExtractionResult(
    string DocumentType,
    string JsonData);
