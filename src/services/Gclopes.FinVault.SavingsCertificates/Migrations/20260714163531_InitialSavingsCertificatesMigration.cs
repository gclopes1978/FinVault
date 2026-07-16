using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gclopes.FinVault.SavingsCertificates.Migrations
{
    /// <inheritdoc />
    public partial class InitialSavingsCertificatesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavingsBonds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Series = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    BaseRate = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    SubscriptionDate = table.Column<DateTime>(type: "date", nullable: false),
                    UnitValue = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    // FIX: Quantity is an int column; precision/scale only apply to decimal
                    // columns and were mistakenly copy-pasted from BaseRate/UnitValue above.
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsBonds", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavingsBonds");
        }
    }
}
