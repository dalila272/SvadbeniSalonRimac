using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SvadbeniSalon.Database.Migrations
{
    public partial class MeniArtikal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Zanrovi",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 966, DateTimeKind.Utc).AddTicks(5571),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 688, DateTimeKind.Utc).AddTicks(8952));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Muzicari",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 966, DateTimeKind.Utc).AddTicks(2927),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 688, DateTimeKind.Utc).AddTicks(6342));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Korisnici",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 955, DateTimeKind.Utc).AddTicks(9416),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 678, DateTimeKind.Utc).AddTicks(2524));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Dekoracije",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 975, DateTimeKind.Utc).AddTicks(8329),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 696, DateTimeKind.Utc).AddTicks(5000));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Artikli",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 976, DateTimeKind.Utc).AddTicks(3484),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 696, DateTimeKind.Utc).AddTicks(7642));

            migrationBuilder.CreateTable(
                name: "Meniji",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cijena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 976, DateTimeKind.Utc).AddTicks(7555)),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meniji", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeniArtikli",
                columns: table => new
                {
                    ArtikalId = table.Column<long>(type: "bigint", nullable: false),
                    MeniId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeniArtikli", x => new { x.MeniId, x.ArtikalId });
                    table.ForeignKey(
                        name: "FK_MeniArtikli_Artikli_ArtikalId",
                        column: x => x.ArtikalId,
                        principalTable: "Artikli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeniArtikli_Meniji_MeniId",
                        column: x => x.MeniId,
                        principalTable: "Meniji",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeniArtikli_ArtikalId",
                table: "MeniArtikli",
                column: "ArtikalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeniArtikli");

            migrationBuilder.DropTable(
                name: "Meniji");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Zanrovi",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 688, DateTimeKind.Utc).AddTicks(8952),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 966, DateTimeKind.Utc).AddTicks(5571));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Muzicari",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 688, DateTimeKind.Utc).AddTicks(6342),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 966, DateTimeKind.Utc).AddTicks(2927));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Korisnici",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 678, DateTimeKind.Utc).AddTicks(2524),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 955, DateTimeKind.Utc).AddTicks(9416));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Dekoracije",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 696, DateTimeKind.Utc).AddTicks(5000),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 975, DateTimeKind.Utc).AddTicks(8329));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Artikli",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 696, DateTimeKind.Utc).AddTicks(7642),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 976, DateTimeKind.Utc).AddTicks(3484));
        }
    }
}
