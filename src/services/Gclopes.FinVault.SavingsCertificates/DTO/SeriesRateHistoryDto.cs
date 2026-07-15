namespace Gclopes.FinVault.SavingsCertificates.DTO
{
    // EffectiveMonth can be any day of the month; only year/month are stored
    // (normalized to the 1st) since the IGCP publishes one rate per calendar month.
    public record SeriesRateHistoryDto(
    string Series,
    DateOnly EffectiveMonth,
    decimal BaseRate);
}
