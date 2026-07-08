namespace Gclopes.FinVault.PensionPlans.Endpoints.DTO;

public record ImportFundPensionMovementResponse(
bool Imported,
string Message,
DateTime ProcessedAt
);
