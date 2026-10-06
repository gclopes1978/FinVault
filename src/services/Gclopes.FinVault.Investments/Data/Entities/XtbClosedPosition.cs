namespace Gclopes.FinVault.Investments.Data.Entities;

public class XtbClosedPosition
{
    public int Id { get; set; }

    public string Instrument { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public decimal Volume { get; set; }
    public decimal OpenPrice { get; set; }
    public DateTime OpenTimeUtc { get; set; }

    public decimal ClosePrice { get; set; }
    public DateTime CloseTimeUtc { get; set; }

    public string Product { get; set; } = string.Empty;

    public decimal ProfitLoss { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal PurchaseValue { get; set; }
    public decimal SaleValue { get; set; }

    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }

    public decimal Commission { get; set; }
    public decimal Margin { get; set; }
    public decimal Swap { get; set; }
    public decimal Rollover { get; set; }

    public decimal? OpenConversionRate { get; set; }
    public decimal? CloseConversionRate { get; set; }

    public string? CloseOrigin { get; set; }

    public string PositionId { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}