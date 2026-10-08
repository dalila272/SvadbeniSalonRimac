using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Enums;
using SvadbeniSalon.Model.Constants;

namespace SvadbeniSalon.Services.Database;

public partial class SvadbeniSalonDbContext : DbContext
{
    private void CreateSalonSeed(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2026, 6, 14, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Zanr>().HasData(
            new { Id = 1, Naziv = "Pop", IsActive = true, CreatedAt = seedDate },
            new { Id = 2, Naziv = "Rock", IsActive = true, CreatedAt = seedDate },
            new { Id = 3, Naziv = "Balet", IsActive = true, CreatedAt = seedDate },
            new { Id = 4, Naziv = "Jazz", IsActive = true, CreatedAt = seedDate },
            new { Id = 5, Naziv = "Klasična", IsActive = true, CreatedAt = seedDate },
            new { Id = 6, Naziv = "Narodna", IsActive = true, CreatedAt = seedDate },
            new { Id = 7, Naziv = "Starogradska", IsActive = true, CreatedAt = seedDate },
            new { Id = 8, Naziv = "Akustična", IsActive = true, CreatedAt = seedDate },
            new { Id = 9, Naziv = "Evergreen", IsActive = true, CreatedAt = seedDate },
            new { Id = 10, Naziv = "Elektronska", IsActive = false, CreatedAt = seedDate }
        );

        modelBuilder.Entity<Muzicar>().HasData(
            new { Id = 1, Naziv = "Michael Jackson", Opis = "Pop legenda", IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 2, Naziv = "The Beatles", Opis = "Rock klasici", IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 3, Naziv = "Pjotr Iljič Čajkovski", Opis = "Balet muzika", IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null }
        );

        modelBuilder.Entity<MuzicarZanr>().HasData(
            new { MuzicarId = 1, ZanrId = 1 },
            new { MuzicarId = 2, ZanrId = 2 },
            new { MuzicarId = 3, ZanrId = 3 }
        );

        modelBuilder.Entity<Dekoracija>().HasData(
            new { Id = 1, Naziv = "Cvjetna dekoracija", Opis = "Bijeli ruže i hortenzije", Cijena = 500m, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 2, Naziv = "Zlatna dekoracija", Opis = "Luksuzni zlatni detalji", Cijena = 800m, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 3, Naziv = "Rustik dekoracija", Opis = "Prirodni drveni elementi", Cijena = 350m, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null }
        );

        modelBuilder.Entity<Artikal>().HasData(
            new { Id = 1, Naziv = "Teleća pršuta", Tip = SvadbeniSalon.Model.Enums.TipArtikla.Hrana, Cijena = 15m, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 2, Naziv = "Pileći file", Tip = SvadbeniSalon.Model.Enums.TipArtikla.Hrana, Cijena = 18m, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 3, Naziv = "Vino crno", Tip = SvadbeniSalon.Model.Enums.TipArtikla.Pice, Cijena = 25m, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 4, Naziv = "Sok od narandže", Tip = SvadbeniSalon.Model.Enums.TipArtikla.Pice, Cijena = 5m, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null }
        );

        modelBuilder.Entity<Meni>().HasData(
            new { Id = 1, Naziv = "Klasični meni", Opis = "Tradicionalni svadbeni meni", Cijena = 35m, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 2, Naziv = "Premium meni", Opis = "Premium jela i pića", Cijena = 55m, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null }
        );

        modelBuilder.Entity<MeniArtikal>().HasData(
            new { MeniId = 1, ArtikalId = 1 },
            new { MeniId = 1, ArtikalId = 3 },
            new { MeniId = 2, ArtikalId = 2 },
            new { MeniId = 2, ArtikalId = 4 }
        );

        modelBuilder.Entity<Ponuda>().HasData(
            new { Id = 1, Naziv = "Gold paket", Opis = "Luksuzni paket sa premium menijem i zlatnom dekoracijom", Cijena = 15000m, MeniId = 2, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 2, Naziv = "Silver paket", Opis = "Elegantan paket sa klasičnim menijem", Cijena = 10000m, MeniId = 1, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 3, Naziv = "Bronze paket", Opis = "Osnovni paket za manje proslave", Cijena = 7000m, MeniId = 1, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 4, Naziv = "Regular paket", Opis = "Standardni paket sa cvjetnom dekoracijom", Cijena = 8500m, MeniId = 1, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null }
        );

        modelBuilder.Entity<MuzicarPonuda>().HasData(
            new { PonudaId = 1, MuzicarId = 1 },
            new { PonudaId = 1, MuzicarId = 2 },
            new { PonudaId = 2, MuzicarId = 2 },
            new { PonudaId = 3, MuzicarId = 3 },
            new { PonudaId = 4, MuzicarId = 1 }
        );

        modelBuilder.Entity<DekoracijaPonuda>().HasData(
            new { PonudaId = 1, DekoracijaId = 2 },
            new { PonudaId = 2, DekoracijaId = 1 },
            new { PonudaId = 3, DekoracijaId = 3 },
            new { PonudaId = 4, DekoracijaId = 1 }
        );

        modelBuilder.Entity<Role>().HasData(
            new
            {
                Id = 1,
                Name = RoleNames.Admin,
                Description = "Administrator role with full permissions",
                IsActive = true,
                CreatedAt = seedDate
            },
            new
            {
                Id = 2,
                Name = RoleNames.Customer,
                Description = "Default customer role",
                IsActive = true,
                CreatedAt = seedDate
            },
            new
            {
                Id = 3,
                Name = RoleNames.Zaposlenik,
                Description = "Salon employee with access to appointments",
                IsActive = true,
                CreatedAt = seedDate
            }
        );

        // Test korisnici (lozinka: test)
        modelBuilder.Entity<User>().HasData(
            new
            {
                Id = 6,
                FirstName = "Dalila",
                LastName = "Korisnik",
                Email = "mobile@salon.local",
                Username = "mobile",
                PasswordHash = "N5b4vpOtGo4txmR/IoPFNoRg1kY=",
                PasswordSalt = "JopMnUSdt7Cec4gKUV0rag==",
                IsActive = true,
                CreatedAt = seedDate,
                LastLoginAt = (DateTime?)null,
                PhoneNumber = (string?)null,
                ProfileImageBase64 = (string?)null
            },
            new
            {
                Id = 7,
                FirstName = RoleNames.Zaposlenik,
                LastName = "Salon",
                Email = "zaposlenik@salon.local",
                Username = "zaposlenik",
                PasswordHash = "xObgsmDrWZgNlmGJsdxoRS+V/AE=",
                PasswordSalt = "0k57QETJENx5BkJuK4wtZg==",
                IsActive = true,
                CreatedAt = seedDate,
                LastLoginAt = (DateTime?)null,
                PhoneNumber = (string?)null,
                ProfileImageBase64 = (string?)null
            },
            new
            {
                Id = 8,
                FirstName = RoleNames.Admin,
                LastName = "Korisnik",
                Email = "admin@salon.local",
                Username = "admin",
                PasswordHash = "qU2ck45AOJU9W8CVxAO89FyOb8M=",
                PasswordSalt = "65z57pEcbOuw+c9Ma3X10Q==",
                IsActive = true,
                CreatedAt = seedDate,
                LastLoginAt = (DateTime?)null,
                PhoneNumber = (string?)null,
                ProfileImageBase64 = (string?)null
            }
        );

        modelBuilder.Entity<UserRole>().HasData(
            new { Id = 6, UserId = 6, RoleId = 2, DateAssigned = seedDate },
            new { Id = 7, UserId = 7, RoleId = 3, DateAssigned = seedDate },
            new { Id = 8, UserId = 8, RoleId = 1, DateAssigned = seedDate }
        );

        modelBuilder.Entity<Svadba>().HasData(
            new
            {
                Id = 101,
                UserId = 6,
                PonudaId = 2,
                DogovorenaCijena = 10000m,
                DatumSvadbe = new DateTime(2025, 9, 20, 0, 0, 0, DateTimeKind.Utc),
                Vrijeme = new TimeSpan(17, 0, 0),
                BrojGostiju = 120,
                BrojRata = 2,
                Status = TerminStatus.Completed,
                Napomena = (string?)"Svadba održana u septembru, sve po dogovoru.",
                CreatedAt = seedDate,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 102,
                UserId = 6,
                PonudaId = 1,
                DogovorenaCijena = 15000m,
                DatumSvadbe = new DateTime(2026, 10, 10, 0, 0, 0, DateTimeKind.Utc),
                Vrijeme = new TimeSpan(18, 0, 0),
                BrojGostiju = 150,
                BrojRata = 3,
                Status = TerminStatus.Confirmed,
                Napomena = (string?)"Potvrđena rezervacija za oktobar.",
                CreatedAt = seedDate,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 103,
                UserId = 6,
                PonudaId = 4,
                DogovorenaCijena = 8500m,
                DatumSvadbe = new DateTime(2025, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                Vrijeme = new TimeSpan(16, 30, 0),
                BrojGostiju = 80,
                BrojRata = 1,
                Status = TerminStatus.Cancelled,
                Napomena = (string?)"Termin otkazan zbog promjene plana.",
                CreatedAt = seedDate,
                UpdatedAt = (DateTime?)null
            }
        );

        modelBuilder.Entity<Rata>().HasData(
            new
            {
                Id = 101,
                SvadbaId = 101,
                Iznos = 5000m,
                DatumUplate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = seedDate
            },
            new
            {
                Id = 102,
                SvadbaId = 101,
                Iznos = 5000m,
                DatumUplate = new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = seedDate
            },
            new
            {
                Id = 103,
                SvadbaId = 102,
                Iznos = 5000m,
                DatumUplate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = seedDate
            }
        );

        modelBuilder.Entity<Racun>().HasData(
            new
            {
                Id = 101,
                SvadbaId = 101,
                BrojRacuna = "RAC-2025-001",
                DatumIzdavanja = new DateTime(2025, 9, 21, 0, 0, 0, DateTimeKind.Utc),
                UkupanIznos = 10000m,
                UplaceniIznos = 10000m,
                KreiraoUserId = (int?)8,
                CreatedAt = seedDate
            },
            new
            {
                Id = 102,
                SvadbaId = 102,
                BrojRacuna = "RAC-2026-002",
                DatumIzdavanja = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc),
                UkupanIznos = 15000m,
                UplaceniIznos = 5000m,
                KreiraoUserId = (int?)7,
                CreatedAt = seedDate
            }
        );

        modelBuilder.Entity<Recenzija>().HasData(
            new
            {
                Id = 101,
                UserId = 6,
                PonudaId = 2,
                SvadbaId = 101,
                Ocjena = 5,
                Komentar = (string?)"Sve je bilo predivno. Osoblje ljubazno, meni odličan, a dekoracija baš kako smo zamislili.",
                CreatedAt = seedDate
            }
        );

        modelBuilder.Entity<DnevniSastanak>().HasData(
            new
            {
                Id = 101,
                UserId = (int?)6,
                KontaktIme = (string?)null,
                DatumSastanka = new DateTime(2025, 8, 10, 10, 0, 0, DateTimeKind.Utc),
                Status = TerminStatus.Completed,
                Napomena = (string?)"Dogovor oko menija i dekoracije.",
                CreatedAt = seedDate,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 102,
                UserId = (int?)6,
                KontaktIme = (string?)null,
                DatumSastanka = new DateTime(2025, 11, 5, 14, 0, 0, DateTimeKind.Utc),
                Status = TerminStatus.Cancelled,
                Napomena = (string?)"Klijent nije mogao doći.",
                CreatedAt = seedDate,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 103,
                UserId = (int?)6,
                KontaktIme = (string?)null,
                DatumSastanka = new DateTime(2026, 8, 20, 11, 0, 0, DateTimeKind.Utc),
                Status = TerminStatus.Confirmed,
                Napomena = (string?)"Razgled salona i dogovor detalja.",
                CreatedAt = seedDate,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 104,
                UserId = (int?)6,
                KontaktIme = (string?)null,
                DatumSastanka = new DateTime(2026, 9, 1, 9, 30, 0, DateTimeKind.Utc),
                Status = TerminStatus.Pending,
                Napomena = (string?)"Čeka se potvrda termina.",
                CreatedAt = seedDate,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 105,
                UserId = (int?)null,
                KontaktIme = (string?)"Ana Marić",
                DatumSastanka = new DateTime(2026, 7, 25, 15, 0, 0, DateTimeKind.Utc),
                Status = TerminStatus.Pending,
                Napomena = (string?)"Dogovor telefonom, dolazak izvana.",
                CreatedAt = seedDate,
                UpdatedAt = (DateTime?)null
            }
        );
    }
}
