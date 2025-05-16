using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class OrderDish
    {
        public Guid IdOrder { get; set; }
        
        public int IdDish { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [ForeignKey("IdOrder")]
        public virtual Order Order { get; set; }

        [ForeignKey("IdDish")]
        public virtual Dish Dish { get; set; }
    }
} 