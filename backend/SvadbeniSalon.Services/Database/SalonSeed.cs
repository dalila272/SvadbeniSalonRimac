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
            new { Id = 3, Naziv = "Balet", IsActive = false, CreatedAt = seedDate },
            new { Id = 4, Naziv = "Jazz", IsActive = true, CreatedAt = seedDate },
            new { Id = 5, Naziv = "Klasična", IsActive = true, CreatedAt = seedDate },
            new { Id = 6, Naziv = "Narodna", IsActive = true, CreatedAt = seedDate },
            new { Id = 7, Naziv = "Starogradska", IsActive = true, CreatedAt = seedDate },
            new { Id = 8, Naziv = "Akustična", IsActive = true, CreatedAt = seedDate },
            new { Id = 9, Naziv = "Evergreen", IsActive = true, CreatedAt = seedDate },
            new { Id = 10, Naziv = "Elektronska", IsActive = false, CreatedAt = seedDate }
        );

        // Bendovi / ansambli usklađeni sa žanrovima (za recommender matching).
        modelBuilder.Entity<Muzicar>().HasData(
            new { Id = 1, Naziv = "Hit Parade Bend", Opis = "Live pop i evergreen hitovi za plesni dio večeri", IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 2, Naziv = "Electric Avenue", Opis = "Energičan rock bend za zabavniju svadbenu atmosferu", IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 3, Naziv = "Kvartet Armonija", Opis = "Gudački kvartet za ceremoniju i elegantni doček", IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 4, Naziv = "Sevdah Ansambl", Opis = "Narodna i starogradska muzika uživo", IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 5, Naziv = "Jazz Club Trio", Opis = "Lounge i swing jazz tokom večere", IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 6, Naziv = "Duo Tišina", Opis = "Akustični duo za intimne trenutke i evergreen klasike", IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null }
        );

        modelBuilder.Entity<MuzicarZanr>().HasData(
            new { MuzicarId = 1, ZanrId = 1 }, // Pop
            new { MuzicarId = 1, ZanrId = 9 }, // Evergreen
            new { MuzicarId = 2, ZanrId = 2 }, // Rock
            new { MuzicarId = 3, ZanrId = 5 }, // Klasična
            new { MuzicarId = 4, ZanrId = 6 }, // Narodna
            new { MuzicarId = 4, ZanrId = 7 }, // Starogradska
            new { MuzicarId = 5, ZanrId = 4 }, // Jazz
            new { MuzicarId = 6, ZanrId = 8 }, // Akustična
            new { MuzicarId = 6, ZanrId = 9 }  // Evergreen
        );

        modelBuilder.Entity<Dekoracija>().HasData(
            new { Id = 1, Naziv = "Cvjetna dekoracija", Opis = "Bijele ruže, hortenzije i svijeće za romantičan ambijent", Cijena = 500m, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 2, Naziv = "Zlatna dekoracija", Opis = "Luksuzni zlatni detalji, kristalne vase i LED akcenti", Cijena = 800m, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 3, Naziv = "Rustik dekoracija", Opis = "Drvo, laneno platno i suho cvijeće za tradicionalni ugođaj", Cijena = 350m, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null }
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

        // Paketi imaju jasne muzičke profile radi interesa → matching.
        modelBuilder.Entity<Ponuda>().HasData(
            new { Id = 1, Naziv = "Gold paket", Opis = "Luksuzna večer: premium meni, zlatna dekoracija, pop/evergreen bend i jazz trio", Cijena = 15000m, MeniId = 2, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 2, Naziv = "Silver paket", Opis = "Elegantna klasika: klasični meni, cvjetna dekoracija, gudači i akustični duo", Cijena = 10000m, MeniId = 1, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 3, Naziv = "Bronze paket", Opis = "Tradicionalna svadba: narodna i starogradska muzika, rustik dekoracija", Cijena = 7000m, MeniId = 1, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 4, Naziv = "Regular paket", Opis = "Zabavna večer: pop i rock bend, cvjetna dekoracija, klasični meni", Cijena = 8500m, MeniId = 1, IsActive = true, CreatedAt = seedDate, UpdatedAt = (DateTime?)null }
        );

        modelBuilder.Entity<MuzicarPonuda>().HasData(
            new { PonudaId = 1, MuzicarId = 1 }, // Gold ← Pop/Evergreen
            new { PonudaId = 1, MuzicarId = 5 }, // Gold ← Jazz
            new { PonudaId = 2, MuzicarId = 3 }, // Silver ← Klasična
            new { PonudaId = 2, MuzicarId = 6 }, // Silver ← Akustična/Evergreen
            new { PonudaId = 3, MuzicarId = 4 }, // Bronze ← Narodna/Starogradska
            new { PonudaId = 4, MuzicarId = 1 }, // Regular ← Pop/Evergreen
            new { PonudaId = 4, MuzicarId = 2 }  // Regular ← Rock
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
            },
            // Dodatni klijenti za collaborative filtering (lozinka: test)
            new
            {
                Id = 9,
                FirstName = "Ana",
                LastName = "Popović",
                Email = "ana.popovic@salon.local",
                Username = "ana.popovic",
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
                Id = 10,
                FirstName = "Marko",
                LastName = "Softić",
                Email = "marko.softic@salon.local",
                Username = "marko.softic",
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
                Id = 11,
                FirstName = "Ena",
                LastName = "Kovač",
                Email = "ena.kovac@salon.local",
                Username = "ena.kovac",
                PasswordHash = "N5b4vpOtGo4txmR/IoPFNoRg1kY=",
                PasswordSalt = "JopMnUSdt7Cec4gKUV0rag==",
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
            new { Id = 8, UserId = 8, RoleId = 1, DateAssigned = seedDate },
            new { Id = 9, UserId = 9, RoleId = 2, DateAssigned = seedDate },
            new { Id = 10, UserId = 10, RoleId = 2, DateAssigned = seedDate },
            new { Id = 11, UserId = 11, RoleId = 2, DateAssigned = seedDate }
        );

        // Interesi: mobile = pop/evergreen; ostali pokrivaju tematske pakete.
        modelBuilder.Entity<UserZanr>().HasData(
            new { UserId = 6, ZanrId = 1 },
            new { UserId = 6, ZanrId = 9 },
            new { UserId = 9, ZanrId = 6 },
            new { UserId = 9, ZanrId = 7 },
            new { UserId = 10, ZanrId = 5 },
            new { UserId = 10, ZanrId = 4 },
            new { UserId = 11, ZanrId = 1 },
            new { UserId = 11, ZanrId = 2 }
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
            },
            new
            {
                Id = 104,
                UserId = 9,
                PonudaId = 3,
                DogovorenaCijena = 7000m,
                DatumSvadbe = new DateTime(2025, 7, 12, 0, 0, 0, DateTimeKind.Utc),
                Vrijeme = new TimeSpan(17, 0, 0),
                BrojGostiju = 90,
                BrojRata = 1,
                Status = TerminStatus.Completed,
                Napomena = (string?)"Tradicionalna svadba — zadovoljni narodnim ansamblom.",
                CreatedAt = seedDate,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 105,
                UserId = 10,
                PonudaId = 2,
                DogovorenaCijena = 10000m,
                DatumSvadbe = new DateTime(2025, 8, 23, 0, 0, 0, DateTimeKind.Utc),
                Vrijeme = new TimeSpan(18, 0, 0),
                BrojGostiju = 110,
                BrojRata = 2,
                Status = TerminStatus.Completed,
                Napomena = (string?)"Elegantna večer uz gudače.",
                CreatedAt = seedDate,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 106,
                UserId = 11,
                PonudaId = 4,
                DogovorenaCijena = 8500m,
                DatumSvadbe = new DateTime(2025, 10, 5, 0, 0, 0, DateTimeKind.Utc),
                Vrijeme = new TimeSpan(19, 0, 0),
                BrojGostiju = 100,
                BrojRata = 1,
                Status = TerminStatus.Completed,
                Napomena = (string?)"Zabavna večer uz pop/rock bend.",
                CreatedAt = seedDate,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 107,
                UserId = 11,
                PonudaId = 1,
                DogovorenaCijena = 15000m,
                DatumSvadbe = new DateTime(2024, 11, 16, 0, 0, 0, DateTimeKind.Utc),
                Vrijeme = new TimeSpan(18, 30, 0),
                BrojGostiju = 160,
                BrojRata = 2,
                Status = TerminStatus.Completed,
                Napomena = (string?)"Prethodna luksuzna svadba u porodici.",
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
            },
            new
            {
                Id = 102,
                UserId = 9,
                PonudaId = 3,
                SvadbaId = 104,
                Ocjena = 5,
                Komentar = (string?)"Sevdah Ansambl je napravio atmosferu — baš onako kako smo htjeli za tradicionalnu svadbu.",
                CreatedAt = seedDate
            },
            new
            {
                Id = 103,
                UserId = 10,
                PonudaId = 2,
                SvadbaId = 105,
                Ocjena = 5,
                Komentar = (string?)"Kvartet Armonija i Duo Tišina — elegantno od dočeka do večere.",
                CreatedAt = seedDate
            },
            new
            {
                Id = 104,
                UserId = 11,
                PonudaId = 4,
                SvadbaId = 106,
                Ocjena = 5,
                Komentar = (string?)"Gosti su plesali cijelu noć uz pop i rock bend.",
                CreatedAt = seedDate
            },
            new
            {
                Id = 105,
                UserId = 11,
                PonudaId = 1,
                SvadbaId = 107,
                Ocjena = 4,
                Komentar = (string?)"Gold paket je luksuzan; jazz trio tokom večere je bio hit.",
                CreatedAt = seedDate
            },
            new
            {
                Id = 106,
                UserId = 9,
                PonudaId = 2,
                SvadbaId = (int?)null,
                Ocjena = 3,
                Komentar = (string?)"Elegantno, ali nama više odgovara narodni ugođaj.",
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
