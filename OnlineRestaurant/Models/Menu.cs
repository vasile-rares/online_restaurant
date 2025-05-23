using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineRestaurant.Services;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace OnlineRestaurant.Models
{
    public class Menu
    {
        private static decimal? _cachedDiscountPercentage;

        public Menu()
        {
            MenuDishes = new List<MenuDish>();
        }

        [Key]
        public int IdMenu { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int IdCategory { get; set; }

        [ForeignKey("IdCategory")]
        public virtual Category Category { get; set; }

        public virtual ICollection<MenuDish> MenuDishes { get; set; }

        [NotMapped]
        public decimal TotalPrice => CalculateTotalPrice();

        [NotMapped]
        public decimal OriginalPrice => CalculateOriginalPrice();

        private decimal CalculateOriginalPrice()
        {
            decimal total = 0;
            if (MenuDishes != null)
            {
                foreach (var menuDish in MenuDishes)
                {
                    if (menuDish.Dish != null)
                    {
                        total += menuDish.Dish.Price;
                    }
                }
            }
            return total;
        }

        private decimal CalculateTotalPrice()
        {
            decimal originalPrice = CalculateOriginalPrice();
            decimal discountPercentage = GetDiscountPercentage();

            // Aplicăm discountul
            return Math.Round(originalPrice * (1 - discountPercentage / 100), 2);
        }

        // Metoda de ajutor pentru a obține procentul de discount
        private static decimal GetDiscountPercentage()
        {
            // Folosim valoarea în cache dacă există
            if (_cachedDiscountPercentage.HasValue)
                return _cachedDiscountPercentage.Value;

            // Încercăm să obținem din servicii
            try
            {
                // Accesăm AppSettingsService prin ServiceProvider static
                if (App.ServiceProvider != null)
                {
                    var appSettingsService = App.ServiceProvider.GetService<AppSettingsService>();
                    if (appSettingsService != null)
                    {
                        _cachedDiscountPercentage = appSettingsService.GetMenuDiscountPercentage();
                        return _cachedDiscountPercentage.Value;
                    }
                }
            }
            catch
            {
                // Ignorăm orice eroare
            }

            // Valoare implicită dacă nu putem obține din configurație
            _cachedDiscountPercentage = 15.0m;
            return _cachedDiscountPercentage.Value;
        }

        // Metodă statică pentru a actualiza discount-ul din exterior
        public static void UpdateDiscountPercentage(decimal newPercentage)
        {
            _cachedDiscountPercentage = newPercentage;
        }
    }
}