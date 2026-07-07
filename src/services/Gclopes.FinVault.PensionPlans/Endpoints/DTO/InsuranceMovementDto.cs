namespace Gclopes.FinVault.PensionPlans.Endpoints.DTO;

// Represents a single movement line exactly as produced by the Python PdfParserService
// (parsers/pension_insurance.py). The PDF parser posts a raw JSON array of these objects
// directly to this service - there is no wrapping envelope (no "sourceDocumentId" field),
// so the endpoint must bind a List<InsuranceMovementDto> straight from the request body.
//
// Property names use PascalCase (C# convention) but bind correctly against the incoming
// lowercase JSON keys ("movement", "receipt", "date", ...) because ASP.NET Core's default
// System.Text.Json options perform case-insensitive property matching on deserialization.
public record InsuranceMovementDto(
    string Movement,
    string? Receipt,
    DateTime Date,
    decimal? Debit,
    decimal? Credit,
    decimal Balance
);
