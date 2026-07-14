
namespace Gclopes.FinVault.PensionPlans.Data
{
    public class PensionInsuranceMovementEntity
    {
        public Guid Id { get; set; }

        public string Movement { get; set; } = string.Empty;

        public string? Receipt { get; set; }

        public DateTime Date { get; set; }

        public decimal? Debit { get; set; }

        public decimal? Credit { get; set; }

        public decimal Balance { get; set; }

        // Audit field: when this record was effectively processed in the insurer's system
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
