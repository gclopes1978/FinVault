using Gclopes.FinVault.Investments.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.Investments.Data;

public class FinVaultInvestmentsDb(
    DbContextOptions<FinVaultInvestmentsDb> options)
    : DbContext(options)
{
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<XtbTransaction> XtbTransactions => Set<XtbTransaction>();
    public DbSet<Trading212Transaction> Trading212Transactions => Set<Trading212Transaction>();

    public DbSet<XtbClosedPosition> XtbClosedPositions => Set<XtbClosedPosition>();
    public DbSet<XtbCashOperation> XtbCashOperations => Set<XtbCashOperation>();
    public DbSet<XtbOpenPosition> XtbOpenPositions => Set<XtbOpenPosition>();

    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<decimal>()
            .HavePrecision(18, 8);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FinVaultInvestmentsDb).Assembly);
    }
}