namespace Gclopes.FinVault.Investments.Data.Entities;

public class XtbCashOperation
{
    public int Id { get; set; }

    public string Type { get; set; } = string.Empty;
    public string Instrument { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public DateTime Time { get; set; }

    public decimal Amount { get; set; }

    public string OperationId { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public string Product { get; set; } = string.Empty;

    public string? PositionId { get; set; }

    public DateTime CreatedAt { get; set; }
}