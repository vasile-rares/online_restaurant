using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class MeniuPreparat : BaseModel
    {
        private int _idMeniu;
        private int _idPreparat;
        private int _cantitateGrame;
        private Meniu? _meniu;
        private Preparat? _preparat;

        [Key, Column(Order = 0)]
        public int IdMeniu
        {
            get => _idMeniu;
            set => SetField(ref _idMeniu, value);
        }

        [Key, Column(Order = 1)]
        public int IdPreparat
        {
            get => _idPreparat;
            set => SetField(ref _idPreparat, value);
        }

        [Required]
        public int CantitateGrame
        {
            get => _cantitateGrame;
            set => SetField(ref _cantitateGrame, value);
        }

        [ForeignKey("IdMeniu")]
        public virtual Meniu? Meniu
        {
            get => _meniu;
            set => SetField(ref _meniu, value);
        }

        [ForeignKey("IdPreparate")]
        public virtual Preparat? Preparat
        {
            get => _preparat;
            set => SetField(ref _preparat, value);
        }
    }
} 