using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SvadbeniSalon.Services.Database;

#nullable disable

namespace SvadbeniSalon.Services.Migrations;

[DbContext(typeof(SvadbeniSalonDbContext))]
[Migration("20260726140000_SeedDesktopTestUser")]
public partial class SeedDesktopTestUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = N'desktop')
            BEGIN
                INSERT INTO Users (CreatedAt, Email, FirstName, IsActive, LastName, PasswordHash, PasswordSalt, Username)
                VALUES (
                    '2026-07-26T00:00:00Z',
                    N'desktop@salon.local',
                    N'Desktop',
                    1,
                    N'Korisnik',
                    N'xObgsmDrWZgNlmGJsdxoRS+V/AE=',
                    N'0k57QETJENx5BkJuK4wtZg==',
                    N'desktop'
                );

                INSERT INTO UserRoles (DateAssigned, RoleId, UserId)
                SELECT '2026-07-26T00:00:00Z', 3, Id
                FROM Users
                WHERE Username = N'desktop';
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM UserRoles
            WHERE UserId IN (SELECT Id FROM Users WHERE Username = N'desktop');
            DELETE FROM Users WHERE Username = N'desktop';
            """);
    }
}
