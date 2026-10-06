using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gclopes.FinVault.Investments.Migrations
{
    /// <inheritdoc />
    public partial class AddXtbExcelImportModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_XtbTransactions_Id",
                table: "XtbTransactions");

            migrationBuilder.CreateTable(
                name: "XtbCashOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Instrument = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Ticker = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    OperationId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Product = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PositionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XtbCashOperations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XtbClosedPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Instrument = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Ticker = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    OpenPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    OpenTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosePrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    CloseTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Product = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProfitLoss = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    GrossProfit = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    PurchaseValue = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    SaleValue = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    StopLoss = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    TakeProfit = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    Commission = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Margin = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Swap = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Rollover = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    OpenConversionRate = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    CloseConversionRate = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    CloseOrigin = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PositionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XtbClosedPositions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XtbOpenPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Product = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    InstrumentPosition = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Ticker = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    OpenPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    OpenTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StopLoss = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    TakeProfit = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    NetProfitPercent = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    NetProfit = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    GrossProfit = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Margin = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    OpenCommission = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Swap = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Rollover = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XtbOpenPositions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_XtbCashOperations_OperationId",
                table: "XtbCashOperations",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XtbCashOperations_Ticker",
                table: "XtbCashOperations",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_XtbCashOperations_Time",
                table: "XtbCashOperations",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_XtbClosedPositions_CloseTimeUtc",
                table: "XtbClosedPositions",
                column: "CloseTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_XtbClosedPositions_PositionId",
                table: "XtbClosedPositions",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_XtbClosedPositions_Ticker",
                table: "XtbClosedPositions",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_XtbOpenPositions_OpenTimeUtc",
                table: "XtbOpenPositions",
                column: "OpenTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_XtbOpenPositions_Ticker",
                table: "XtbOpenPositions",
                column: "Ticker");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "XtbCashOperations");

            migrationBuilder.DropTable(
                name: "XtbClosedPositions");

            migrationBuilder.DropTable(
                name: "XtbOpenPositions");

            migrationBuilder.CreateIndex(
                name: "IX_XtbTransactions_Id",
                table: "XtbTransactions",
                column: "Id",
                unique: true);
        }
    }
}
