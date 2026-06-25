namespace Gclopes.FinVault.CurrentAccount.Models
{
    public class AuditLog
    {
        // Unique tracking identifier for this specific audit record log entry
        public Guid Id { get; set; }

        // Name of the domain entity model class being targeted or mutated
        public string EntityName { get; set; } = string.Empty;

        // Type of data operation mutation execution performed like Create, Update, or Delete
        public string Action { get; set; } = string.Empty;

        // Identifier tracking the specific authenticated system user who triggered this change
        public string UserId { get; set; } = string.Empty;

        // Accurate structural universal date and time marker when this operation occurred
        public DateTime Timestamp { get; set; }

        // JSON string block detailing the exact properties and values that were changed
        public string ChangesPayload { get; set; } = string.Empty;
    }
}
