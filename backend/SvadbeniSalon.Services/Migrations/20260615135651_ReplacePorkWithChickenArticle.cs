using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SvadbeniSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePorkWithChickenArticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Artikli",
                keyColumn: "Id",
                keyValue: 2,
                column: "Naziv",
                value: "Pileći file");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Artikli",
                keyColumn: "Id",
                keyValue: 2,
                column: "Naziv",
                value: "Svinjski file");
        }
    }
}
