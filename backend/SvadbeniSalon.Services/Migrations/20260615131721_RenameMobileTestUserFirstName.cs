using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SvadbeniSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class RenameMobileTestUserFirstName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                column: "FirstName",
                value: "Dalila");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                column: "FirstName",
                value: "Mobile");
        }
    }
}
