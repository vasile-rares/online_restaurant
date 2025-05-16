using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class MenuDish
    {
        public int IdMenu { get; set; }
        
        public int IdDish { get; set; }
        
        public int Quantity { get; set; }

        [ForeignKey("IdMenu")]
        public virtual Menu Menu { get; set; }

        [ForeignKey("IdDish")]
        public virtual Dish Dish { get; set; }
    }
} 