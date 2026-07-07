namespace Gclopes.FinVault.PensionPlans.Endpoints.DTO;

// Shape returned to API consumers (e.g. the Angular frontend) when listing movements.
// Kept separate from InsuranceMovementEntity on purpose: the entity is a persistence
// concern (EF Core), while this record is the public contract of the API. Changing one
// should not force a change on the other.
public record InsuranceMovementResponse(
    Guid Id,
    string Movement,
    string? Receipt,
    DateTime Date,
    decimal? Debit,
    decimal? Credit,
    decimal Balance,
    DateTime CreatedAt
);
