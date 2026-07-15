using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gclopes.FinVault.SavingsCertificates.Migrations
{
    /// <inheritdoc />
    public partial class AddSeriesRateHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeriesRateHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Series = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    EffectiveMonth = table.Column<DateOnly>(type: "date", nullable: false),
                    BaseRate = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesRateHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeriesRateHistories_Series_EffectiveMonth",
                table: "SeriesRateHistories",
                columns: new[] { "Series", "EffectiveMonth" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeriesRateHistories");
        }
    }
}
