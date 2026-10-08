using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SvadbeniSalon.Services.Database;

#nullable disable

namespace SvadbeniSalon.Services.Migrations;

[DbContext(typeof(SvadbeniSalonDbContext))]
[Migration("20260928140000_RecenzijaOptionalSvadba")]
public partial class RecenzijaOptionalSvadba : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            -- Allow pre-wedding package ratings (SvadbaId optional).
            IF EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'IX_Recenzije_SvadbaId' AND object_id = OBJECT_ID(N'[dbo].[Recenzije]')
            )
            BEGIN
                DROP INDEX [IX_Recenzije_SvadbaId] ON [dbo].[Recenzije];
            END

            IF COL_LENGTH(N'dbo.Recenzije', N'SvadbaId') IS NOT NULL
            BEGIN
                ALTER TABLE [dbo].[Recenzije] ALTER COLUMN [SvadbaId] INT NULL;
            END

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'IX_Recenzije_SvadbaId' AND object_id = OBJECT_ID(N'[dbo].[Recenzije]')
            )
            BEGIN
                CREATE UNIQUE INDEX [IX_Recenzije_SvadbaId]
                    ON [dbo].[Recenzije] ([SvadbaId])
                    WHERE [SvadbaId] IS NOT NULL;
            END

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'IX_Recenzije_UserId_PonudaId' AND object_id = OBJECT_ID(N'[dbo].[Recenzije]')
            )
            BEGIN
                CREATE UNIQUE INDEX [IX_Recenzije_UserId_PonudaId]
                    ON [dbo].[Recenzije] ([UserId], [PonudaId]);
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'IX_Recenzije_UserId_PonudaId' AND object_id = OBJECT_ID(N'[dbo].[Recenzije]')
            )
            BEGIN
                DROP INDEX [IX_Recenzije_UserId_PonudaId] ON [dbo].[Recenzije];
            END

            IF EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'IX_Recenzije_SvadbaId' AND object_id = OBJECT_ID(N'[dbo].[Recenzije]')
            )
            BEGIN
                DROP INDEX [IX_Recenzije_SvadbaId] ON [dbo].[Recenzije];
            END

            -- Cannot alter to NOT NULL if nulls exist; fill with dummy only in Down for empty cases.
            UPDATE [dbo].[Recenzije] SET [SvadbaId] = 0 WHERE [SvadbaId] IS NULL;

            ALTER TABLE [dbo].[Recenzije] ALTER COLUMN [SvadbaId] INT NOT NULL;

            CREATE UNIQUE INDEX [IX_Recenzije_SvadbaId]
                ON [dbo].[Recenzije] ([SvadbaId]);
            """);
    }
}
