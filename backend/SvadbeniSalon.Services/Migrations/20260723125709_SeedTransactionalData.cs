using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SvadbeniSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class SeedTransactionalData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DnevniSastanci",
                columns: new[] { "Id", "CreatedAt", "DatumSastanka", "KontaktIme", "Napomena", "Status", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 101, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 10, 10, 0, 0, 0, DateTimeKind.Utc), null, "Dogovor oko menija i dekoracije.", 3, null, 6 },
                    { 102, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 5, 14, 0, 0, 0, DateTimeKind.Utc), null, "Klijent nije mogao doći.", 2, null, 6 },
                    { 103, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 20, 11, 0, 0, 0, DateTimeKind.Utc), null, "Razgled salona i dogovor detalja.", 1, null, 6 },
                    { 104, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 1, 9, 30, 0, 0, DateTimeKind.Utc), null, "Čeka se potvrda termina.", 0, null, 6 },
                    { 105, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 25, 15, 0, 0, 0, DateTimeKind.Utc), "Ana Marić", "Dogovor telefonom, dolazak izvana.", 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "Svadbe",
                columns: new[] { "Id", "BrojGostiju", "BrojRata", "CreatedAt", "DatumSvadbe", "Napomena", "PonudaId", "Status", "UpdatedAt", "UserId", "Vrijeme" },
                values: new object[,]
                {
                    { 101, 120, 2, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Svadba održana u septembru, sve po dogovoru.", 2, 3, null, 6, new TimeSpan(0, 17, 0, 0, 0) },
                    { 102, 150, 3, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Potvrđena rezervacija za oktobar.", 1, 1, null, 6, new TimeSpan(0, 18, 0, 0, 0) },
                    { 103, 80, 1, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Termin otkazan zbog promjene plana.", 4, 2, null, 6, new TimeSpan(0, 16, 30, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "Racuni",
                columns: new[] { "Id", "BrojRacuna", "CreatedAt", "DatumIzdavanja", "KreiraoUserId", "SvadbaId", "UkupanIznos", "UplaceniIznos" },
                values: new object[,]
                {
                    { 101, "RAC-2025-001", new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), 8, 101, 10000m, 10000m },
                    { 102, "RAC-2026-002", new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 5, 0, 0, 0, 0, DateTimeKind.Utc), 7, 102, 15000m, 5000m }
                });

            migrationBuilder.InsertData(
                table: "Rate",
                columns: new[] { "Id", "CreatedAt", "DatumUplate", "Iznos", "SvadbaId" },
                values: new object[,]
                {
                    { 101, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5000m, 101 },
                    { 102, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5000m, 101 },
                    { 103, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5000m, 102 }
                });

            migrationBuilder.InsertData(
                table: "Recenzije",
                columns: new[] { "Id", "CreatedAt", "Komentar", "Ocjena", "PonudaId", "SvadbaId", "UserId" },
                values: new object[] { 101, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Sve je bilo predivno. Osoblje ljubazno, meni odličan, a dekoracija baš kako smo zamislili.", 5, 2, 101, 6 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DnevniSastanci",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "DnevniSastanci",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "DnevniSastanci",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "DnevniSastanci",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "DnevniSastanci",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Racuni",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Racuni",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Rate",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Rate",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Rate",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Recenzije",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Svadbe",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Svadbe",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Svadbe",
                keyColumn: "Id",
                keyValue: 102);
        }
    }
}
