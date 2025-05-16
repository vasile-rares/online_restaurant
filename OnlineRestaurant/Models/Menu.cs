using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class Menu
    {
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

        private decimal CalculateTotalPrice()
        {
            decimal total = 0;
            if (MenuDishes != null)
            {
                foreach (var menuDish in MenuDishes)
                {
                    if (menuDish.Dish != null)
                    {
                        total += menuDish.Dish.Price * menuDish.Quantity;
                    }
                }
            }
            return total;
        }
    }
} 