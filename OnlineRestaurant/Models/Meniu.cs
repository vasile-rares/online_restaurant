using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class Meniu
    {
        public Meniu()
        {
            MeniuPreparate = new List<MeniuPreparat>();
        }

        [Key]
        public int IdMeniu { get; set; }

        [Required]
        [MaxLength(100)]
        public string Denumire { get; set; } = string.Empty;

        [Required]
        public int IdCategorie { get; set; }

        [ForeignKey("IdCategorie")]
        public virtual Categorie Categorie { get; set; }

        public virtual ICollection<MeniuPreparat> MeniuPreparate { get; set; }

        [NotMapped]
        public decimal PretTotal => CalculeazaPretTotal();

        private decimal CalculeazaPretTotal()
        {
            decimal total = 0;
            if (MeniuPreparate != null)
            {
                foreach (var mp in MeniuPreparate)
                {
                    if (mp.Preparat != null)
                    {
                        total += mp.Preparat.Pret * mp.Cantitate;
                    }
                }
            }
            return total;
        }
    }
} 