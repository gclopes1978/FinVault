namespace Gclopes.FinVault.SavingsCertificates.Services.Strategies;

public class SeriesEStrategy : BaseSeriesStrategy
{
    public override bool CanHandle(string series) => series.Equals("E", StringComparison.OrdinalIgnoreCase);

    // According to IGCP's official technical sheet for Series E:
    // "0.5% - from the beginning of the 2nd year until the end of the 5th year" (i.e. once 1 full
    // year has elapsed) and "1.0% - from the beginning of the 6th year until the end of the 10th year".
    protected override decimal GetPermanencePremium(double years)
    {
        return years switch
        {
            < 1 => 0.0000m,
            < 6 => 0.0050m,  // 0.5% premium from the 2nd to the 5th year
            _ => 0.0100m     // 1.0% premium from the 6th year onwards
        };
    }
}