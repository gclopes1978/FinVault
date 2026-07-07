using Gclopes.FinVault.PensionPlans.Endpoints.DTO;

namespace Gclopes.FinVault.PensionPlans.Services.Interfaces;

public interface IInsuranceMovementService
{
    // Persists a batch of movements coming from the PDF parser. Returns how many were
    // actually saved vs. skipped as duplicates so the caller can report back accurately.
    Task<ImportMovementsResponse> ImportMovementsAsync(IEnumerable<InsuranceMovementDto> movements);

    // Returns every movement currently stored, ordered chronologically (oldest first),
    // which is the order that makes most sense when displaying a statement/ledger.
    Task<IEnumerable<InsuranceMovementResponse>> GetAllMovementsAsync();
}
