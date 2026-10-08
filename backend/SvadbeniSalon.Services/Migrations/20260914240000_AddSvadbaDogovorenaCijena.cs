using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SvadbeniSalon.Services.Database;

#nullable disable

namespace SvadbeniSalon.Services.Migrations;

[DbContext(typeof(SvadbeniSalonDbContext))]
[Migration("20260914240000_AddSvadbaDogovorenaCijena")]
public partial class AddSvadbaDogovorenaCijena : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'dbo.Svadbe', N'DogovorenaCijena') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Svadbe]
                    ADD [DogovorenaCijena] DECIMAL(18,2) NOT NULL
                        CONSTRAINT [DF_Svadbe_DogovorenaCijena] DEFAULT (0);
            END

            UPDATE s
            SET s.[DogovorenaCijena] = p.[Cijena]
            FROM [dbo].[Svadbe] s
            INNER JOIN [dbo].[Ponude] p ON p.[Id] = s.[PonudaId]
            WHERE s.[DogovorenaCijena] = 0;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'dbo.Svadbe', N'DogovorenaCijena') IS NOT NULL
            BEGIN
                ALTER TABLE [dbo].[Svadbe] DROP CONSTRAINT [DF_Svadbe_DogovorenaCijena];
                ALTER TABLE [dbo].[Svadbe] DROP COLUMN [DogovorenaCijena];
            END
            """);
    }
}
