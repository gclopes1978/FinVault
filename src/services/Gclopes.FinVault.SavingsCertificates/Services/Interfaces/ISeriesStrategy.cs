using Gclopes.FinVault.SavingsCertificates.Data.Entities;

namespace Gclopes.FinVault.SavingsCertificates.Services.Interfaces;

public interface ISeriesStrategy
{
    bool CanHandle(string series);

    // rateHistory should contain the bond's series' published monthly base rates
    // (any order). Quarters that fall in a month missing from the history fall back
    // to bond.BaseRate (today's snapshot rate) so the calculation still works before
    // the history has been fully populated.
    decimal CalculateInterest(SavingsBond bond, IReadOnlyCollection<SeriesRateHistory> rateHistory);
}
