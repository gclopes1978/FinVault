namespace Gclopes.FinVault.CurrentAccount.Data
{
    public class Movement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty; // "Credit" ou "Debit"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}