using Microsoft.EntityFrameworkCore;

namespace SvadbeniSalon.Services.Database;

public partial class SvadbeniSalonDbContext : DbContext
{
    private void CreateSalonConfiguration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MeniArtikal>()
            .HasKey(ma => new { ma.MeniId, ma.ArtikalId });

        modelBuilder.Entity<MuzicarPonuda>()
            .HasKey(mp => new { mp.PonudaId, mp.MuzicarId });

        modelBuilder.Entity<DekoracijaPonuda>()
            .HasKey(dp => new { dp.PonudaId, dp.DekoracijaId });

        modelBuilder.Entity<MuzicarZanr>()
            .HasKey(mz => new { mz.MuzicarId, mz.ZanrId });

        modelBuilder.Entity<UserZanr>()
            .HasKey(uz => new { uz.UserId, uz.ZanrId });

        modelBuilder.Entity<UserZanr>()
            .HasOne(uz => uz.User)
            .WithMany(u => u.UserZanrovi)
            .HasForeignKey(uz => uz.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserZanr>()
            .HasOne(uz => uz.Zanr)
            .WithMany(z => z.UserZanrovi)
            .HasForeignKey(uz => uz.ZanrId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ponuda>()
            .HasOne(p => p.Meni)
            .WithMany(m => m.Ponude)
            .HasForeignKey(p => p.MeniId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Svadba>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Svadba>()
            .HasOne(s => s.Ponuda)
            .WithMany()
            .HasForeignKey(s => s.PonudaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DnevniSastanak>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Recenzija>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Recenzija>()
            .HasOne(r => r.Ponuda)
            .WithMany()
            .HasForeignKey(r => r.PonudaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Recenzija>()
            .HasOne(r => r.Svadba)
            .WithMany()
            .HasForeignKey(r => r.SvadbaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // One rating per user per package (pre- or post-wedding).
        modelBuilder.Entity<Recenzija>()
            .HasIndex(r => new { r.UserId, r.PonudaId })
            .IsUnique();

        // At most one review linked to a given wedding (when SvadbaId is set).
        modelBuilder.Entity<Recenzija>()
            .HasIndex(r => r.SvadbaId)
            .IsUnique()
            .HasFilter("[SvadbaId] IS NOT NULL");

        modelBuilder.Entity<Rata>()
            .HasOne(r => r.Svadba)
            .WithMany(s => s.Rate)
            .HasForeignKey(r => r.SvadbaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Racun>()
            .HasOne(r => r.Svadba)
            .WithMany(s => s.Racuni)
            .HasForeignKey(r => r.SvadbaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Racun>()
            .HasIndex(r => r.BrojRacuna)
            .IsUnique();

        modelBuilder.Entity<Notifikacija>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notifikacija>()
            .HasIndex(n => new { n.UserId, n.IsRead });
    }
}
