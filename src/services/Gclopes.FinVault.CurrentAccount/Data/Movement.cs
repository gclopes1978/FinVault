namespace Gclopes.FinVault.CurrentAccount.Data;

public class Movement
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // "Credit" or "Debit"
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } 
    public DateTime ValueDate { get; set; }
    public DateTime CreatedAt { get; set; }
}