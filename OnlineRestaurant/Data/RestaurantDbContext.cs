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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurare relație many-to-many între Preparat și Alergen
            modelBuilder.Entity<Preparat>()
                .HasMany(p => p.Alergeni)
                .WithMany(a => a.Preparate)
                .UsingEntity(j => j.ToTable("PreparateAlergeni"));

            // Configurare relație many-to-many între Meniu și Preparat
            modelBuilder.Entity<Meniu>()
                .HasMany(m => m.Preparate)
                .WithMany(p => p.Meniuri)
                .UsingEntity(j => j.ToTable("MeniuPreparate"));

            // Configurare relație many-to-many între Comanda și Preparat
            modelBuilder.Entity<Comanda>()
                .HasMany(c => c.Preparate)
                .WithMany(p => p.Comenzi)
                .UsingEntity(j => j.ToTable("ComandaPreparate"));

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