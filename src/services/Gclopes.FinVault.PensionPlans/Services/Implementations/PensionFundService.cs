using Gclopes.FinVault.PensionPlans.Data;
using Gclopes.FinVault.PensionPlans.Domain;
using Gclopes.FinVault.PensionPlans.Endpoints.DTO;
using Gclopes.FinVault.PensionPlans.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.PensionPlans.Services.Implementations
{
    public class PensionFundService(ApplicationDbContext context, ILogger<PensionFundService> logger) : IPensionFundService
    {
        public async Task<IEnumerable<PensionFundMovementResponse>> GetAllMovementsAsync()
        {
            var movements = await context.PensionFundMovement
            .OrderBy(m => m.Date)
            .ToListAsync();

            return movements.Select(m => new PensionFundMovementResponse(
                m.Id,
                m.Date,
                m.Quantity,
                m.UnitPrice,
                m.Valuation,
                m.CreatedAt
            ));
        }

        public async Task<ImportFundPensionMovementResponse> ImportFundMovementsAsync(PensionFundMovementDto movement)
        {
            // Basic validation — quantities/prices should never be negative for this instrument
            if(!PensionFundMovementValidator.IsValid(movement, out var errorMessage))
            {
                logger.LogWarning(
                    "Invalid fund movement data: {ErrorMessage}", errorMessage);
                return new ImportFundPensionMovementResponse(
                    Imported: false,
                    Message: $"Invalid movement data: {errorMessage}",
                    ProcessedAt: DateTime.UtcNow);
            }

            // Idempotency check: avoid importing the same movement twice when the same
            // PDF statement gets reprocessed (parser has no stable external ID to key on)
            var alreadyExists = await context.PensionFundMovement.AnyAsync(m =>
                m.Date == movement.Date &&
                m.Quantity == movement.Quantity &&
                m.UnitPrice == movement.UnitPrice &&
                m.Valuation == movement.Valuation);

            if (alreadyExists)
            {
                logger.LogInformation(
                    "Duplicate fund movement skipped for date {Date}", movement.Date);

                return new ImportFundPensionMovementResponse(
                    Imported: false,
                    Message: "Movement already exists, skipped.",
                    ProcessedAt: DateTime.UtcNow);
            }

            var entity = new PensionFundMovementEntity
            {
                Id = Guid.NewGuid(),
                Date = movement.Date,
                Quantity = movement.Quantity,
                UnitPrice = movement.UnitPrice,
                Valuation = movement.Valuation
            };

            context.PensionFundMovement.Add(entity);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Fund movement imported: {Id} for date {Date}", entity.Id, entity.Date);

            return new ImportFundPensionMovementResponse(
                Imported: true,
                Message: "Movement imported successfully.",
                ProcessedAt: DateTime.UtcNow);
        }
    }
}
