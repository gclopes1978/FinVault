namespace Gclopes.FinVault.CurrentAccount.Endpoints.DTO
{
    public record BankStatementResponse(
      Guid StatementId,
      string Message,
      DateTime ProcessedAt
  );
}
