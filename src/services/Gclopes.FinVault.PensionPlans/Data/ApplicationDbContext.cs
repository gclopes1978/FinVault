using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.PensionPlans.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<InsuranceMovementEntity> InsuranceMovementEntity => Set<InsuranceMovementEntity>();
        // Override configuring pipeline structures to register independent system interceptor hooks
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            // Execute default baseline lifecycle configurations inside the base database component framework
            base.OnConfiguring(optionsBuilder);
        }
    }
}
