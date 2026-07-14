namespace Gclopes.FinVault.SavingsCertificates.Data.Entities;

// FIX: This entity was missing from the repository entirely. Program.cs, the
// endpoints, the strategies and the already-generated EF migration all referenced
// "Gclopes.FinVault.SavingsCertificates.Data.Entities.SavingsBond", so the project
// could not compile. Recreated here matching the schema recorded in
// Migrations/SavingsBondDbContextModelSnapshot.cs.
public class SavingsBond
{
    public Guid Id { get; set; }

    // Series identifier, e.g. "E" or "F" (Certificados de Aforro).
    public string Series { get; set; } = string.Empty;

    public decimal BaseRate { get; set; }

    public DateTime SubscriptionDate { get; set; }

    public decimal UnitValue { get; set; }

    public int Quantity { get; set; }
}
