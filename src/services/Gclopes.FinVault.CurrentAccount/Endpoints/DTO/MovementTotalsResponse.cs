namespace Gclopes.FinVault.CurrentAccount.Endpoints.DTO;

// Aggregate figures shown at the top of the Current Account screen.
// TotalCredits/TotalDebits respect the same date-range filters as GET /api/movements;
// CurrentBalance is always the running balance of the single most recent movement,
// regardless of any filter, since it represents the real account balance.
public record MovementTotalsResponse(
    decimal TotalCredits,
    decimal TotalDebits,
    decimal? CurrentBalance);
