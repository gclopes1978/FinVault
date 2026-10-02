using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gclopes.FinVault.Investments.Migrations
{
    /// <inheritdoc />
    public partial class AddTrading212ExcelFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrencyConversionFee",
                table: "Trading212Transactions",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCurrencyConversionFee",
                table: "Trading212Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyStampDutyReserveTax",
                table: "Trading212Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "StampDutyReserveTax",
                table: "Trading212Transactions",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyConversionFee",
                table: "Trading212Transactions");

            migrationBuilder.DropColumn(
                name: "CurrencyCurrencyConversionFee",
                table: "Trading212Transactions");

            migrationBuilder.DropColumn(
                name: "CurrencyStampDutyReserveTax",
                table: "Trading212Transactions");

            migrationBuilder.DropColumn(
                name: "StampDutyReserveTax",
                table: "Trading212Transactions");
        }
    }
}
