using Gclopes.FinVault.PensionPlans.Endpoints.DTO;

namespace Gclopes.FinVault.PensionPlans.Domain;

// Encapsulates the business rules a movement must satisfy before it is persisted.
// Kept as a small static helper (mirrors Gclopes.FinVault.CurrentAccount.Domain.MovementValidator)
// so the rules live in one place, are easy to unit test in isolation, and are not scattered
// across the endpoint or the service layer.
public static class InsuranceMovementValidator
{
    public static bool IsValid(InsuranceMovementDto dto, out string? errorMessage)
    {
        // "Movement" is the human-readable description (e.g. "Liquidação de Prémio").
        // Without it the record is meaningless, so we reject it outright.
        if (string.IsNullOrWhiteSpace(dto.Movement))
        {
            errorMessage = "Movement description is required.";
            return false;
        }

        // A default DateTime (0001-01-01) means the "date" field was missing or failed to
        // parse upstream - better to reject early than to silently store a wrong date.
        if (dto.Date == default)
        {
            errorMessage = "Movement date is required and must be a valid date.";
            return false;
        }

        // Debit and Credit are optional individually (a line is either a debit OR a credit,
        // per the sample payload), but at least one of them must carry a value - a movement
        // with both null would mean nothing actually happened.
        if (dto.Debit is null && dto.Credit is null)
        {
            errorMessage = "Either Debit or Credit must be provided.";
            return false;
        }

        // Negative amounts would indicate a parsing error further up the pipeline
        // (the PDF parser already strips signs and works with absolute values).
        if (dto.Debit is < 0 || dto.Credit is < 0)
        {
            errorMessage = "Debit and Credit values cannot be negative.";
            return false;
        }

        errorMessage = null;
        return true;
    }
}
