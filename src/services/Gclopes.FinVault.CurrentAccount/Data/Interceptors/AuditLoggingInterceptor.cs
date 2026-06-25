using System.Text.Json;
using Gclopes.FinVault.CurrentAccount.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Gclopes.FinVault.CurrentAccount.Data.Interceptors
{
    // Implement a custom SaveChangesInterceptor to automatically trap database modification events
    public class AuditLoggingInterceptor : SaveChangesInterceptor
    {
        // Override the asynchronous saving changes pipeline method to execute custom tracking logic
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            // Verify if the active database context pipeline instance is available before processing
            if (eventData.Context == null)
            {
                // Bypass execution immediately returning base result if context is completely empty
                return await base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            // Extract the database context instance from the event metadata layer parameters
            var context = eventData.Context;

            // Query the tracker memory to isolate all managed tracking records that are being mutated
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLog &&
                            (e.State == EntityState.Added ||
                             e.State == EntityState.Modified ||
                             e.State == EntityState.Deleted))
                .ToList();

            // Check if there are any modification entry items requiring auditing records tracking
            if (entries.Count == 0)
            {
                // Exit processing quickly if no entities match the targeted tracking state constraints
                return await base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            // Initialize an internal data list container layer to hold newly computed audit logs
            var auditLogs = new List<AuditLog>();
            // Establish a current centralized tracking universal record time marker calculation index
            var currentTimestamp = DateTime.UtcNow;
            // Define a placeholder variable to simulate or inject an active system user context
            var operationalUser = "system-user";

            // Enumerate over each intercepted tracking row state item discovered inside memory state
            foreach (var entry in entries)
            {
                // Create a temporary state dictionary to log property property tracking payload schemas
                var changes = new Dictionary<string, object?>();
                // Extract the string representation metadata value identifying the entity runtime class
                var entityName = entry.Entity.GetType().Name;
                // Capture the core state mutation operation name based on structural enum codes
                var action = entry.State.ToString();

                // Evaluate processing paths if the entity state matches inbound item creation records
                if (entry.State == EntityState.Added)
                {
                    // Enumerate all properties to archive data initialization parameters into the schema
                    foreach (var property in entry.CurrentValues.Properties)
                    {
                        // Store the current inbound data state attributes into the changes payload log
                        changes[property.Name] = entry.CurrentValues[property];
                    }
                }
                // Evaluate processing paths if the entity state matches inbound item deletion records
                else if (entry.State == EntityState.Deleted)
                {
                    // Enumerate all original properties to archive data snapshot information into logs
                    foreach (var property in entry.OriginalValues.Properties)
                    {
                        // Store the original property values inside the dictionary collection trace
                        changes[property.Name] = entry.OriginalValues[property];
                    }
                }
                // Evaluate processing paths if the entity state matches outbound active modification tracks
                else if (entry.State == EntityState.Modified)
                {
                    // Enumerate all properties to filter out exact specific fields updated during save
                    foreach (var property in entry.OriginalValues.Properties)
                    {
                        // Fetch the current working value applied over the target tracking runtime environment
                        var currentValue = entry.CurrentValues[property.Name];
                        // Fetch the original base value preserved inside tracking memory configurations
                        var originalValue = entry.OriginalValues[property.Name];

                        // Compare values to determine if the selected field property was genuinely transformed
                        if (!Equals(currentValue, originalValue))
                        {
                            // Capture historical audit trails tracking old values alongside new parameters
                            changes[property.Name] = new { Old = originalValue, New = currentValue };
                        }
                    }
                }

                // Construct a new instance record tracking model for the historical audit registry
                var log = new AuditLog
                {
                    // Generate a cryptographically strong unique tracker sequential guid database key
                    Id = Guid.NewGuid(),
                    // Inject the string name metadata describing the subject database object mapping
                    EntityName = entityName,
                    // Inject the action operation verb mapping confirming the code trace execution pattern
                    Action = action,
                    // Assign the tracked user identifier executing the active context pipeline process
                    UserId = operationalUser,
                    // Apply the synchronized universal logging creation tracking clock execution index
                    Timestamp = currentTimestamp,
                    // Convert the tracking changes data dictionary into a clean compact JSON string string
                    ChangesPayload = JsonSerializer.Serialize(changes)
                };

                // Add the newly constructed logging record parameters into the output collection list
                auditLogs.Add(log);
            }

            // Append the range collection of audit records safely back into the context memory pool
            context.Set<AuditLog>().AddRange(auditLogs);

            // Forward execution processing controls out to the underlying framework data commit layers
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}