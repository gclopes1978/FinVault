using Gclopes.FinVault.SavingsCertificates.Data.Entities;

namespace Gclopes.FinVault.SavingsCertificates.Services.Interfaces;

public interface ISeriesStrategy
{
    bool CanHandle(string series);
    decimal CalculateInterest(SavingsBond bond);
}
