namespace OnlineRestaurant.Models
{
    public class MeniuPreparat
    {
        public int MeniuId { get; set; }
        public int PreparatId { get; set; }
        public decimal CantitateInMeniu { get; set; }
        
        // Navigation properties
        public virtual Meniu Meniu { get; set; }
        public virtual Preparat Preparat { get; set; }
    }
} 