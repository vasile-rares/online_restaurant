using System.Collections.Generic;

namespace OnlineRestaurant.Models
{
    public class Alergen
    {
        public int Id { get; set; }
        public string Nume { get; set; }

        // Navigation properties
        public virtual ICollection<Preparat> Preparate { get; set; }
    }
} 