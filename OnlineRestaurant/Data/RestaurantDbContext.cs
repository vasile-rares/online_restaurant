using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Models;

namespace OnlineRestaurant.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categorie> Categorii { get; set; }
        public DbSet<Alergen> Alergeni { get; set; }
        public DbSet<Preparat> Preparate { get; set; }
        public DbSet<FotografiePreparat> FotografiiPreparat { get; set; }
        public DbSet<Meniu> Meniuri { get; set; }
        public DbSet<Utilizator> Utilizatori { get; set; }
        public DbSet<Comanda> Comenzi { get; set; }
        public DbSet<PreparatAlergen> PreparateAlergeni { get; set; }
        public DbSet<MeniuPreparat> MeniuPreparate { get; set; }
        public DbSet<ComandaPreparat> ComandaPreparate { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurare relație many-to-many între Preparat și Alergen cu entitate de legătură
            modelBuilder.Entity<PreparatAlergen>()
                .HasKey(pa => new { pa.PreparatId, pa.AlergenId });

            modelBuilder.Entity<PreparatAlergen>()
                .HasOne(pa => pa.Preparat)
                .WithMany()
                .HasForeignKey(pa => pa.PreparatId);

            modelBuilder.Entity<PreparatAlergen>()
                .HasOne(pa => pa.Alergen)
                .WithMany()
                .HasForeignKey(pa => pa.AlergenId);

            // Configurare relație many-to-many între Meniu și Preparat cu entitate de legătură
            modelBuilder.Entity<MeniuPreparat>()
                .HasKey(mp => new { mp.MeniuId, mp.PreparatId });

            modelBuilder.Entity<MeniuPreparat>()
                .HasOne(mp => mp.Meniu)
                .WithMany()
                .HasForeignKey(mp => mp.MeniuId);

            modelBuilder.Entity<MeniuPreparat>()
                .HasOne(mp => mp.Preparat)
                .WithMany()
                .HasForeignKey(mp => mp.PreparatId);

            // Configurare relație many-to-many între Comanda și Preparat cu entitate de legătură
            modelBuilder.Entity<ComandaPreparat>()
                .HasKey(cp => new { cp.ComandaId, cp.PreparatId });

            modelBuilder.Entity<ComandaPreparat>()
                .HasOne(cp => cp.Comanda)
                .WithMany()
                .HasForeignKey(cp => cp.ComandaId);

            modelBuilder.Entity<ComandaPreparat>()
                .HasOne(cp => cp.Preparat)
                .WithMany()
                .HasForeignKey(cp => cp.PreparatId);

            // Configurare constrângeri pentru Comanda
            modelBuilder.Entity<Comanda>()
                .Property(c => c.Stare)
                .HasMaxLength(50);

            // Configurare constrângeri pentru Utilizator
            modelBuilder.Entity<Utilizator>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
} 