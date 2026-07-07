namespace Gclopes.FinVault.CurrentAccount.Data;

public class Movement
{
    public Guid Id { get; set; }
    public Guid BankStatementId { get; set; } // FK para o extrato
    // Propriedade de navegação (ajuda o EF a entender a relação)
    public BankStatement BankStatement { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // "C" ou "D"
    public decimal Balance { get; set; }

    public string Description { get; set; } = string.Empty;
    public string? UserNote { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime ValueDate { get; set; }
    public DateTime CreatedAt { get; set; }

}