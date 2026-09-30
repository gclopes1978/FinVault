namespace Gclopes.FinVault.CurrentAccount.Endpoints.DTO
{
    public record MovementTotalsByYearMonthResponse(int Year, int Month, decimal TotalCredit, decimal TotalDebit);
}
