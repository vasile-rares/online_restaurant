namespace OnlineRestaurant.Models
{
    public class PreparatAlergen
    {
        public int PreparatId { get; set; }
        public int AlergenId { get; set; }
        
        // Navigation properties
        public virtual Preparat Preparat { get; set; }
        public virtual Alergen Alergen { get; set; }
    }
} 