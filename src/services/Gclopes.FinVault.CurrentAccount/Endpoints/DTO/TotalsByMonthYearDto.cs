namespace Gclopes.FinVault.CurrentAccount.Endpoints.DTO
{
    public record TotalsByMonthYearDto(int Year, int Month, decimal TotalCredit, decimal TotalDebit);
    
}
