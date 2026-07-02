using Gclopes.FinVault.Investments.Data;

namespace Gclopes.FinVault.Investments.DTO;

public class TransactionDto
{
    public string Id { get; set; } = string.Empty; // Mudado para string porque os IDs da T212 e XTB são formatos diferentes
    public string Symbol { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "BUY", "SELL", "MARKET BUY", "DIVIDEND", etc.
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
    public BrokerSource Source { get; set; }
    public DateTime TransactionDate { get; set; }
}
