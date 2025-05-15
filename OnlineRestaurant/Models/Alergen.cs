using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineRestaurant.Models
{
    public class Alergen
    {
        public Alergen()
        {
            PreparatAlergeni = new List<PreparatAlergen>();
        }

        [Key]
        public int IdAlergen { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nume { get; set; } = string.Empty;

        public virtual ICollection<PreparatAlergen> PreparatAlergeni { get; set; }
    }
}