using Gclopes.FinVault.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.Identity.Data;

/// <summary>
/// Database context for the Identity microservice, managing users, roles and API clients.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    /// <summary>
    /// Gets the collection of registered API clients.
    /// </summary>
    public DbSet<ApiClient> ApiClients => Set<ApiClient>();

    /// <summary>
    /// Configures entity relationships and database constraints.
    /// </summary>
    /// <param name="builder">The model builder used to configure the database model.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Ensure each API client has a unique client identifier.
        builder.Entity<ApiClient>(entity =>
        {
            entity.HasKey(client => client.Id);

            entity.HasIndex(client => client.ClientId)
                .IsUnique();

            entity.Property(client => client.ClientId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(client => client.ClientSecretHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(client => client.Name)
                .IsRequired()
                .HasMaxLength(200);
        });
    }
}