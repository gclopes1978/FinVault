using Gclopes.FinVault.CurrentAccount.Endpoints.DTO;
using Gclopes.FinVault.CurrentAccount.Models;

namespace Gclopes.FinVault.CurrentAccount.Services.Interfaces;

public interface IMovementService
{
    // Atualiza a nota pessoal de um movimento; devolve o movimento atualizado, ou null se não existir
    Task<MovementResponse?> UpdateNoteAsync(Guid movementId, string note);

    // Registra um extrato completo (via importação de PDF/JSON)
    Task<BankStatementResponse> RegisterBankStatementAsync(BankStatementPayloadDto statementDto);

    // Recupera os movimentos paginados, filtrados por intervalo de datas e ordenados por
    // TransactionDate desc, ValueDate desc (default)
    Task<PagedResult<MovementResponse>> GetMovementsAsync(MovementQueryParameters query);

    // Recupera os totais de crédito/débito (respeitando os mesmos filtros de data) e o saldo atual
    Task<MovementTotalsResponse> GetMovementTotalsAsync(MovementQueryParameters query);

    Task<List<MovementTotalsByYearMonthResponse>> GetTotalsByYearMonthResponse(int year);

    Task<List<int>> GetAvailableYearsAsync();
}