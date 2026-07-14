namespace Gclopes.FinVault.SavingsCertificates.Services.Strategies;

public class SeriesEStrategy : BaseSeriesStrategy
{
    public override bool CanHandle(string series) => series.Equals("E", StringComparison.OrdinalIgnoreCase);

    // According to IGCP rules for Series E, permanence premiums are applied annually after the first year.
    protected override decimal GetPermanencePremium(double years)
    {
        return years switch
        {
            < 1 => 0.0000m,
            < 2 => 0.0000m,
            < 6 => 0.0050m,  // 0.5% premium from 2nd to 5th year
            _ => 0.0100m   // 1.0% premium from 6th year onwards
        };
    }
}