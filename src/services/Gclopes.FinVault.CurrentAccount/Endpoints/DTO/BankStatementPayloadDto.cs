namespace Gclopes.FinVault.CurrentAccount.Endpoints.DTO;

public record BankStatementPayloadDto(
     decimal InitialBalance,
     decimal FinalBalance,
     List<TransactionDto> Transactions
 );
