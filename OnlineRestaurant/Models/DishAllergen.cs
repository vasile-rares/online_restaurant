using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class DishAllergen
    {
        public int IdDish { get; set; }
        
        public int IdAllergen { get; set; }

        [ForeignKey("IdDish")]
        public virtual Dish Dish { get; set; }

        [ForeignKey("IdAllergen")]
        public virtual Allergen Allergen { get; set; }
    }
} 