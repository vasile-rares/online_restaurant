using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class FotografiePreparat
    {
        [Key]
        public int IdFotografie { get; set; }

        [Required]
        public int IdPreparat { get; set; }

        [Required]
        [MaxLength(255)]
        public string Url { get; set; } = string.Empty;

        [ForeignKey("IdPreparat")]
        public virtual Preparat Preparat { get; set; }
    }
} 