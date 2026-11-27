using System.Text.Json;

namespace Gclopes.FinVault.Investments.DTO;

public sealed record Trading212StatementImportRequest(
    string SourceDocumentId,
    JsonElement Data);
