using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class OrderMenu
    {
        public Guid IdOrder { get; set; }
        
        public int IdMenu { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [ForeignKey("IdOrder")]
        public virtual Order Order { get; set; }

        [ForeignKey("IdMenu")]
        public virtual Menu Menu { get; set; }
    }
} 