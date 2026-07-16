using Gclopes.FinVault.SavingsCertificates.Data;
using Gclopes.FinVault.SavingsCertificates.Data.Entities;
using Gclopes.FinVault.SavingsCertificates.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.SavingsCertificates.Services.Implementations;

// The service now uses Primary Constructor to inject all available strategies
public class InterestCalculationService(
    IEnumerable<ISeriesStrategy> strategies,
    SavingsBondDbContext db) : IInterestCalculationService
{
    public async Task<decimal> CalculateCurrentValueAsync(SavingsBond bond, CancellationToken ct = default)
    {
        // Select the appropriate strategy for the bond series
        var strategy = strategies.FirstOrDefault(s => s.CanHandle(bond.Series))
                       ?? throw new NotSupportedException($"Series {bond.Series} is not supported.");

        // The historical monthly base rates published by the IGCP for this series,
        // used so each quarter is calculated with the rate that was actually in
        // force at the time (see SeriesRateHistory / BaseSeriesStrategy).
        var rateHistory = await db.SeriesRateHistories
            .Where(r => r.Series == bond.Series)
            .AsNoTracking()
            .ToListAsync(ct);

        // Delegate the calculation to the specific strategy
        return strategy.CalculateInterest(bond, rateHistory);
    }
}
