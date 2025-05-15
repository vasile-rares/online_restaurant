using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class PreparatAlergen
    {
        public int IdPreparat { get; set; }
        
        public int IdAlergen { get; set; }

        [ForeignKey("IdPreparat")]
        public virtual Preparat Preparat { get; set; }

        [ForeignKey("IdAlergen")]
        public virtual Alergen Alergen { get; set; }
    }
} 