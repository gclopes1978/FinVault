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
            // AuditLog configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ChangesPayload).IsRequired();
            });

            // Movement configuration (fixing the decimal precision warning)
            modelBuilder.Entity<Movement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(10);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}