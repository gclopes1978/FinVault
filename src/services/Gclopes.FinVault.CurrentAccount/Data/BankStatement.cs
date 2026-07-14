namespace Gclopes.FinVault.CurrentAccount.Data;

public class BankStatement
{
    public Guid Id { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal FinalBalance { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    // Relação 1-para-muitos
    public List<Movement> Movements { get; set; } = [];
}
