namespace Gclopes.FinVault.PensionPlans.Endpoints.DTO;

public record PensionFundMovementDto(
    DateTime Date,
    decimal Quantity,
    decimal UnitPrice,
    decimal Valuation
);
