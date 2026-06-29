using Gclopes.FinVault.CurrentAccount.Models;

namespace Gclopes.FinVault.CurrentAccount.Services.Interfaces;

public interface IMovementService
{
    // Registers a new financial movement in the system
    Task<MovementResponse> RegisterMovementAsync(CreateMovementRequest request, string userId);

    // Retrieves all recorded financial movements ordered by transaction date
    Task<IEnumerable<MovementResponse>> GetAllMovementsAsync(string userId);
}