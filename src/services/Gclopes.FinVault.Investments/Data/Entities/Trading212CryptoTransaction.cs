namespace Gclopes.FinVault.Investments.Data.Entities;

/// <summary>
/// Represents an executed cryptocurrency transaction from Trading212.
/// </summary>
public class Trading212CryptoTransaction
{
    public int Id { get; set; }

    public string ExecutionId { get; set; } = string.Empty;

    public string OrderId { get; set; } = string.Empty;

    public string AssetSymbol { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    public DateTime Time { get; set; }

    public decimal Quantity { get; set; }

    public decimal ExecutionPrice { get; set; }

    public decimal Value { get; set; }

    public decimal RealizedProfitLoss { get; set; }

    public DateTime CreatedAt { get; set; }
}