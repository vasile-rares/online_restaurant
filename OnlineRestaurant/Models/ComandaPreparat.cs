namespace OnlineRestaurant.Models
{
    public class ComandaPreparat
    {
        public int ComandaId { get; set; }
        public int PreparatId { get; set; }
        public int Cantitate { get; set; }
        
        // Navigation properties
        public virtual Comanda Comanda { get; set; }
        public virtual Preparat Preparat { get; set; }
    }
} 