namespace Gclopes.FinVault.PensionPlans.Data
{
    public class PensionFundMovementEntity
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        
        public decimal Valuation { get; set; }

        // Audit field: when this record was effectively processed in the Bank's system
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
