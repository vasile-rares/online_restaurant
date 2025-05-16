using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class Preparat
    {
        public Preparat()
        {
            PreparatAlergeni = new List<PreparatAlergen>();
            MeniuPreparate = new List<MeniuPreparat>();
            Fotografii = new List<FotografiePreparat>();
        }

        [Key]
        public int IdPreparat { get; set; }

        [Required]
        [MaxLength(100)]
        public string Denumire { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Pret { get; set; }

        [Required]
        public int CantitatePortie { get; set; } // în grame

        [Required]
        public int CantitateTotala { get; set; } // în grame

        [Required]
        public int IdCategorie { get; set; }

        [ForeignKey("IdCategorie")]
        public virtual Categorie Categorie { get; set; }

        public virtual ICollection<PreparatAlergen> PreparatAlergeni { get; set; }

        public virtual ICollection<MeniuPreparat> MeniuPreparate { get; set; }

        public virtual ICollection<FotografiePreparat> Fotografii { get; set; }

        [NotMapped]
        public bool InStoc => CantitateTotala >= CantitatePortie;

        [NotMapped]
        public int NumarPortiiDisponibile => CantitatePortie > 0 ? CantitateTotala / CantitatePortie : 0;
    }
}