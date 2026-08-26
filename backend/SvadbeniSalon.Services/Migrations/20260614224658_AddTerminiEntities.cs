using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SvadbeniSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddTerminiEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DnevniSastanci",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DatumSastanka = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Napomena = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DnevniSastanci", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DnevniSastanci_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Svadbe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PonudaId = table.Column<int>(type: "int", nullable: false),
                    DatumSvadbe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Vrijeme = table.Column<TimeSpan>(type: "time", nullable: false),
                    BrojGostiju = table.Column<int>(type: "int", nullable: false),
                    BrojRata = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Napomena = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Svadbe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Svadbe_Ponude_PonudaId",
                        column: x => x.PonudaId,
                        principalTable: "Ponude",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Svadbe_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DnevniSastanci_UserId",
                table: "DnevniSastanci",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Svadbe_PonudaId",
                table: "Svadbe",
                column: "PonudaId");

            migrationBuilder.CreateIndex(
                name: "IX_Svadbe_UserId",
                table: "Svadbe",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DnevniSastanci");

            migrationBuilder.DropTable(
                name: "Svadbe");
        }
    }
}
