using Gclopes.FinVault.SavingsCertificates.Data.Entities;
using Gclopes.FinVault.SavingsCertificates.Services.Interfaces;

namespace Gclopes.FinVault.SavingsCertificates.Services.Strategies;

public abstract class BaseSeriesStrategy : ISeriesStrategy
{
    public abstract bool CanHandle(string series);

    public decimal CalculateInterest(SavingsBond bond, IReadOnlyCollection<SeriesRateHistory> rateHistory)
    {
        var totalInvested = bond.UnitValue * bond.Quantity;
        var currentCapital = totalInvested;
        var today = DateTime.UtcNow;

        // Build a lookup of "first day of month" -> published base rate for this bond's
        // series, so each quarter can use the rate that was actually in force at the
        // time, instead of assuming bond.BaseRate (today's snapshot) applied throughout.
        var ratesByMonth = rateHistory
            .Where(r => r.Series.Equals(bond.Series, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(r => r.EffectiveMonth, r => r.BaseRate);

        // IGCP capitalizes interest quarterly, on the day of the month matching the
        // subscription's value date (e.g. subscribed on the 3rd => matures on the 3rd
        // of Feb/May/Aug/Nov...). Walking real calendar quarters (via AddMonths) instead
        // of a fixed 91-day approximation keeps this correct across leap years and
        // months of different lengths.
        var quarterIndex = 0;
        while (true)
        {
            var quarterStart = bond.SubscriptionDate.AddMonths(3 * quarterIndex);
            var quarterEnd = bond.SubscriptionDate.AddMonths(3 * (quarterIndex + 1));

            if (quarterEnd > today)
            {
                break;
            }

            var years = quarterIndex * 0.25;
            var premium = GetPermanencePremium(years);
            var baseRate = GetBaseRateForMonth(bond, quarterStart, ratesByMonth);

            // IGCP quarterly rate: (annual base rate + premium) / 4, both expressed as
            // fractions. baseRate/BaseRate is stored as a percentage (e.g. 3.403 =>
            // 3.403%), so it must be divided by 100 on its own. The premium returned by
            // GetPermanencePremium is already a fraction (e.g. 0.0050m => 0.50%).
            var quarterlyRate = (baseRate / 100m) / 4m + premium / 4m;

            // IGCP capitalizes the interest NET of the 28% IRS withholding tax every
            // quarter (not gross interest with tax deducted only once at the end).
            var quarterlyGrossInterest = currentCapital * quarterlyRate;
            var quarterlyNetInterest = Math.Round(quarterlyGrossInterest * 0.72m, 2, MidpointRounding.AwayFromZero);

            currentCapital += quarterlyNetInterest;
            quarterIndex++;
        }

        var totalNetInterest = currentCapital - totalInvested;

        return Math.Round(totalNetInterest, 2);
    }

    // Looks up the rate published for the month a quarter starts in. Falls back to
    // bond.BaseRate (today's snapshot) when the history hasn't been populated for that
    // month yet, so calculations keep working while the table is being filled in.
    private static decimal GetBaseRateForMonth(
        SavingsBond bond,
        DateTime quarterStart,
        IReadOnlyDictionary<DateOnly, decimal> ratesByMonth)
    {
        var month = new DateOnly(quarterStart.Year, quarterStart.Month, 1);
        return ratesByMonth.TryGetValue(month, out var rate) ? rate : bond.BaseRate;
    }

    protected abstract decimal GetPermanencePremium(double years);
}
