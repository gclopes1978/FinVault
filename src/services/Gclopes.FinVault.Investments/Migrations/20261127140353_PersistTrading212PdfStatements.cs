using Gclopes.FinVault.Investments.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gclopes.FinVault.Investments.Migrations;

[DbContext(typeof(FinVaultInvestmentsDb))]
[Migration("20261127140353_PersistTrading212PdfStatements")]
public partial class PersistTrading212PdfStatements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Trading212PdfStatements",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SourceDocumentId = table.Column<string>(
                    type: "nvarchar(128)",
                    maxLength: 128,
                    nullable: false),
                DataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ImportedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Trading212PdfStatements", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Trading212PdfStatements_SourceDocumentId",
            table: "Trading212PdfStatements",
            column: "SourceDocumentId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Trading212PdfStatements");
    }
}
