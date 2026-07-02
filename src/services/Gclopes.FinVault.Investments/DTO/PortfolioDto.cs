namespace Gclopes.FinVault.Investments.DTO;

public class PortfolioDto
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal CurrentValue { get; set; }
    public DateTime LastUpdated { get; set; }
}
