namespace Gclopes.FinVault.Investments.Data.Entities;

public class XtbTransaction
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string Instrument { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
