using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class DishPhoto
    {
        [Key]
        public int IdPhoto { get; set; }

        [Required]
        public int IdDish { get; set; }

        [Required]
        [MaxLength(255)]
        public string Url { get; set; } = string.Empty;

        [ForeignKey("IdDish")]
        public virtual Dish Dish { get; set; }
    }
} 