using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SvadbeniSalon.Database.Migrations
{
    public partial class Ponuda : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Zanrovi",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 835, DateTimeKind.Utc).AddTicks(6361),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 966, DateTimeKind.Utc).AddTicks(5571));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Muzicari",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 835, DateTimeKind.Utc).AddTicks(3207),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 966, DateTimeKind.Utc).AddTicks(2927));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Meniji",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 843, DateTimeKind.Utc).AddTicks(4358),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 976, DateTimeKind.Utc).AddTicks(7555));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Korisnici",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 824, DateTimeKind.Utc).AddTicks(6190),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 955, DateTimeKind.Utc).AddTicks(9416));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Dekoracije",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 842, DateTimeKind.Utc).AddTicks(9334),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 975, DateTimeKind.Utc).AddTicks(8329));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Artikli",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 843, DateTimeKind.Utc).AddTicks(1911),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 976, DateTimeKind.Utc).AddTicks(3484));

            migrationBuilder.CreateTable(
                name: "Ponude",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cijena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MeniId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 843, DateTimeKind.Utc).AddTicks(6513)),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ponude", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ponude_Meniji_MeniId",
                        column: x => x.MeniId,
                        principalTable: "Meniji",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DekoracijaPonuda",
                columns: table => new
                {
                    PonudaId = table.Column<long>(type: "bigint", nullable: false),
                    DekoracijaId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DekoracijaPonuda", x => new { x.PonudaId, x.DekoracijaId });
                    table.ForeignKey(
                        name: "FK_DekoracijaPonuda_Dekoracije_DekoracijaId",
                        column: x => x.DekoracijaId,
                        principalTable: "Dekoracije",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DekoracijaPonuda_Ponude_PonudaId",
                        column: x => x.PonudaId,
                        principalTable: "Ponude",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MuzicarDekoracija",
                columns: table => new
                {
                    PonudaId = table.Column<long>(type: "bigint", nullable: false),
                    MuzicarId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuzicarDekoracija", x => new { x.PonudaId, x.MuzicarId });
                    table.ForeignKey(
                        name: "FK_MuzicarDekoracija_Muzicari_MuzicarId",
                        column: x => x.MuzicarId,
                        principalTable: "Muzicari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MuzicarDekoracija_Ponude_PonudaId",
                        column: x => x.PonudaId,
                        principalTable: "Ponude",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DekoracijaPonuda_DekoracijaId",
                table: "DekoracijaPonuda",
                column: "DekoracijaId");

            migrationBuilder.CreateIndex(
                name: "IX_MuzicarDekoracija_MuzicarId",
                table: "MuzicarDekoracija",
                column: "MuzicarId");

            migrationBuilder.CreateIndex(
                name: "IX_Ponude_MeniId",
                table: "Ponude",
                column: "MeniId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DekoracijaPonuda");

            migrationBuilder.DropTable(
                name: "MuzicarDekoracija");

            migrationBuilder.DropTable(
                name: "Ponude");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Zanrovi",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 966, DateTimeKind.Utc).AddTicks(5571),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 835, DateTimeKind.Utc).AddTicks(6361));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Muzicari",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 966, DateTimeKind.Utc).AddTicks(2927),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 835, DateTimeKind.Utc).AddTicks(3207));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Meniji",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 976, DateTimeKind.Utc).AddTicks(7555),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 843, DateTimeKind.Utc).AddTicks(4358));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Korisnici",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 955, DateTimeKind.Utc).AddTicks(9416),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 824, DateTimeKind.Utc).AddTicks(6190));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Dekoracije",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 975, DateTimeKind.Utc).AddTicks(8329),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 842, DateTimeKind.Utc).AddTicks(9334));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Artikli",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 59, 20, 976, DateTimeKind.Utc).AddTicks(3484),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 29, 19, 22, 43, 843, DateTimeKind.Utc).AddTicks(1911));
        }
    }
}
