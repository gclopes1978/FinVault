namespace Gclopes.FinVault.CurrentAccount.Models;



public record MovementResponse(
    Guid Id,
    decimal Amount,
    string Type,
    string Description,
    string? UserNote,
    DateTime TransactionDate,
    DateTime ValueDate,
    DateTime CreatedAt
);