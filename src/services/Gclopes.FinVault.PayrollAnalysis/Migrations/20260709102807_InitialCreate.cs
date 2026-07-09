using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gclopes.FinVault.PayrollAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayrollImports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SourceDocumentId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollImports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PayrollImportId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Code = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Field = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    ReferenceMonth = table.Column<string>(type: "NVARCHAR2(7)", maxLength: 7, nullable: true),
                    Quantity = table.Column<decimal>(type: "DECIMAL(18,4)", precision: 18, scale: 4, nullable: true),
                    Rate = table.Column<decimal>(type: "DECIMAL(9,4)", precision: 9, scale: 4, nullable: true),
                    UnitValue = table.Column<decimal>(type: "DECIMAL(18,4)", precision: 18, scale: 4, nullable: true),
                    Amount = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollItems_PayrollImports_PayrollImportId",
                        column: x => x.PayrollImportId,
                        principalTable: "PayrollImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollImports_SourceDocumentId",
                table: "PayrollImports",
                column: "SourceDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollItems_PayrollImportId",
                table: "PayrollItems",
                column: "PayrollImportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollItems");

            migrationBuilder.DropTable(
                name: "PayrollImports");
        }
    }
}
