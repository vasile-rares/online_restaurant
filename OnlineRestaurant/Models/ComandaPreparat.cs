using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public class ComandaPreparat : BaseModel
    {
        private Guid _idComanda;
        private int _idPreparat;
        private int _cantitate;
        private Comanda? _comanda;
        private Preparat? _preparat;

        [Key, Column(Order = 0)]
        public Guid IdComanda
        {
            get => _idComanda;
            set => SetField(ref _idComanda, value);
        }

        [Key, Column(Order = 1)]
        public int IdPreparat
        {
            get => _idPreparat;
            set => SetField(ref _idPreparat, value);
        }

        [Required]
        public int Cantitate
        {
            get => _cantitate;
            set => SetField(ref _cantitate, value);
        }

        [ForeignKey("IdComanda")]
        public virtual Comanda? Comanda
        {
            get => _comanda;
            set => SetField(ref _comanda, value);
        }

        [ForeignKey("IdPreparat")]
        public virtual Preparat? Preparat
        {
            get => _preparat;
            set => SetField(ref _preparat, value);
        }
    }
} 