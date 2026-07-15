using Gclopes.FinVault.SavingsCertificates.Data.Entities;
using Gclopes.FinVault.SavingsCertificates.Services.Interfaces;

namespace Gclopes.FinVault.SavingsCertificates.Services.Strategies;

public abstract class BaseSeriesStrategy : ISeriesStrategy
{
    protected const int DaysPerQuarter = 91; // IGCP uses 91 days for quarter calculation

    public abstract bool CanHandle(string series);

    public decimal CalculateInterest(SavingsBond bond)
    {
        var totalInvested = bond.UnitValue * bond.Quantity;
        var startDate = bond.SubscriptionDate;
        var endDate = DateTime.UtcNow;

        var currentCapital = totalInvested;
        var totalDays = (endDate - startDate).Days;

        // Number of completed quarters
        var totalQuarters = totalDays / DaysPerQuarter;

        for (int i = 0; i < totalQuarters; i++)
        {
            // Years elapsed at the START of this quarter, used for the permanence premium tier
            var years = i * (DaysPerQuarter / 365.0m);
            var premium = GetPermanencePremium((double)years);

            // IGCP quarterly rate: (annual base rate + premium) / 4, both expressed as fractions.
            // bond.BaseRate is stored as a percentage (e.g. 3.403 => 3.403%), so it must be divided
            // by 100 on its own. The premium returned by GetPermanencePremium is already a fraction
            // (e.g. 0.0050m => 0.50%). Adding it to bond.BaseRate BEFORE the /100 division (the
            // previous bug) shrank the premium's effect by a factor of 100.
            var quarterlyRate = (bond.BaseRate / 100m) / 4m + (decimal)premium / 4m;

            // IGCP capitalizes the interest NET of the 28% IRS withholding tax every quarter
            // (not gross interest with tax deducted only once at the end).
            var quarterlyGrossInterest = currentCapital * quarterlyRate;
            var quarterlyNetInterest = Math.Round(quarterlyGrossInterest * 0.72m, 2, MidpointRounding.AwayFromZero);

            currentCapital += quarterlyNetInterest;
        }

        var totalNetInterest = currentCapital - totalInvested;

        return Math.Round(totalNetInterest, 2);
    }

    protected virtual decimal GetBaseRate(SavingsBond bond) => bond.BaseRate;
    protected abstract decimal GetPermanencePremium(double years);
}