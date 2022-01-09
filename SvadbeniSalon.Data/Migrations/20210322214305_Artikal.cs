using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SvadbeniSalon.Database.Migrations
{
    public partial class Artikal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Zanrovi",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 688, DateTimeKind.Utc).AddTicks(8952),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 21, 41, 67, DateTimeKind.Utc).AddTicks(827));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Muzicari",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 688, DateTimeKind.Utc).AddTicks(6342),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 21, 41, 66, DateTimeKind.Utc).AddTicks(7871));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Korisnici",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 678, DateTimeKind.Utc).AddTicks(2524),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 21, 41, 56, DateTimeKind.Utc).AddTicks(44));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Dekoracije",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 696, DateTimeKind.Utc).AddTicks(5000),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 21, 41, 74, DateTimeKind.Utc).AddTicks(3759));

            migrationBuilder.CreateTable(
                name: "Artikli",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    Cijena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 696, DateTimeKind.Utc).AddTicks(7642)),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artikli", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Artikli");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Zanrovi",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 21, 41, 67, DateTimeKind.Utc).AddTicks(827),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 688, DateTimeKind.Utc).AddTicks(8952));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Muzicari",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 21, 41, 66, DateTimeKind.Utc).AddTicks(7871),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 688, DateTimeKind.Utc).AddTicks(6342));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Korisnici",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 21, 41, 56, DateTimeKind.Utc).AddTicks(44),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 678, DateTimeKind.Utc).AddTicks(2524));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Dekoracije",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 3, 22, 21, 21, 41, 74, DateTimeKind.Utc).AddTicks(3759),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 3, 22, 21, 43, 4, 696, DateTimeKind.Utc).AddTicks(5000));
        }
    }
}
