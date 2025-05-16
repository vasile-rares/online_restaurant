using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class Dish
    {
        public Dish()
        {
            DishAllergens = new List<DishAllergen>();
            MenuDishes = new List<MenuDish>();
            Photos = new List<DishPhoto>();
        }

        [Key]
        public int IdDish { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [Required]
        public int PortionSize { get; set; } // in grams

        [Required]
        public int TotalQuantity { get; set; } // in grams

        [Required]
        public int IdCategory { get; set; }

        [ForeignKey("IdCategory")]
        public virtual Category Category { get; set; }

        public virtual ICollection<DishAllergen> DishAllergens { get; set; }

        public virtual ICollection<MenuDish> MenuDishes { get; set; }

        public virtual ICollection<DishPhoto> Photos { get; set; }

        [NotMapped]
        public bool InStock => TotalQuantity >= PortionSize;

        [NotMapped]
        public int AvailablePortions => PortionSize > 0 ? TotalQuantity / PortionSize : 0;
    }
}