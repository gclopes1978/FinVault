using Gclopes.FinVault.PensionPlans.Endpoints.DTO;

namespace Gclopes.FinVault.PensionPlans.Services.Interfaces;

public interface IPensionFundService
{
    Task<ImportFundPensionMovementResponse> ImportFundMovementsAsync(PensionFundMovementDto movement);
    Task<IEnumerable<PensionFundMovementResponse>> GetAllMovementsAsync();
}
