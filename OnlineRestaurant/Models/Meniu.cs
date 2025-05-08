using System.Collections.Generic;

namespace OnlineRestaurant.Models
{
    public class Meniu
    {
        public int Id { get; set; }
        public string Denumire { get; set; }
        public int CategorieId { get; set; }

        // Navigation properties
        public virtual Categorie Categorie { get; set; }
        public virtual ICollection<Preparat> Preparate { get; set; }
    }
} 