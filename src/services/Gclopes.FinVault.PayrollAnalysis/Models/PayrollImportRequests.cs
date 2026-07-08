namespace Gclopes.FinVault.PayrollAnalysis.Models;

// Matches exactly the payload shape dispatched by the PdfParser's PayrollParser
// (FOLHASALARIAL_SERVICE_URL). Property names are PascalCase here; System.Text.Json's default
// case-insensitive binding matches them against the parser's camelCase JSON.
public record ImportPayrollRequest(
    Guid SourceDocumentId,
    List<PayrollItemRequest> Data
);

public record PayrollItemRequest(
    int Code,
    string Field,
    string? Description,
    string? Date,
    decimal? Quantity,
    decimal? Rate,
    decimal? UnitValue,
    decimal Amount,
    string Type
);

public record PayrollItemResponse(
    Guid Id,
    int Code,
    string Field,
    string? Description,
    string? ReferenceMonth,
    decimal? Quantity,
    decimal? Rate,
    decimal? UnitValue,
    decimal Amount,
    string Type
);

public record PayrollImportResponse(
    Guid Id,
    Guid SourceDocumentId,
    DateTime ImportedAt,
    IReadOnlyList<PayrollItemResponse> Items
);
