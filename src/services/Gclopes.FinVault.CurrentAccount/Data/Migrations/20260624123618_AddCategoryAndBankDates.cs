using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gclopes.FinVault.CurrentAccount.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndBankDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Movements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionDate",
                table: "Movements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ValueDate",
                table: "Movements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Movements");

            migrationBuilder.DropColumn(
                name: "TransactionDate",
                table: "Movements");

            migrationBuilder.DropColumn(
                name: "ValueDate",
                table: "Movements");
        }
    }
}
