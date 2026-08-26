using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SvadbeniSalon.Services.Database;

#nullable disable

namespace SvadbeniSalon.Services.Migrations;

[DbContext(typeof(SvadbeniSalonDbContext))]
[Migration("20260726180000_RemoveNotifikacijeAndObavijesti")]
public partial class RemoveNotifikacijeAndObavijesti : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[dbo].[Notifikacije]', N'U') IS NOT NULL
                DROP TABLE [dbo].[Notifikacije];
            IF OBJECT_ID(N'[dbo].[Obavijesti]', N'U') IS NOT NULL
                DROP TABLE [dbo].[Obavijesti];
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // No-op.
    }
}
