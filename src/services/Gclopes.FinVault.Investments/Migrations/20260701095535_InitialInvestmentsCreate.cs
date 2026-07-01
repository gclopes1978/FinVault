using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gclopes.FinVault.Investments.Migrations
{
    /// <inheritdoc />
    public partial class InitialInvestmentsCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Source = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    AverageCost = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTrading212s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    T212Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Ticker = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NumberOfShares = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    PricePerShare = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    CurrencyPricePerShare = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    CurrencyTotal = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    WithholdingTax = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    CurrencyWithholdingTax = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTrading212s", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XtbTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ticker = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Instrument = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Product = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XtbTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Symbol_Source",
                table: "Positions",
                columns: new[] { "Symbol", "Source" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTrading212s_T212Id",
                table: "TransactionTrading212s",
                column: "T212Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XtbTransactions_Id",
                table: "XtbTransactions",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "TransactionTrading212s");

            migrationBuilder.DropTable(
                name: "XtbTransactions");
        }
    }
}
