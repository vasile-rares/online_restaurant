using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class FotografiePreparat : BaseModel
    {
        private int _idFoto;
        private int _idPreparat;
        private string _url = string.Empty;
        private Preparat? _preparat;

        [Key]
        public int IdFoto
        {
            get => _idFoto;
            set => SetField(ref _idFoto, value);
        }

        [Required]
        public int IdPreparat
        {
            get => _idPreparat;
            set => SetField(ref _idPreparat, value);
        }

        [Required]
        [MaxLength(255)]
        public string Url
        {
            get => _url;
            set => SetField(ref _url, value);
        }

        [ForeignKey("IdPreparat")]
        public virtual Preparat? Preparat
        {
            get => _preparat;
            set => SetField(ref _preparat, value);
        }
    }
} 