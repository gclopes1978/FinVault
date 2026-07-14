using Gclopes.FinVault.PensionPlans.Endpoints.DTO;

namespace Gclopes.FinVault.PensionPlans.Domain
{
    public static class PensionFundMovementValidator
    {
        public static bool IsValid(PensionFundMovementDto movement, out string errorMessage)
        {
            if (movement.Date == default)
            {
                errorMessage = "Date is required.";
                return false;
            }
            if (movement.Quantity <= 0)
            {
                errorMessage = "Quantity must be greater than zero.";
                return false;
            }
            if (movement.UnitPrice <= 0)
            {
                errorMessage = "Unit price must be greater than zero.";
                return false;
            }
            if (movement.Valuation < 0)
            {
                errorMessage = "Valorization cannot be negative.";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }
    }
}
