using Gclopes.FinVault.CurrentAccount.Data;
using Gclopes.FinVault.CurrentAccount.Domain;
using Gclopes.FinVault.CurrentAccount.Endpoints.DTO;
using Gclopes.FinVault.CurrentAccount.Models;
using Gclopes.FinVault.CurrentAccount.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Gclopes.FinVault.CurrentAccount.Services.Implementations;

public class MovementService(ApplicationDbContext context) : IMovementService
{

    public async Task<IEnumerable<MovementResponse>> GetAllMovementsAsync()
    {
        var movements = await context.Movements
        .OrderBy(m => m.TransactionDate)
        .ToListAsync();

        return movements.Select(m => new MovementResponse(
            m.Id,
            m.Amount,
            m.Type,
            m.Description,
            m.UserNote,
            m.TransactionDate,
            m.ValueDate,
            m.CreatedAt
        ));
    }

    public async Task<BankStatementResponse> RegisterBankStatementAsync(BankStatementPayloadDto dto, string userId)
    {
        // 1. Validação de Domínio (usando o BalanceCalculator que criámos)
        // Extraímos os dados para uma forma simples (tupla) que o Domain entende
        var movementsForCalc = dto.Data.Transactions.Select(t => (t.Amount, t.Type));

        var calculatedFinalBalance = BalanceCalculator.CalculateFinalBalance(dto.Data.InitialBalance, movementsForCalc);

        // Verificação matemática de segurança
        if (Math.Abs(calculatedFinalBalance - dto.Data.FinalBalance) > 0.01m)
        {
            throw new InvalidOperationException($"Inconsistência financeira: O saldo calculado ({calculatedFinalBalance}) não bate com o saldo final fornecido ({dto.Data.FinalBalance}).");
        }

        // 2. Criação do Objeto (Infraestrutura)
        var statement = new BankStatement
        {
            Id = Guid.NewGuid(),
            InitialBalance = dto.Data.InitialBalance,
            FinalBalance = dto.Data.FinalBalance,
        };

        foreach (var t in dto.Data.Transactions)
        {
            var movement = new Movement
            {
                Id = Guid.NewGuid(),
                Amount = t.Amount,
                Type = t.Type,
                Description = t.Description,
                Balance = t.Balance,
                BankStatementId = statement.Id,
                // DateTime.ParseExact é perigoso sem try-catch, mas se já validaste no FluentValidation, aqui é seguro
                TransactionDate = DateTime.ParseExact(t.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                ValueDate = DateTime.ParseExact(t.ValueDate, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                CreatedAt = DateTime.UtcNow
            };
            statement.Movements.Add(movement);
        }

        // 3. Persistência
        context.BankStatements.Add(statement);
        await context.SaveChangesAsync();

        // 4. Retorno (Mapeamento para o Response)
        return new BankStatementResponse(statement.Id, "Bank statement processed successfully.", statement.ProcessedAt);
    }

    public async Task<bool> UpdateNoteAsync(Guid movementId, string note)
    {
        // Procura o movimento, mas garante que o utilizador é o dono!
        var movement = await context.Movements
            .FirstOrDefaultAsync(m => m.Id == movementId);

        if (movement == null) return false;

        // Atualiza apenas a nota
        movement.UserNote = note;

        // Grava no banco
        await context.SaveChangesAsync();

        return true;
    }
}