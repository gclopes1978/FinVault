namespace Gclopes.FinVault.Investments.Data.Entities;

public class XtbOpenPosition
{
    public int Id { get; set; }

    public string Product { get; set; } = string.Empty;
    public string InstrumentPosition { get; set; } = string.Empty;

    public string Ticker { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public decimal Volume { get; set; }
    public decimal Value { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal OpenPrice { get; set; }

    public DateTime OpenTimeUtc { get; set; }

    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }

    public decimal? NetProfitPercent { get; set; }
    public decimal NetProfit { get; set; }
    public decimal GrossProfit { get; set; }

    public decimal Margin { get; set; }
    public decimal OpenCommission { get; set; }
    public decimal Swap { get; set; }
    public decimal Rollover { get; set; }

    public DateTime CreatedAt { get; set; }
}