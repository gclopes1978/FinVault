namespace Gclopes.FinVault.SavingsCertificates.Services.Strategies;

public class SeriesFStrategy : BaseSeriesStrategy
{
    public override bool CanHandle(string series) => series.Equals("F", StringComparison.OrdinalIgnoreCase);

    // Permanence premium for Series F based on the IGCP schedule.
    protected override decimal GetPermanencePremium(double years) => years switch
    {
        < 1 => 0.0000m,
        < 6 => 0.0025m, // 0.25% premium from 2nd to 5th year
        < 10 => 0.0050m, // 0.50% premium from 6th to 9th year
        < 12 => 0.0100m, // 1.00% premium from 10th to 11th year
        < 14 => 0.0150m, // 1.50% premium from 12th to 13th year
        _ => 0.0175m  // 1.75% premium from 14th year onwards
    };
}