using System;

namespace Gclopes.FinVault.CurrentAccount.Models;

public record CreateMovementRequest(
    decimal Amount,
    string Type,
    string Description,
    string Category,
    DateTime TransactionDate,
    DateTime ValueDate
);

public record MovementResponse(
    Guid Id,
    decimal Amount,
    string Type,
    string Description,
    string Category,
    DateTime TransactionDate,
    DateTime ValueDate,
    DateTime CreatedAt
);