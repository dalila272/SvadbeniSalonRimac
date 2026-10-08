using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SvadbeniSalon.Services.Database;

#nullable disable

namespace SvadbeniSalon.Services.Migrations;

/// <summary>
/// Obogaćuje katalog i seed ocjene/interese da recommender daje smislenije matching razloge.
/// </summary>
[DbContext(typeof(SvadbeniSalonDbContext))]
[Migration("20261008143000_EnrichRecommenderCatalogSeed")]
public partial class EnrichRecommenderCatalogSeed : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DECLARE @seed datetime2 = '2026-06-14T00:00:00Z';

            UPDATE Zanrovi SET IsActive = 0 WHERE Id = 3 AND Naziv = N'Balet';

            UPDATE Muzicari SET Naziv = N'Hit Parade Bend', Opis = N'Live pop i evergreen hitovi za plesni dio večeri' WHERE Id = 1;
            UPDATE Muzicari SET Naziv = N'Electric Avenue', Opis = N'Energičan rock bend za zabavniju svadbenu atmosferu' WHERE Id = 2;
            UPDATE Muzicari SET Naziv = N'Kvartet Armonija', Opis = N'Gudački kvartet za ceremoniju i elegantni doček' WHERE Id = 3;

            SET IDENTITY_INSERT Muzicari ON;
            IF NOT EXISTS (SELECT 1 FROM Muzicari WHERE Id = 4)
                INSERT INTO Muzicari (Id, Naziv, Opis, IsActive, CreatedAt, UpdatedAt)
                VALUES (4, N'Sevdah Ansambl', N'Narodna i starogradska muzika uživo', 1, @seed, NULL);
            IF NOT EXISTS (SELECT 1 FROM Muzicari WHERE Id = 5)
                INSERT INTO Muzicari (Id, Naziv, Opis, IsActive, CreatedAt, UpdatedAt)
                VALUES (5, N'Jazz Club Trio', N'Lounge i swing jazz tokom večere', 1, @seed, NULL);
            IF NOT EXISTS (SELECT 1 FROM Muzicari WHERE Id = 6)
                INSERT INTO Muzicari (Id, Naziv, Opis, IsActive, CreatedAt, UpdatedAt)
                VALUES (6, N'Duo Tišina', N'Akustični duo za intimne trenutke i evergreen klasike', 1, @seed, NULL);
            SET IDENTITY_INSERT Muzicari OFF;

            DELETE FROM MuzicarZanrovi WHERE MuzicarId IN (1,2,3,4,5,6);
            INSERT INTO MuzicarZanrovi (MuzicarId, ZanrId) VALUES
                (1, 1), (1, 9),
                (2, 2),
                (3, 5),
                (4, 6), (4, 7),
                (5, 4),
                (6, 8), (6, 9);

            UPDATE Dekoracije SET Opis = N'Bijele ruže, hortenzije i svijeće za romantičan ambijent' WHERE Id = 1;
            UPDATE Dekoracije SET Opis = N'Luksuzni zlatni detalji, kristalne vase i LED akcenti' WHERE Id = 2;
            UPDATE Dekoracije SET Opis = N'Drvo, laneno platno i suho cvijeće za tradicionalni ugođaj' WHERE Id = 3;

            UPDATE Ponude SET Opis = N'Luksuzna večer: premium meni, zlatna dekoracija, pop/evergreen bend i jazz trio' WHERE Id = 1;
            UPDATE Ponude SET Opis = N'Elegantna klasika: klasični meni, cvjetna dekoracija, gudači i akustični duo' WHERE Id = 2;
            UPDATE Ponude SET Opis = N'Tradicionalna svadba: narodna i starogradska muzika, rustik dekoracija' WHERE Id = 3;
            UPDATE Ponude SET Opis = N'Zabavna večer: pop i rock bend, cvjetna dekoracija, klasični meni' WHERE Id = 4;

            DELETE FROM MuzicarPonuda WHERE PonudaId IN (1,2,3,4);
            INSERT INTO MuzicarPonuda (PonudaId, MuzicarId) VALUES
                (1, 1), (1, 5),
                (2, 3), (2, 6),
                (3, 4),
                (4, 1), (4, 2);

            -- Klijenti za collaborative filtering (lozinka: test)
            SET IDENTITY_INSERT Users ON;
            IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = 9)
                INSERT INTO Users (Id, CreatedAt, Email, FirstName, IsActive, LastName, PasswordHash, PasswordSalt, Username)
                VALUES (9, @seed, N'ana.popovic@salon.local', N'Ana', 1, N'Popović',
                        N'N5b4vpOtGo4txmR/IoPFNoRg1kY=', N'JopMnUSdt7Cec4gKUV0rag==', N'ana.popovic');
            IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = 10)
                INSERT INTO Users (Id, CreatedAt, Email, FirstName, IsActive, LastName, PasswordHash, PasswordSalt, Username)
                VALUES (10, @seed, N'marko.softic@salon.local', N'Marko', 1, N'Softić',
                        N'N5b4vpOtGo4txmR/IoPFNoRg1kY=', N'JopMnUSdt7Cec4gKUV0rag==', N'marko.softic');
            IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = 11)
                INSERT INTO Users (Id, CreatedAt, Email, FirstName, IsActive, LastName, PasswordHash, PasswordSalt, Username)
                VALUES (11, @seed, N'ena.kovac@salon.local', N'Ena', 1, N'Kovač',
                        N'N5b4vpOtGo4txmR/IoPFNoRg1kY=', N'JopMnUSdt7Cec4gKUV0rag==', N'ena.kovac');
            SET IDENTITY_INSERT Users OFF;

            SET IDENTITY_INSERT UserRoles ON;
            IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Id = 9)
                INSERT INTO UserRoles (Id, DateAssigned, RoleId, UserId) VALUES (9, @seed, 2, 9);
            IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Id = 10)
                INSERT INTO UserRoles (Id, DateAssigned, RoleId, UserId) VALUES (10, @seed, 2, 10);
            IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Id = 11)
                INSERT INTO UserRoles (Id, DateAssigned, RoleId, UserId) VALUES (11, @seed, 2, 11);
            SET IDENTITY_INSERT UserRoles OFF;

            -- Interesi (mobile: Pop + Evergreen radi jasnog demo matchinga)
            DELETE FROM UserZanrovi WHERE UserId = 6;
            INSERT INTO UserZanrovi (UserId, ZanrId) VALUES (6, 1), (6, 9);
            IF NOT EXISTS (SELECT 1 FROM UserZanrovi WHERE UserId = 9 AND ZanrId = 6)
                INSERT INTO UserZanrovi (UserId, ZanrId) VALUES (9, 6);
            IF NOT EXISTS (SELECT 1 FROM UserZanrovi WHERE UserId = 9 AND ZanrId = 7)
                INSERT INTO UserZanrovi (UserId, ZanrId) VALUES (9, 7);
            IF NOT EXISTS (SELECT 1 FROM UserZanrovi WHERE UserId = 10 AND ZanrId = 5)
                INSERT INTO UserZanrovi (UserId, ZanrId) VALUES (10, 5);
            IF NOT EXISTS (SELECT 1 FROM UserZanrovi WHERE UserId = 10 AND ZanrId = 4)
                INSERT INTO UserZanrovi (UserId, ZanrId) VALUES (10, 4);
            IF NOT EXISTS (SELECT 1 FROM UserZanrovi WHERE UserId = 11 AND ZanrId = 1)
                INSERT INTO UserZanrovi (UserId, ZanrId) VALUES (11, 1);
            IF NOT EXISTS (SELECT 1 FROM UserZanrovi WHERE UserId = 11 AND ZanrId = 2)
                INSERT INTO UserZanrovi (UserId, ZanrId) VALUES (11, 2);

            SET IDENTITY_INSERT Svadbe ON;
            IF NOT EXISTS (SELECT 1 FROM Svadbe WHERE Id = 104)
                INSERT INTO Svadbe (Id, UserId, PonudaId, DogovorenaCijena, DatumSvadbe, Vrijeme, BrojGostiju, BrojRata, Status, Napomena, CreatedAt, UpdatedAt)
                VALUES (104, 9, 3, 7000, '2025-07-12', '17:00:00', 90, 1, 3, N'Tradicionalna svadba — zadovoljni narodnim ansamblom.', @seed, NULL);
            IF NOT EXISTS (SELECT 1 FROM Svadbe WHERE Id = 105)
                INSERT INTO Svadbe (Id, UserId, PonudaId, DogovorenaCijena, DatumSvadbe, Vrijeme, BrojGostiju, BrojRata, Status, Napomena, CreatedAt, UpdatedAt)
                VALUES (105, 10, 2, 10000, '2025-08-23', '18:00:00', 110, 2, 3, N'Elegantna večer uz gudače.', @seed, NULL);
            IF NOT EXISTS (SELECT 1 FROM Svadbe WHERE Id = 106)
                INSERT INTO Svadbe (Id, UserId, PonudaId, DogovorenaCijena, DatumSvadbe, Vrijeme, BrojGostiju, BrojRata, Status, Napomena, CreatedAt, UpdatedAt)
                VALUES (106, 11, 4, 8500, '2025-10-05', '19:00:00', 100, 1, 3, N'Zabavna večer uz pop/rock bend.', @seed, NULL);
            IF NOT EXISTS (SELECT 1 FROM Svadbe WHERE Id = 107)
                INSERT INTO Svadbe (Id, UserId, PonudaId, DogovorenaCijena, DatumSvadbe, Vrijeme, BrojGostiju, BrojRata, Status, Napomena, CreatedAt, UpdatedAt)
                VALUES (107, 11, 1, 15000, '2024-11-16', '18:30:00', 160, 2, 3, N'Prethodna luksuzna svadba u porodici.', @seed, NULL);
            SET IDENTITY_INSERT Svadbe OFF;

            SET IDENTITY_INSERT Recenzije ON;
            IF NOT EXISTS (SELECT 1 FROM Recenzije WHERE Id = 102)
                INSERT INTO Recenzije (Id, UserId, PonudaId, SvadbaId, Ocjena, Komentar, CreatedAt)
                VALUES (102, 9, 3, 104, 5, N'Sevdah Ansambl je napravio atmosferu — baš onako kako smo htjeli za tradicionalnu svadbu.', @seed);
            IF NOT EXISTS (SELECT 1 FROM Recenzije WHERE Id = 103)
                INSERT INTO Recenzije (Id, UserId, PonudaId, SvadbaId, Ocjena, Komentar, CreatedAt)
                VALUES (103, 10, 2, 105, 5, N'Kvartet Armonija i Duo Tišina — elegantno od dočeka do večere.', @seed);
            IF NOT EXISTS (SELECT 1 FROM Recenzije WHERE Id = 104)
                INSERT INTO Recenzije (Id, UserId, PonudaId, SvadbaId, Ocjena, Komentar, CreatedAt)
                VALUES (104, 11, 4, 106, 5, N'Gosti su plesali cijelu noć uz pop i rock bend.', @seed);
            IF NOT EXISTS (SELECT 1 FROM Recenzije WHERE Id = 105)
                INSERT INTO Recenzije (Id, UserId, PonudaId, SvadbaId, Ocjena, Komentar, CreatedAt)
                VALUES (105, 11, 1, 107, 4, N'Gold paket je luksuzan; jazz trio tokom večere je bio hit.', @seed);
            IF NOT EXISTS (SELECT 1 FROM Recenzije WHERE Id = 106)
                INSERT INTO Recenzije (Id, UserId, PonudaId, SvadbaId, Ocjena, Komentar, CreatedAt)
                VALUES (106, 9, 2, NULL, 3, N'Elegantno, ali nama više odgovara narodni ugođaj.', @seed);
            SET IDENTITY_INSERT Recenzije OFF;

            DECLARE @maxMuz int = (SELECT ISNULL(MAX(Id), 0) FROM Muzicari);
            DECLARE @maxUser int = (SELECT ISNULL(MAX(Id), 0) FROM Users);
            DECLARE @maxUr int = (SELECT ISNULL(MAX(Id), 0) FROM UserRoles);
            DECLARE @maxSv int = (SELECT ISNULL(MAX(Id), 0) FROM Svadbe);
            DECLARE @maxRec int = (SELECT ISNULL(MAX(Id), 0) FROM Recenzije);
            DBCC CHECKIDENT ('Muzicari', RESEED, @maxMuz);
            DBCC CHECKIDENT ('Users', RESEED, @maxUser);
            DBCC CHECKIDENT ('UserRoles', RESEED, @maxUr);
            DBCC CHECKIDENT ('Svadbe', RESEED, @maxSv);
            DBCC CHECKIDENT ('Recenzije', RESEED, @maxRec);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Jednosmjerno obogaćivanje seed podataka — Down namjerno prazan.
    }
}
