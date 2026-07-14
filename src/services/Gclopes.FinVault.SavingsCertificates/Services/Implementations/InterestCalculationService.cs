using Gclopes.FinVault.SavingsCertificates.Data.Entities;
using Gclopes.FinVault.SavingsCertificates.Services.Interfaces;

namespace Gclopes.FinVault.SavingsCertificates.Services.Implementations;

// The service now uses Primary Constructor to inject all available strategies
public class InterestCalculationService(IEnumerable<ISeriesStrategy> strategies) : IInterestCalculationService
{
    public decimal CalculateCurrentValue(SavingsBond bond)
    {
        // Select the appropriate strategy for the bond series
        var strategy = strategies.FirstOrDefault(s => s.CanHandle(bond.Series))
                       ?? throw new NotSupportedException($"Series {bond.Series} is not supported.");

        // Delegate the calculation to the specific strategy
        return strategy.CalculateInterest(bond);
    }
}