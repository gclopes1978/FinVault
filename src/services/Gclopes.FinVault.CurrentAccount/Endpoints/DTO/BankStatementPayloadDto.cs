namespace Gclopes.FinVault.CurrentAccount.Endpoints.DTO;

// Matches exactly the envelope shape dispatched by the PdfParser's BankStatementParser
// (CURRENTACCOUNT_SERVICE_URL): { sourceDocumentId, data }. System.Text.Json's default
// case-insensitive binding matches these PascalCase properties against the parser's camelCase JSON.
public record BankStatementPayloadDto(
     Guid SourceDocumentId,
     BankStatementDataDto Data
 );

public record BankStatementDataDto(
     decimal InitialBalance,
     decimal FinalBalance,
     List<TransactionDto> Transactions
 );
