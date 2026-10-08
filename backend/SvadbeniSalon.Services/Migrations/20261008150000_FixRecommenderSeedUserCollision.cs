using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SvadbeniSalon.Services.Database;

#nullable disable

namespace SvadbeniSalon.Services.Migrations;

/// <summary>
/// Ispravlja sudar seed ID-eva (9/10/11) s pravim korisnicima — veže demo podatke po username-u.
/// </summary>
[DbContext(typeof(SvadbeniSalonDbContext))]
[Migration("20261008150000_FixRecommenderSeedUserCollision")]
public partial class FixRecommenderSeedUserCollision : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DECLARE @seed datetime2 = '2026-06-14T00:00:00Z';
            DECLARE @hash nvarchar(max) = N'N5b4vpOtGo4txmR/IoPFNoRg1kY=';
            DECLARE @salt nvarchar(max) = N'JopMnUSdt7Cec4gKUV0rag==';

            -- Ukloni pogrešno atribuirane seed recenzije/svadbe (veza na Id 9/10/11 = stvarni nalozi)
            DELETE FROM Recenzije WHERE Id BETWEEN 102 AND 106;
            DELETE FROM Svadbe WHERE Id BETWEEN 104 AND 107
              AND Napomena IN (
                N'Tradicionalna svadba — zadovoljni narodnim ansamblom.',
                N'Elegantna večer uz gudače.',
                N'Zabavna večer uz pop/rock bend.',
                N'Prethodna luksuzna svadba u porodici.');

            -- Demo klijenti po username-u (bez forsiranog Id)
            IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = N'ana.popovic')
                INSERT INTO Users (CreatedAt, Email, FirstName, IsActive, LastName, PasswordHash, PasswordSalt, Username)
                VALUES (@seed, N'ana.popovic@salon.local', N'Ana', 1, N'Popović', @hash, @salt, N'ana.popovic');

            IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = N'marko.softic')
                INSERT INTO Users (CreatedAt, Email, FirstName, IsActive, LastName, PasswordHash, PasswordSalt, Username)
                VALUES (@seed, N'marko.softic@salon.local', N'Marko', 1, N'Softić', @hash, @salt, N'marko.softic');

            IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = N'ena.kovac')
                INSERT INTO Users (CreatedAt, Email, FirstName, IsActive, LastName, PasswordHash, PasswordSalt, Username)
                VALUES (@seed, N'ena.kovac@salon.local', N'Ena', 1, N'Kovač', @hash, @salt, N'ena.kovac');

            DECLARE @ana int = (SELECT Id FROM Users WHERE Username = N'ana.popovic');
            DECLARE @marko int = (SELECT Id FROM Users WHERE Username = N'marko.softic');
            DECLARE @ena int = (SELECT Id FROM Users WHERE Username = N'ena.kovac');
            DECLARE @mobile int = (SELECT Id FROM Users WHERE Username = N'mobile');

            -- Customer role
            IF @ana IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @ana AND RoleId = 2)
                INSERT INTO UserRoles (DateAssigned, RoleId, UserId) VALUES (@seed, 2, @ana);
            IF @marko IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @marko AND RoleId = 2)
                INSERT INTO UserRoles (DateAssigned, RoleId, UserId) VALUES (@seed, 2, @marko);
            IF @ena IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @ena AND RoleId = 2)
                INSERT INTO UserRoles (DateAssigned, RoleId, UserId) VALUES (@seed, 2, @ena);

            -- Interesi
            IF @mobile IS NOT NULL
            BEGIN
                DELETE FROM UserZanrovi WHERE UserId = @mobile;
                INSERT INTO UserZanrovi (UserId, ZanrId) VALUES (@mobile, 1), (@mobile, 9); -- Pop, Evergreen
            END
            IF @ana IS NOT NULL
            BEGIN
                DELETE FROM UserZanrovi WHERE UserId = @ana;
                INSERT INTO UserZanrovi (UserId, ZanrId) VALUES (@ana, 6), (@ana, 7); -- Narodna, Starogradska
            END
            IF @marko IS NOT NULL
            BEGIN
                DELETE FROM UserZanrovi WHERE UserId = @marko;
                INSERT INTO UserZanrovi (UserId, ZanrId) VALUES (@marko, 5), (@marko, 4); -- Klasična, Jazz
            END
            IF @ena IS NOT NULL
            BEGIN
                DELETE FROM UserZanrovi WHERE UserId = @ena;
                INSERT INTO UserZanrovi (UserId, ZanrId) VALUES (@ena, 1), (@ena, 2); -- Pop, Rock
            END

            -- Završene svadbe + ocjene (samo ako još nema seed napomene)
            IF @ana IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Svadbe WHERE UserId = @ana AND Napomena LIKE N'Tradicionalna svadba%')
            BEGIN
                DECLARE @sAna int;
                INSERT INTO Svadbe (UserId, PonudaId, DogovorenaCijena, DatumSvadbe, Vrijeme, BrojGostiju, BrojRata, Status, Napomena, CreatedAt)
                VALUES (@ana, 3, 7000, '2025-07-12', '17:00:00', 90, 1, 3, N'Tradicionalna svadba — zadovoljni narodnim ansamblom.', @seed);
                SET @sAna = SCOPE_IDENTITY();
                INSERT INTO Recenzije (UserId, PonudaId, SvadbaId, Ocjena, Komentar, CreatedAt)
                VALUES (@ana, 3, @sAna, 5, N'Sevdah Ansambl je napravio atmosferu — baš onako kako smo htjeli za tradicionalnu svadbu.', @seed);
                INSERT INTO Recenzije (UserId, PonudaId, SvadbaId, Ocjena, Komentar, CreatedAt)
                VALUES (@ana, 2, NULL, 3, N'Elegantno, ali nama više odgovara narodni ugođaj.', @seed);
            END

            IF @marko IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Svadbe WHERE UserId = @marko AND Napomena LIKE N'Elegantna večer%')
            BEGIN
                DECLARE @sMarko int;
                INSERT INTO Svadbe (UserId, PonudaId, DogovorenaCijena, DatumSvadbe, Vrijeme, BrojGostiju, BrojRata, Status, Napomena, CreatedAt)
                VALUES (@marko, 2, 10000, '2025-08-23', '18:00:00', 110, 2, 3, N'Elegantna večer uz gudače.', @seed);
                SET @sMarko = SCOPE_IDENTITY();
                INSERT INTO Recenzije (UserId, PonudaId, SvadbaId, Ocjena, Komentar, CreatedAt)
                VALUES (@marko, 2, @sMarko, 5, N'Kvartet Armonija i Duo Tišina — elegantno od dočeka do večere.', @seed);
            END

            IF @ena IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Svadbe WHERE UserId = @ena AND Napomena LIKE N'Zabavna večer%')
            BEGIN
                DECLARE @sEna1 int;
                DECLARE @sEna2 int;
                INSERT INTO Svadbe (UserId, PonudaId, DogovorenaCijena, DatumSvadbe, Vrijeme, BrojGostiju, BrojRata, Status, Napomena, CreatedAt)
                VALUES (@ena, 4, 8500, '2025-10-05', '19:00:00', 100, 1, 3, N'Zabavna večer uz pop/rock bend.', @seed);
                SET @sEna1 = SCOPE_IDENTITY();
                INSERT INTO Svadbe (UserId, PonudaId, DogovorenaCijena, DatumSvadbe, Vrijeme, BrojGostiju, BrojRata, Status, Napomena, CreatedAt)
                VALUES (@ena, 1, 15000, '2024-11-16', '18:30:00', 160, 2, 3, N'Prethodna luksuzna svadba u porodici.', @seed);
                SET @sEna2 = SCOPE_IDENTITY();
                INSERT INTO Recenzije (UserId, PonudaId, SvadbaId, Ocjena, Komentar, CreatedAt)
                VALUES (@ena, 4, @sEna1, 5, N'Gosti su plesali cijelu noć uz pop i rock bend.', @seed);
                INSERT INTO Recenzije (UserId, PonudaId, SvadbaId, Ocjena, Komentar, CreatedAt)
                VALUES (@ena, 1, @sEna2, 4, N'Gold paket je luksuzan; jazz trio tokom večere je bio hit.', @seed);
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
