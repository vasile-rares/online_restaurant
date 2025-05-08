using System.Collections.Generic;

namespace OnlineRestaurant.Models
{
    public class Preparat
    {
        public int Id { get; set; }
        public string Denumire { get; set; }
        public decimal Pret { get; set; }
        public decimal CantitatePortie { get; set; }
        public decimal CantitateTotala { get; set; }
        public string UnitateMasura { get; set; }
        public int CategorieId { get; set; }

        // Navigation properties
        public virtual Categorie Categorie { get; set; }
        public virtual ICollection<PreparatAlergen> PreparateAlergeni { get; set; }
        public virtual ICollection<FotografiePreparat> Fotografii { get; set; }
        public virtual ICollection<MeniuPreparat> MeniuPreparate { get; set; }
        public virtual ICollection<ComandaPreparat> ComandaPreparate { get; set; }
    }
} 