namespace Gclopes.FinVault.PayrollAnalysis.Data;

public class PayrollImport
{
    public Guid Id { get; set; }

    // Identifier of the source PDF in the DocumentIngestion pipeline. Used as an idempotency key
    // so the same payslip is never persisted twice, even if RabbitMQ redelivers the message.
    public Guid SourceDocumentId { get; set; }

    public DateTime ImportedAt { get; set; }

    public ICollection<PayrollItem> Items { get; set; } = new List<PayrollItem>();
}
