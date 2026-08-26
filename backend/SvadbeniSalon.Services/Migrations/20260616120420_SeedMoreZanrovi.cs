using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SvadbeniSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class SeedMoreZanrovi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Zanrovi",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Naziv" },
                values: new object[,]
                {
                    { 4, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Jazz" },
                    { 5, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Klasična" },
                    { 6, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Narodna" },
                    { 7, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Starogradska" },
                    { 8, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Akustična" },
                    { 9, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Evergreen" },
                    { 10, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), false, "Elektronska" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Zanrovi",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Zanrovi",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Zanrovi",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Zanrovi",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Zanrovi",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Zanrovi",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Zanrovi",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
