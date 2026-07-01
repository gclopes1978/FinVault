using Gclopes.FinVault.Investments.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.Investments.Data;

public class FinVaultInvestmentsDb(DbContextOptions<FinVaultInvestmentsDb> options) : DbContext(options)
{
    public DbSet<Position> Positions { get; set; }
    public DbSet<XtbTransaction> XtbTransactions { get; set; }
    public DbSet<TransactionTrading212> TransactionTrading212s { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Position entity
        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Symbol).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Quantity).HasPrecision(18, 8);
            entity.Property(e => e.AverageCost).HasPrecision(18, 8);
            entity.Property(e => e.CurrentPrice).HasPrecision(18, 8);
            entity.HasIndex(e => new { e.Symbol, e.Source }).IsUnique();
        });

        modelBuilder.Entity<XtbTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            // IMPORTANTE: Diz ao EF Core que NÃO é auto-incremento, pois vimos do CSV
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Ticker).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Instrument).HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(18, 8);
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.Product).HasMaxLength(50);

            // Criar um índice para a pesquisa ser ainda mais rápida
            entity.HasIndex(e => e.Id).IsUnique();
        });

        modelBuilder.Entity<TransactionTrading212>(entity =>
        {
            // Primary Key
            entity.HasKey(e => e.Id);

            // Configuração do ID da Trading212 (Obrigatório e Único para evitar duplicados)
            entity.Property(e => e.T212Id)
                  .IsRequired()
                  .HasMaxLength(50); // Os IDs da T212 são strings alfanuméricas
            entity.HasIndex(e => e.T212Id).IsUnique();

            // Strings de Texto
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50); // Ex: "Market buy", "Dividend"
            entity.Property(e => e.Isin).IsRequired().HasMaxLength(12);   // O padrão ISIN mundial tem exatamente 12 chars
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(20); // Ex: "AAPL", "MSFT_US"
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);             // Nullable por definição na entidade

            // Códigos de Moeda (ISO 4217 - EUR, USD, GBP...)
            entity.Property(e => e.CurrencyPricePerShare).IsRequired().HasMaxLength(3);
            entity.Property(e => e.CurrencyTotal).IsRequired().HasMaxLength(3);
            entity.Property(e => e.CurrencyWithholdingTax).IsRequired().HasMaxLength(3);

            // Campos Decimais (18,8 é o standard para ações fracionárias e taxas de câmbio precisas)
            entity.Property(e => e.NumberOfShares).HasPrecision(18, 8);
            entity.Property(e => e.PricePerShare).HasPrecision(18, 8);
            entity.Property(e => e.ExchangeRate).HasPrecision(18, 8);
            entity.Property(e => e.Total).HasPrecision(18, 8);
            entity.Property(e => e.WithholdingTax).HasPrecision(18, 8);
        });
    }
}