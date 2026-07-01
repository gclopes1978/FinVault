namespace Gclopes.FinVault.Investments.Data.Entities;

public class Position
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public BrokerSource Source { get; set; }
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal CurrentValue => Quantity * CurrentPrice;
    public DateTime LastUpdated { get; set; }
    public DateTime CreatedAt { get; set; }

}