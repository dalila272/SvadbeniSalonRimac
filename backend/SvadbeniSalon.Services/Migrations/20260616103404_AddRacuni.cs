using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SvadbeniSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddRacuni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Racuni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SvadbaId = table.Column<int>(type: "int", nullable: false),
                    BrojRacuna = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DatumIzdavanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UkupanIznos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UplaceniIznos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KreiraoUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Racuni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Racuni_Svadbe_SvadbaId",
                        column: x => x.SvadbaId,
                        principalTable: "Svadbe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Racuni_Users_KreiraoUserId",
                        column: x => x.KreiraoUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Racuni_BrojRacuna",
                table: "Racuni",
                column: "BrojRacuna",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Racuni_KreiraoUserId",
                table: "Racuni",
                column: "KreiraoUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Racuni_SvadbaId",
                table: "Racuni",
                column: "SvadbaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Racuni");
        }
    }
}
