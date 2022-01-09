using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvadbeniSalon.Database
{
    public class LocalContext : DbContext
    {
        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Zanr> Zanrovi { get; set; }
        public DbSet<Muzicar> Muzicari { get; set; }
        public DbSet<MuzicarZanr> MuzicarZanrovi { get; set; }
        public DbSet<Dekoracija> Dekoracije { get; set; }
        public DbSet<Artikal> Artikli { get; set; }
        public DbSet<Meni> Meniji { get; set; }
        public DbSet<MeniArtikal> MeniArtikli { get; set; }
        public DbSet<Ponuda> Ponude { get; set; }
        public DbSet<DekoracijaPonuda> DekoracijaPonuda { get; set; }
        public DbSet<MuzicariPonuda> MuzicarDekoracija { get; set; }

        public LocalContext(DbContextOptions<LocalContext> options)
        : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Korisnik>();

            modelBuilder.Entity<Korisnik>()
            .Property(b => b.CreatedAt)
            .HasDefaultValue(DateTime.UtcNow);
            modelBuilder.Entity<Korisnik>().HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<Muzicar>()
            .Property(b => b.CreatedAt)
            .HasDefaultValue(DateTime.UtcNow);
            modelBuilder.Entity<Muzicar>().HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<Zanr>()
            .Property(b => b.CreatedAt)
            .HasDefaultValue(DateTime.UtcNow);
            modelBuilder.Entity<Zanr>().HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<MuzicarZanr>().HasKey(mz => new { mz.MuzicarId, mz.ZanrId });

            modelBuilder.Entity<Dekoracija>()
            .Property(b => b.CreatedAt)
            .HasDefaultValue(DateTime.UtcNow);
            modelBuilder.Entity<Dekoracija>().HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<Artikal>()
            .Property(b => b.CreatedAt)
            .HasDefaultValue(DateTime.UtcNow);
            modelBuilder.Entity<Artikal>().HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<Meni>()
            .Property(b => b.CreatedAt)
            .HasDefaultValue(DateTime.UtcNow);
            modelBuilder.Entity<Meni>().HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<Ponuda>()
           .Property(b => b.CreatedAt)
           .HasDefaultValue(DateTime.UtcNow);
            modelBuilder.Entity<Ponuda>().HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<MeniArtikal>().HasKey(ma => new { ma.MeniId, ma.ArtikalId });
            modelBuilder.Entity<DekoracijaPonuda>().HasKey(dp => new { dp.PonudaId, dp.DekoracijaId });
            modelBuilder.Entity<MuzicariPonuda>().HasKey(mp => new { mp.PonudaId, mp.MuzicarId });

        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();

            OnBeforeSaving();

            return base.SaveChanges();
        }
        private void OnBeforeSaving()
        {
            var updatedEntries = ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged && x.State != EntityState.Detached && x.State != EntityState.Added);
            foreach (var entry in updatedEntries)
            {

                if (entry.State == EntityState.Deleted)
                {
                    if (entry.Entity is BaseEntity deleteDate)
                    {
                        deleteDate.DeletedAt = DateTime.UtcNow;
                        entry.State = EntityState.Modified;
                    }
                }
                if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity is BaseEntity updatedDateAt)
                    {
                        updatedDateAt.UpdatedAt = DateTime.UtcNow;
                        entry.State = EntityState.Modified;
                    }
                }
                if (entry.Entity is BaseEntity updatedDate)
                {
                    updatedDate.UpdatedAt = DateTime.UtcNow;
                    entry.State = EntityState.Modified;
                }
            }
        }
    }
}
