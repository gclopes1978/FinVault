namespace Gclopes.FinVault.CurrentAccount.Endpoints.DTO;

public record TransactionDto(
    string Date,
    string ValueDate,
    string Description,
    decimal Amount,
    string Type,
    decimal Balance
);