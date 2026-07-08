namespace Gclopes.FinVault.PensionPlans.Endpoints.DTO;

public record PensionFundMovementResponse(
    Guid Id, 
    DateTime Date, 
    decimal Quantity, 
    decimal UnitPrice, 
    decimal Valuation, 
    DateTime CreatedAt
);
