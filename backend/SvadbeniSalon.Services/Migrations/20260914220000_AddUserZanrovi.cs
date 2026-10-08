using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SvadbeniSalon.Services.Database;

#nullable disable

namespace SvadbeniSalon.Services.Migrations;

[DbContext(typeof(SvadbeniSalonDbContext))]
[Migration("20260914220000_AddUserZanrovi")]
public partial class AddUserZanrovi : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[dbo].[UserZanrovi]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[UserZanrovi] (
                    [UserId] INT NOT NULL,
                    [ZanrId] INT NOT NULL,
                    CONSTRAINT [PK_UserZanrovi] PRIMARY KEY ([UserId], [ZanrId]),
                    CONSTRAINT [FK_UserZanrovi_Users_UserId]
                        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_UserZanrovi_Zanrovi_ZanrId]
                        FOREIGN KEY ([ZanrId]) REFERENCES [dbo].[Zanrovi] ([Id]) ON DELETE CASCADE
                );
            END

            -- Demo interesi za test klijenta mobile (Pop, Rock)
            IF EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Id] = 6)
            BEGIN
                INSERT INTO [dbo].[UserZanrovi] ([UserId], [ZanrId])
                SELECT 6, z.[Id]
                FROM (VALUES (1), (2)) AS z([Id])
                WHERE EXISTS (SELECT 1 FROM [dbo].[Zanrovi] WHERE [Id] = z.[Id])
                  AND NOT EXISTS (
                      SELECT 1 FROM [dbo].[UserZanrovi] uz
                      WHERE uz.[UserId] = 6 AND uz.[ZanrId] = z.[Id]
                  );
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[dbo].[UserZanrovi]', N'U') IS NOT NULL
                DROP TABLE [dbo].[UserZanrovi];
            """);
    }
}
