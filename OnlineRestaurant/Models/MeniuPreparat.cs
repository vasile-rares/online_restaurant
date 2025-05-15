using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class MeniuPreparat
    {
        public int IdMeniu { get; set; }
        
        public int IdPreparat { get; set; }
        
        public int Cantitate { get; set; }

        [ForeignKey("IdMeniu")]
        public virtual Meniu Meniu { get; set; }

        [ForeignKey("IdPreparat")]
        public virtual Preparat Preparat { get; set; }
    }
} 