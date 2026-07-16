namespace Gclopes.FinVault.SavingsCertificates.DTO
{
    public record SavingsBondCreateDto(
    string Series,
    decimal BaseRate,
    DateTime SubscriptionDate,
    decimal UnitValue,
    int Quantity);
}
