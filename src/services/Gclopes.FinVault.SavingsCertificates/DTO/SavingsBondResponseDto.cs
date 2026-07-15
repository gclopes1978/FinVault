namespace Gclopes.FinVault.SavingsCertificates.DTO
{
    public record SavingsBondResponseDto(
    Guid Id,
    string Series,
    decimal BaseRate,
    DateTime SubscriptionDate,
    decimal UnitValue,
    int Quantity,
    decimal AccumulatedInterest);
}
