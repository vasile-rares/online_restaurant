using System.Collections.Generic;

namespace OnlineRestaurant.Models
{
    public class Categorie
    {
        public int Id { get; set; }
        public string Nume { get; set; }

        // Navigation properties
        public virtual ICollection<Preparat> Preparate { get; set; }
        public virtual ICollection<Meniu> Meniuri { get; set; }
    }
} 