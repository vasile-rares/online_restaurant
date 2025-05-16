using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineRestaurant.Models
{
    public class Category
    {
        public Category()
        {
            Dishes = new List<Dish>();
            Menus = new List<Menu>();
        }

        [Key]
        public int IdCategory { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Dish> Dishes { get; set; }
        
        public virtual ICollection<Menu> Menus { get; set; }
    }
} 