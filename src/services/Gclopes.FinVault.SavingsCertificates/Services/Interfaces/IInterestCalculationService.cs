using Gclopes.FinVault.SavingsCertificates.Data.Entities;

namespace Gclopes.FinVault.SavingsCertificates.Services.Interfaces;

public interface IInterestCalculationService
{
    decimal CalculateCurrentValue(SavingsBond bond);
}