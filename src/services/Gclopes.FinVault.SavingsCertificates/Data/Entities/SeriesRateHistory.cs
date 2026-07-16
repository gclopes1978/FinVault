namespace Gclopes.FinVault.SavingsCertificates.Data.Entities;

// The IGCP redetermines the base rate for each Certificados de Aforro series every
// month (e.g. Series E: E3+1%, capped between 0% and 3.5%). A bond's accrued interest
// therefore depends on the rate that was actually in force during each historical
// quarter, not on today's snapshot rate stored on SavingsBond.BaseRate. This table
// holds that monthly history so BaseSeriesStrategy can look up the correct rate per
// quarter instead of assuming a single constant rate for the whole holding period.
public class SeriesRateHistory
{
    public int Id { get; set; }

    // Series identifier, e.g. "E" or "F".
    public string Series { get; set; } = string.Empty;

    // First day of the month during which this rate was in effect (IGCP publishes a
    // new rate on the antepenultimate business day of the previous month).
    public DateOnly EffectiveMonth { get; set; }

    public decimal BaseRate { get; set; }
}
