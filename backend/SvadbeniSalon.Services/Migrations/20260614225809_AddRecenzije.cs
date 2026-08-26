using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SvadbeniSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddRecenzije : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recenzije",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PonudaId = table.Column<int>(type: "int", nullable: false),
                    SvadbaId = table.Column<int>(type: "int", nullable: false),
                    Ocjena = table.Column<int>(type: "int", nullable: false),
                    Komentar = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recenzije", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recenzije_Ponude_PonudaId",
                        column: x => x.PonudaId,
                        principalTable: "Ponude",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recenzije_Svadbe_SvadbaId",
                        column: x => x.SvadbaId,
                        principalTable: "Svadbe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recenzije_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recenzije_PonudaId",
                table: "Recenzije",
                column: "PonudaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recenzije_SvadbaId",
                table: "Recenzije",
                column: "SvadbaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recenzije_UserId",
                table: "Recenzije",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recenzije");
        }
    }
}
