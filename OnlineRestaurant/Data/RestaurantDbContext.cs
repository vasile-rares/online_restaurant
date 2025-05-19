using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Models;
using System;

namespace OnlineRestaurant.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Allergen> Allergens { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DishPhoto> DishPhotos { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<DishAllergen> DishAllergens { get; set; }
        public DbSet<MenuDish> MenuDishes { get; set; }
        public DbSet<OrderDish> OrderDishes { get; set; }
        public DbSet<OrderMenu> OrderMenus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurare pentru chei primare compuse
            modelBuilder.Entity<DishAllergen>()
                .HasKey(pa => new { pa.IdDish, pa.IdAllergen });

            modelBuilder.Entity<MenuDish>()
                .HasKey(mp => new { mp.IdMenu, mp.IdDish });

            modelBuilder.Entity<OrderDish>()
                .HasKey(cp => new { cp.IdOrder, cp.IdDish });
                
            modelBuilder.Entity<OrderMenu>()
                .HasKey(om => new { om.IdOrder, om.IdMenu });

            // Configurare relații many-to-many
            modelBuilder.Entity<DishAllergen>()
                .HasOne(pa => pa.Dish)
                .WithMany(p => p.DishAllergens)
                .HasForeignKey(pa => pa.IdDish)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DishAllergen>()
                .HasOne(pa => pa.Allergen)
                .WithMany(a => a.DishAllergens)
                .HasForeignKey(pa => pa.IdAllergen)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MenuDish>()
                .HasOne(mp => mp.Menu)
                .WithMany(m => m.MenuDishes)
                .HasForeignKey(mp => mp.IdMenu)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MenuDish>()
                .HasOne(mp => mp.Dish)
                .WithMany(p => p.MenuDishes)
                .HasForeignKey(mp => mp.IdDish)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDish>()
                .HasOne(cp => cp.Order)
                .WithMany(c => c.OrderDishes)
                .HasForeignKey(cp => cp.IdOrder)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDish>()
                .HasOne(cp => cp.Dish)
                .WithMany()
                .HasForeignKey(cp => cp.IdDish)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<OrderMenu>()
                .HasOne(om => om.Order)
                .WithMany(o => o.OrderMenus)
                .HasForeignKey(om => om.IdOrder)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderMenu>()
                .HasOne(om => om.Menu)
                .WithMany()
                .HasForeignKey(om => om.IdMenu)
                .OnDelete(DeleteBehavior.Restrict);

            // Tabele
            modelBuilder.Entity<Dish>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Dishes)
                .HasForeignKey(p => p.IdCategory)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Menus)
                .HasForeignKey(m => m.IdCategory)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DishPhoto>()
                .HasOne(f => f.Dish)
                .WithMany(p => p.Photos)
                .HasForeignKey(f => f.IdDish)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(c => c.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(c => c.IdUser)
                .OnDelete(DeleteBehavior.Restrict);

            // Convertirea enum-ului OrderStatus la string cu valorile corecte
            modelBuilder.Entity<Order>()
                .Property(c => c.Status)
                .HasConversion(
                    v => v.ToString().Replace('_', ' '),  // Replace underscores with spaces when saving to DB
                    v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v.Replace(' ', '_'))  // Replace spaces with underscores when reading from DB
                );

            // Configurarea preciziei pentru valorile monetare
            modelBuilder.Entity<Dish>()
                .Property(p => p.Price)
                .HasColumnType("decimal(10,2)");

            // Email unic pentru utilizatori
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Allergen>().ToTable("Allergens");
            modelBuilder.Entity<Dish>().ToTable("Dishes");
            modelBuilder.Entity<DishPhoto>().ToTable("DishPhotos");
            modelBuilder.Entity<DishAllergen>().ToTable("DishAllergen");
            modelBuilder.Entity<Menu>().ToTable("Menus");
            modelBuilder.Entity<MenuDish>().ToTable("MenuDish");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderDish>().ToTable("OrderDish");
            modelBuilder.Entity<OrderMenu>().ToTable("OrderMenu");
        }
    }
} 