using Gclopes.FinVault.PensionPlans.Data;
using Gclopes.FinVault.PensionPlans.Domain;
using Gclopes.FinVault.PensionPlans.Endpoints.DTO;
using Gclopes.FinVault.PensionPlans.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.PensionPlans.Services.Implementations;

// Primary constructor injects the EF Core DbContext (same pattern used by
// Gclopes.FinVault.CurrentAccount.Services.Implementations.MovementService).
public class PensionInsuranceService(ApplicationDbContext context) : IPensionInsuranceService
{
    public async Task<ImportPensionInsuranceMovementsResponse> ImportMovementsAsync(IEnumerable<PensionInsuranceMovementDto> movements)
    {
        var movementsList = movements.ToList();

        int imported = 0;
        int skipped = 0;

        // Load existing (Movement, Date, Balance) combinations already in the database once,
        // instead of querying inside the loop below (N+1 problem). "Encerramento do Mês" rows
        // repeat every month with different balances, so comparing Balance alongside Movement
        // and Date is enough to uniquely identify a real duplicate re-import without rejecting
        // two different months' closing entries.
        var existingKeys = await context.PensionInsuranceMovement
            .Select(e => new { e.Movement, e.Date, e.Balance })
            .ToListAsync();

        var existingKeySet = existingKeys
            .Select(e => (e.Movement, e.Date, e.Balance))
            .ToHashSet();

        var entitiesToAdd = new List<PensionInsuranceMovementEntity>();

        foreach (var dto in movementsList)
        {
            // Reject structurally invalid rows early instead of letting a bad record
            // corrupt the batch or blow up on SaveChangesAsync().
            if (!PensionInsuranceMovementValidator.IsValid(dto, out var errorMessage))
            {
                throw new InvalidOperationException($"Invalid movement '{dto.Movement}' on {dto.Date:yyyy-MM-dd}: {errorMessage}");
            }

            var key = (dto.Movement, dto.Date, dto.Balance);
            if (existingKeySet.Contains(key))
            {
                skipped++;
                continue;
            }

            entitiesToAdd.Add(new PensionInsuranceMovementEntity
            {
                Id = Guid.NewGuid(),
                Movement = dto.Movement,
                Receipt = dto.Receipt,
                Date = dto.Date,
                Debit = dto.Debit,
                Credit = dto.Credit,
                Balance = dto.Balance,
                CreatedAt = DateTime.UtcNow
            });

            // Track the key we just queued so duplicates *within the same batch*
            // (e.g. the same PDF page parsed twice) are also caught.
            existingKeySet.Add(key);
            imported++;
        }

        if (entitiesToAdd.Count > 0)
        {
            context.PensionInsuranceMovement.AddRange(entitiesToAdd);
            await context.SaveChangesAsync();
        }

        return new ImportPensionInsuranceMovementsResponse(imported, skipped, DateTime.UtcNow);
    }

    public async Task<IEnumerable<PensionInsuranceMovementResponse>> GetAllMovementsAsync()
    {
        var movements = await context.PensionInsuranceMovement
            .OrderBy(m => m.Date)
            .ToListAsync();

        return movements.Select(m => new PensionInsuranceMovementResponse(
            m.Id,
            m.Movement,
            m.Receipt,
            m.Date,
            m.Debit,
            m.Credit,
            m.Balance,
            m.CreatedAt
        ));
    }
}
