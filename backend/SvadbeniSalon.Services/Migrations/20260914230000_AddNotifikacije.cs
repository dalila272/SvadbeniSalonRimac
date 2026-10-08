using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SvadbeniSalon.Services.Database;

#nullable disable

namespace SvadbeniSalon.Services.Migrations;

[DbContext(typeof(SvadbeniSalonDbContext))]
[Migration("20260914230000_AddNotifikacije")]
public partial class AddNotifikacije : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[dbo].[Notifikacije]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Notifikacije] (
                    [Id] INT IDENTITY(1,1) NOT NULL,
                    [UserId] INT NOT NULL,
                    [Naslov] NVARCHAR(200) NOT NULL,
                    [Tekst] NVARCHAR(2000) NOT NULL,
                    [Kind] NVARCHAR(50) NULL,
                    [IsRead] BIT NOT NULL CONSTRAINT [DF_Notifikacije_IsRead] DEFAULT (0),
                    [CreatedAt] DATETIME2 NOT NULL,
                    CONSTRAINT [PK_Notifikacije] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Notifikacije_Users_UserId]
                        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
                );

                CREATE INDEX [IX_Notifikacije_UserId_IsRead]
                    ON [dbo].[Notifikacije] ([UserId], [IsRead]);
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[dbo].[Notifikacije]', N'U') IS NOT NULL
                DROP TABLE [dbo].[Notifikacije];
            """);
    }
}
