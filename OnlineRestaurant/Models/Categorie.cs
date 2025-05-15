using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineRestaurant.Models
{
    public class Categorie
    {
        public Categorie()
        {
            Preparate = new List<Preparat>();
            Meniuri = new List<Meniu>();
        }

        [Key]
        public int IdCategorie { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nume { get; set; } = string.Empty;

        public virtual ICollection<Preparat> Preparate { get; set; }
        
        public virtual ICollection<Meniu> Meniuri { get; set; }
    }
} 