using Gclopes.FinVault.SavingsCertificates.Data.Entities;

namespace Gclopes.FinVault.SavingsCertificates.Services.Interfaces;

public interface IInterestCalculationService
{
    // Async because the calculation now needs to fetch the series' historical
    // monthly base rates from the database (see SeriesRateHistory).
    Task<decimal> CalculateCurrentValueAsync(SavingsBond bond, CancellationToken ct = default);
}