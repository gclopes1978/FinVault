using Gclopes.FinVault.CurrentAccount.Data.Interceptors;
using Gclopes.FinVault.CurrentAccount.Models;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.CurrentAccount.Data
{
    // Define the core application database context class managing tracking data mappings
    public class ApplicationDbContext : DbContext
    {
        // Initialize the base configuration constructor linking entity framework core database parameters
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Map the structural AuditLog data model out into a dedicated tracking database table schema
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        // Map the baseline financial transaction account movements records database table schema
        public DbSet<Movement> Movements => Set<Movement>();

        // Override configuring pipeline structures to register independent system interceptor hooks
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Register the custom generic audit logging interceptor directly into system configurations
            optionsBuilder.AddInterceptors(new AuditLoggingInterceptor());

            // Execute default baseline lifecycle configurations inside the base database component framework
            base.OnConfiguring(optionsBuilder);
        }

        // Configure explicit model constraint criteria configurations during initial creation assembly execution
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Target the AuditLog schema properties to set strict mapping configuration policies
            modelBuilder.Entity<AuditLog>(entity =>
            {
                // Establish the primary unique cluster index tracking identity key constraints
                entity.HasKey(e => e.Id);
                // Require the target entity context tracking metadata identity string name configuration
                entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
                // Require the execution transaction operational mutation description status string name
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                // Require the executing actor identity logging record tracking string name fields
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                // Enforce mandatory entry settings over the payload text data storage mapping layer
                entity.Property(e => e.ChangesPayload).IsRequired();
            });

            // Pass execution workflows onward downstream directly into the fundamental assembly generation process
            base.OnModelCreating(modelBuilder);
        }
    }
}