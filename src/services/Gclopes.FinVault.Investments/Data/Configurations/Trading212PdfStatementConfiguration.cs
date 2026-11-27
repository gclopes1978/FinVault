using Gclopes.FinVault.Investments.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gclopes.FinVault.Investments.Data.Configurations;

public class Trading212PdfStatementConfiguration
    : IEntityTypeConfiguration<Trading212PdfStatement>
{
    public void Configure(EntityTypeBuilder<Trading212PdfStatement> entity)
    {
        entity.HasKey(statement => statement.Id);

        entity.Property(statement => statement.SourceDocumentId)
            .HasMaxLength(128)
            .IsRequired();

        entity.HasIndex(statement => statement.SourceDocumentId)
            .IsUnique();

        entity.Property(statement => statement.DataJson)
            .IsRequired();
    }
}
