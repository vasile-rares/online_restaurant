using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class ComandaPreparat
    {
        public Guid IdComanda { get; set; }
        
        public int IdPreparat { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Cantitate { get; set; }

        [ForeignKey("IdComanda")]
        public virtual Comanda Comanda { get; set; }

        [ForeignKey("IdPreparat")]
        public virtual Preparat Preparat { get; set; }
    }
} 