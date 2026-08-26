using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SvadbeniSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddSalonEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artikli",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    Cijena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artikli", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dekoracije",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Cijena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dekoracije", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Meniji",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Cijena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meniji", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Muzicari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Muzicari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zanrovi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zanrovi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeniArtikli",
                columns: table => new
                {
                    MeniId = table.Column<int>(type: "int", nullable: false),
                    ArtikalId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Ponude",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Cijena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MeniId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MuzicarZanrovi",
                columns: table => new
                {
                    MuzicarId = table.Column<int>(type: "int", nullable: false),
                    ZanrId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuzicarZanrovi", x => new { x.MuzicarId, x.ZanrId });
                    table.ForeignKey(
                        name: "FK_MuzicarZanrovi_Muzicari_MuzicarId",
                        column: x => x.MuzicarId,
                        principalTable: "Muzicari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MuzicarZanrovi_Zanrovi_ZanrId",
                        column: x => x.ZanrId,
                        principalTable: "Zanrovi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DekoracijaPonuda",
                columns: table => new
                {
                    PonudaId = table.Column<int>(type: "int", nullable: false),
                    DekoracijaId = table.Column<int>(type: "int", nullable: false)
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
                name: "MuzicarPonuda",
                columns: table => new
                {
                    PonudaId = table.Column<int>(type: "int", nullable: false),
                    MuzicarId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuzicarPonuda", x => new { x.PonudaId, x.MuzicarId });
                    table.ForeignKey(
                        name: "FK_MuzicarPonuda_Muzicari_MuzicarId",
                        column: x => x.MuzicarId,
                        principalTable: "Muzicari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MuzicarPonuda_Ponude_PonudaId",
                        column: x => x.PonudaId,
                        principalTable: "Ponude",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Artikli",
                columns: new[] { "Id", "Cijena", "CreatedAt", "IsActive", "Naziv", "Tip", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 15m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Teleća pršuta", 1, null },
                    { 2, 18m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Svinjski file", 1, null },
                    { 3, 25m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Vino crno", 2, null },
                    { 4, 5m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Sok od narandže", 2, null }
                });

            migrationBuilder.InsertData(
                table: "Dekoracije",
                columns: new[] { "Id", "Cijena", "CreatedAt", "IsActive", "Naziv", "Opis", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 500m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Cvjetna dekoracija", "Bijeli ruže i hortenzije", null },
                    { 2, 800m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Zlatna dekoracija", "Luksuzni zlatni detalji", null },
                    { 3, 350m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Rustik dekoracija", "Prirodni drveni elementi", null }
                });

            migrationBuilder.InsertData(
                table: "Meniji",
                columns: new[] { "Id", "Cijena", "CreatedAt", "IsActive", "Naziv", "Opis", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 35m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Klasični meni", "Tradicionalni svadbeni meni", null },
                    { 2, 55m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Premium meni", "Premium jela i pića", null }
                });

            migrationBuilder.InsertData(
                table: "Muzicari",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Naziv", "Opis", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Michael Jackson", "Pop legenda", null },
                    { 2, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "The Beatles", "Rock klasici", null },
                    { 3, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Pjotr Iljič Čajkovski", "Balet muzika", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastLoginAt", "LastName", "PasswordHash", "PasswordSalt", "PhoneNumber", "ProfileImageBase64", "Username" },
                values: new object[,]
                {
                    { 6, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), "mobile@salon.local", "Mobile", true, null, "Korisnik", "N5b4vpOtGo4txmR/IoPFNoRg1kY=", "JopMnUSdt7Cec4gKUV0rag==", null, null, "mobile" },
                    { 7, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), "desktop@salon.local", "Desktop", true, null, "Korisnik", "xObgsmDrWZgNlmGJsdxoRS+V/AE=", "0k57QETJENx5BkJuK4wtZg==", null, null, "desktop" },
                    { 8, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), "admin@salon.local", "Admin", true, null, "Korisnik", "qU2ck45AOJU9W8CVxAO89FyOb8M=", "65z57pEcbOuw+c9Ma3X10Q==", null, null, "admin" }
                });

            migrationBuilder.InsertData(
                table: "Zanrovi",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Naziv" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Pop" },
                    { 2, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Rock" },
                    { 3, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, "Balet" }
                });

            migrationBuilder.InsertData(
                table: "MeniArtikli",
                columns: new[] { "ArtikalId", "MeniId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 3, 1 },
                    { 2, 2 },
                    { 4, 2 }
                });

            migrationBuilder.InsertData(
                table: "MuzicarZanrovi",
                columns: new[] { "MuzicarId", "ZanrId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 }
                });

            migrationBuilder.InsertData(
                table: "Ponude",
                columns: new[] { "Id", "Cijena", "CreatedAt", "IsActive", "MeniId", "Naziv", "Opis", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 15000m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, 2, "Gold paket", "Luksuzni paket sa premium menijem i zlatnom dekoracijom", null },
                    { 2, 10000m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, "Silver paket", "Elegantan paket sa klasičnim menijem", null },
                    { 3, 7000m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, "Bronze paket", "Osnovni paket za manje proslave", null },
                    { 4, 8500m, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, "Regular paket", "Standardni paket sa cvjetnom dekoracijom", null }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "DateAssigned", "RoleId", "UserId" },
                values: new object[,]
                {
                    { 6, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), 2, 6 },
                    { 7, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), 2, 7 },
                    { 8, new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Utc), 1, 8 }
                });

            migrationBuilder.InsertData(
                table: "DekoracijaPonuda",
                columns: new[] { "DekoracijaId", "PonudaId" },
                values: new object[,]
                {
                    { 2, 1 },
                    { 1, 2 },
                    { 3, 3 },
                    { 1, 4 }
                });

            migrationBuilder.InsertData(
                table: "MuzicarPonuda",
                columns: new[] { "MuzicarId", "PonudaId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 1, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DekoracijaPonuda_DekoracijaId",
                table: "DekoracijaPonuda",
                column: "DekoracijaId");

            migrationBuilder.CreateIndex(
                name: "IX_MeniArtikli_ArtikalId",
                table: "MeniArtikli",
                column: "ArtikalId");

            migrationBuilder.CreateIndex(
                name: "IX_MuzicarPonuda_MuzicarId",
                table: "MuzicarPonuda",
                column: "MuzicarId");

            migrationBuilder.CreateIndex(
                name: "IX_MuzicarZanrovi_ZanrId",
                table: "MuzicarZanrovi",
                column: "ZanrId");

            migrationBuilder.CreateIndex(
                name: "IX_Ponude_MeniId",
                table: "Ponude",
                column: "MeniId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DekoracijaPonuda");

            migrationBuilder.DropTable(
                name: "MeniArtikli");

            migrationBuilder.DropTable(
                name: "MuzicarPonuda");

            migrationBuilder.DropTable(
                name: "MuzicarZanrovi");

            migrationBuilder.DropTable(
                name: "Dekoracije");

            migrationBuilder.DropTable(
                name: "Artikli");

            migrationBuilder.DropTable(
                name: "Ponude");

            migrationBuilder.DropTable(
                name: "Muzicari");

            migrationBuilder.DropTable(
                name: "Zanrovi");

            migrationBuilder.DropTable(
                name: "Meniji");

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
