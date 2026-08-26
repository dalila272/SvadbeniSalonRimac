using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SvadbeniSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminAndCustomerRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Roles 1/2 već postoje na starijim bazama.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = 1)
                BEGIN
                    SET IDENTITY_INSERT [Roles] ON;
                    INSERT INTO [Roles] ([Id], [CreatedAt], [Description], [IsActive], [Name])
                    VALUES (1, '2026-06-14T00:00:00.0000000Z', N'Administrator role with full permissions', 1, N'Admin');
                    SET IDENTITY_INSERT [Roles] OFF;
                END

                IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = 2)
                BEGIN
                    SET IDENTITY_INSERT [Roles] ON;
                    INSERT INTO [Roles] ([Id], [CreatedAt], [Description], [IsActive], [Name])
                    VALUES (2, '2026-06-14T00:00:00.0000000Z', N'Default customer role', 1, N'Customer');
                    SET IDENTITY_INSERT [Roles] OFF;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ne brišemo Role 1/2 — koriste ih UserRoles seed i postojeći korisnici.
        }
    }
}
