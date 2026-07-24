namespace Gclopes.FinVault.CurrentAccount.Endpoints.DTO;

// Generic envelope for any paginated list response returned by this service
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
