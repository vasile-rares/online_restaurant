using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineRestaurant.Models
{
    public class Allergen
    {
        public Allergen()
        {
            DishAllergens = new List<DishAllergen>();
        }

        [Key]
        public int IdAllergen { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<DishAllergen> DishAllergens { get; set; }
    }
}