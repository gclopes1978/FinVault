namespace Gclopes.FinVault.CurrentAccount.Endpoints.DTO;

// Query-string bound filter/pagination options for GET /api/movements.
// Bound automatically by ASP.NET Core minimal APIs via [AsParameters].
public class MovementQueryParameters
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    // 1-based page index; anything below 1 is coerced to 1
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    // Number of items per page; capped at MaxPageSize to protect the service from abusive requests
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    // Inclusive lower bound on TransactionDate
    public DateTime? TransactionDateFrom { get; set; }

    // Inclusive upper bound on TransactionDate
    public DateTime? TransactionDateTo { get; set; }

    // Inclusive lower bound on ValueDate
    public DateTime? ValueDateFrom { get; set; }

    // Inclusive upper bound on ValueDate
    public DateTime? ValueDateTo { get; set; }
}
