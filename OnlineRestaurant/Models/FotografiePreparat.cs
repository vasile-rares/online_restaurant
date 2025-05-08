namespace OnlineRestaurant.Models
{
    public class FotografiePreparat
    {
        public int Id { get; set; }
        public int PreparatId { get; set; }
        public string Url { get; set; }

        // Navigation property
        public virtual Preparat Preparat { get; set; }
    }
} 