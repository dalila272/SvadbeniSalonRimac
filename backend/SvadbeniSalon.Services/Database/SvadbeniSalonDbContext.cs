using Microsoft.EntityFrameworkCore;

namespace SvadbeniSalon.Services.Database
{
    public partial class SvadbeniSalonDbContext : DbContext
    {
        public SvadbeniSalonDbContext(DbContextOptions<SvadbeniSalonDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        public DbSet<Ponuda> Ponude { get; set; }
        public DbSet<Meni> Meniji { get; set; }
        public DbSet<Artikal> Artikli { get; set; }
        public DbSet<Muzicar> Muzicari { get; set; }
        public DbSet<Dekoracija> Dekoracije { get; set; }
        public DbSet<Zanr> Zanrovi { get; set; }
        public DbSet<MeniArtikal> MeniArtikli { get; set; }
        public DbSet<MuzicarPonuda> MuzicarPonuda { get; set; }
        public DbSet<DekoracijaPonuda> DekoracijaPonuda { get; set; }
        public DbSet<MuzicarZanr> MuzicarZanrovi { get; set; }
        public DbSet<Svadba> Svadbe { get; set; }
        public DbSet<DnevniSastanak> DnevniSastanci { get; set; }
        public DbSet<Recenzija> Recenzije { get; set; }
        public DbSet<Rata> Rate { get; set; }
        public DbSet<Racun> Racuni { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            CreateAuthConfiguration(modelBuilder);
            CreateSalonConfiguration(modelBuilder);
            CreateSalonSeed(modelBuilder);
        }
    }
}
