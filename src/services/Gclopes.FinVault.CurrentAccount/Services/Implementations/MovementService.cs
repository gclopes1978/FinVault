using Gclopes.FinVault.CurrentAccount.Data;
using Gclopes.FinVault.CurrentAccount.Models;
using Gclopes.FinVault.CurrentAccount.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.CurrentAccount.Services.Implementations;

public class MovementService(ApplicationDbContext context) : IMovementService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<MovementResponse> RegisterMovementAsync(CreateMovementRequest request)
    {
        var movement = new Movement
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Type = request.Type,
            Description = request.Description,
            Category = request.Category,
            TransactionDate = request.TransactionDate,
            ValueDate = request.ValueDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.Movements.Add(movement);
        await _context.SaveChangesAsync();

        return new MovementResponse(
            movement.Id,
            movement.Amount,
            movement.Type,
            movement.Description,
            movement.Category,
            movement.TransactionDate,
            movement.ValueDate,
            movement.CreatedAt
        );
    }

    public async Task<IEnumerable<MovementResponse>> GetAllMovementsAsync()
    {
        var movements = await _context.Movements
            .OrderByDescending(m => m.TransactionDate)
            .ToListAsync();

        return movements.Select(m => new MovementResponse(
            m.Id,
            m.Amount,
            m.Type,
            m.Description,
            m.Category,
            m.TransactionDate,
            m.ValueDate,
            m.CreatedAt
        ));
    }
}