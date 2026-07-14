using Gclopes.FinVault.CurrentAccount.Endpoints.DTO;
using Gclopes.FinVault.CurrentAccount.Models;

namespace Gclopes.FinVault.CurrentAccount.Services.Interfaces;

public interface IMovementService
{
    // Registra uma nova transação manual individual
    Task<bool> UpdateNoteAsync(Guid movementId, string note);

    // Registra um extrato completo (via importação de PDF/JSON)
    Task<BankStatementResponse> RegisterBankStatementAsync(BankStatementPayloadDto statementDto, string userId);

    // Recupera todos os movimentos
    Task<IEnumerable<MovementResponse>> GetAllMovementsAsync();
}