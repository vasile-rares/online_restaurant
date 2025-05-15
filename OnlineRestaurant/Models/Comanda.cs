using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public enum StareComanda
    {
        inregistrata,
        se_pregateste,
        a_plecat_la_client,
        livrata,
        anulata
    }

    public class Comanda : BaseModel
    {
        private Guid _idComanda;
        private int _idUtilizator;
        private DateTime _dataComanda = DateTime.Now;
        private StareComanda _stare = StareComanda.inregistrata;
        private Utilizator? _utilizator;
        private ObservableCollection<ComandaPreparat>? _comandaPreparate;

        public Comanda()
        {
            _idComanda = Guid.NewGuid();
            _comandaPreparate = new ObservableCollection<ComandaPreparat>();
        }

        [Key]
        public Guid IdComanda
        {
            get => _idComanda;
            set => SetField(ref _idComanda, value);
        }

        [Required]
        public int IdUtilizator
        {
            get => _idUtilizator;
            set => SetField(ref _idUtilizator, value);
        }

        [Required]
        public DateTime DataComanda
        {
            get => _dataComanda;
            set => SetField(ref _dataComanda, value);
        }

        [Required]
        [MaxLength(20)]
        public StareComanda Stare
        {
            get => _stare;
            set => SetField(ref _stare, value);
        }

        [ForeignKey("IdUtilizator")]
        public virtual Utilizator? Utilizator
        {
            get => _utilizator;
            set => SetField(ref _utilizator, value);
        }

        public virtual ObservableCollection<ComandaPreparat>? ComandaPreparate
        {
            get => _comandaPreparate;
            set => SetField(ref _comandaPreparate, value);
        }

        [NotMapped]
        public virtual IEnumerable<Preparat> Preparate => ComandaPreparate?.Select(cp => cp.Preparat).Where(p => p != null).Cast<Preparat>() ?? Enumerable.Empty<Preparat>();
    }
} 