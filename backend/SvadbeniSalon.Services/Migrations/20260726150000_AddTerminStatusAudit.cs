using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SvadbeniSalon.Services.Database;

#nullable disable

namespace SvadbeniSalon.Services.Migrations;

[DbContext(typeof(SvadbeniSalonDbContext))]
[Migration("20260726150000_AddTerminStatusAudit")]
public partial class AddTerminStatusAudit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "StatusChangedByUserId",
            table: "Svadbe",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "StatusChangedAt",
            table: "Svadbe",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "StatusChangeReason",
            table: "Svadbe",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "StatusChangedByUserId",
            table: "DnevniSastanci",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "StatusChangedAt",
            table: "DnevniSastanci",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "StatusChangeReason",
            table: "DnevniSastanci",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Svadbe_StatusChangedByUserId",
            table: "Svadbe",
            column: "StatusChangedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_DnevniSastanci_StatusChangedByUserId",
            table: "DnevniSastanci",
            column: "StatusChangedByUserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Svadbe_Users_StatusChangedByUserId",
            table: "Svadbe",
            column: "StatusChangedByUserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_DnevniSastanci_Users_StatusChangedByUserId",
            table: "DnevniSastanci",
            column: "StatusChangedByUserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        // Unique: jedan aktivan termin po datumu (Pending/Confirmed).
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IX_Svadbe_DatumSvadbe_Active
            ON Svadbe (DatumSvadbe)
            WHERE Status IN (0, 1);
            """);

        // Unique: jedan aktivan sastanak po tačnom terminu.
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IX_DnevniSastanci_DatumSastanka_Active
            ON DnevniSastanci (DatumSastanka)
            WHERE Status IN (0, 1);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Svadbe_DatumSvadbe_Active ON Svadbe;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS IX_DnevniSastanci_DatumSastanka_Active ON DnevniSastanci;");

        migrationBuilder.DropForeignKey(
            name: "FK_Svadbe_Users_StatusChangedByUserId",
            table: "Svadbe");

        migrationBuilder.DropForeignKey(
            name: "FK_DnevniSastanci_Users_StatusChangedByUserId",
            table: "DnevniSastanci");

        migrationBuilder.DropIndex(
            name: "IX_Svadbe_StatusChangedByUserId",
            table: "Svadbe");

        migrationBuilder.DropIndex(
            name: "IX_DnevniSastanci_StatusChangedByUserId",
            table: "DnevniSastanci");

        migrationBuilder.DropColumn(name: "StatusChangedByUserId", table: "Svadbe");
        migrationBuilder.DropColumn(name: "StatusChangedAt", table: "Svadbe");
        migrationBuilder.DropColumn(name: "StatusChangeReason", table: "Svadbe");

        migrationBuilder.DropColumn(name: "StatusChangedByUserId", table: "DnevniSastanci");
        migrationBuilder.DropColumn(name: "StatusChangedAt", table: "DnevniSastanci");
        migrationBuilder.DropColumn(name: "StatusChangeReason", table: "DnevniSastanci");
    }
}
