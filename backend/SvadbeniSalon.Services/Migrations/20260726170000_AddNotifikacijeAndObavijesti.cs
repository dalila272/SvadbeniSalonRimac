using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SvadbeniSalon.Services.Database;

#nullable disable

namespace SvadbeniSalon.Services.Migrations;

[DbContext(typeof(SvadbeniSalonDbContext))]
[Migration("20260726170000_AddNotifikacijeAndObavijesti")]
public partial class AddNotifikacijeAndObavijesti : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
