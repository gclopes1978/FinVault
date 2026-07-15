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
            // Years for the permanence premium calculation
            var years = i * (DaysPerQuarter / 365.0m);
            var premium = GetPermanencePremium((double)years);

            // IGCP quarterly rate calculation: (Base Rate + Premium) / 4
            // Assuming bond.BaseRate is the annual percentage (e.g., 3.403)
            var quarterlyRate = (bond.BaseRate + (decimal)premium) / 100m / 4m;

            // Capitalization: Interest is added to the principal
            var quarterlyInterest = currentCapital * quarterlyRate;
            currentCapital += quarterlyInterest;
        }

        var totalGrossInterest = currentCapital - totalInvested;
        var totalNetInterest = totalGrossInterest * 0.72m; // Applying 28% tax

        return Math.Round(totalNetInterest, 2);
    }

    protected virtual decimal GetBaseRate(SavingsBond bond) => bond.BaseRate;
    protected abstract decimal GetPermanencePremium(double years);
}