using Gclopes.FinVault.SavingsCertificates.Data.Entities;
using Gclopes.FinVault.SavingsCertificates.Services.Interfaces;

// FIX: This file had no namespace declaration, so the class was landing in the global
// namespace instead of Services.Strategies (inconsistent with SeriesEStrategy/SeriesFStrategy,
// which live in this namespace and rely on unqualified access to this base class).
namespace Gclopes.FinVault.SavingsCertificates.Services.Strategies;

public abstract class BaseSeriesStrategy : ISeriesStrategy
{
    protected const int MonthsPerQuarter = 3;

    public abstract bool CanHandle(string series);

    public decimal CalculateInterest(SavingsBond bond)
    {
        var totalInvested = bond.UnitValue * bond.Quantity;
        var yearsHeld = (DateTime.UtcNow - bond.SubscriptionDate).TotalDays / 365.25;

        // Determine the permanence premium based on the number of years held
        var premium = GetPermanencePremium(yearsHeld);

        // Get base rate (BaseRate + Euribor if applicable)
        var baseRate = GetBaseRate(bond);

        // Apply 28% tax retention (net value = 72% of gross)
        var effectiveRate = (baseRate + premium) * 0.72m;
        var quarterlyRate = effectiveRate / 4;

        // Calculate number of quarters elapsed
        var quarters = (int)((DateTime.UtcNow - bond.SubscriptionDate).TotalDays / 91.25);

        if (quarters <= 0) return 0m;

        // Compound interest calculation: A = P(1 + r/n)^(nt)
        var accumulatedValue = totalInvested * (decimal)Math.Pow((double)(1 + quarterlyRate), quarters);
        return Math.Round(accumulatedValue - totalInvested, 2);
    }

    // Common implementation for all series
    protected virtual decimal GetBaseRate(SavingsBond bond) => bond.BaseRate;
    protected abstract decimal GetPermanencePremium(double years);
}