using System.Collections.ObjectModel;

namespace OnlineRestaurant.ViewModels
{
    public enum TipItemMeniu
    {
        Preparat,
        Meniu
    }

    public class ItemMeniuViewModel : BaseVM
    {
        private int _id;
        private TipItemMeniu _tip;
        private string _denumire = string.Empty;
        private decimal _pret;
        private int _cantitatePortie;
        private bool _disponibil;
        private string _detaliiContinut = string.Empty;
        private ObservableCollection<string> _imagini;
        private ObservableCollection<string> _alergeni;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public TipItemMeniu Tip
        {
            get => _tip;
            set => SetProperty(ref _tip, value);
        }

        public string Denumire
        {
            get => _denumire;
            set => SetProperty(ref _denumire, value);
        }

        public decimal Pret
        {
            get => _pret;
            set => SetProperty(ref _pret, value);
        }

        public int CantitatePortie
        {
            get => _cantitatePortie;
            set => SetProperty(ref _cantitatePortie, value);
        }

        public bool Disponibil
        {
            get => _disponibil;
            set => SetProperty(ref _disponibil, value);
        }

        public string DetaliiContinut
        {
            get => _detaliiContinut;
            set => SetProperty(ref _detaliiContinut, value);
        }

        public ObservableCollection<string> Imagini
        {
            get => _imagini;
            set => SetProperty(ref _imagini, value);
        }

        public ObservableCollection<string> Alergeni
        {
            get => _alergeni;
            set => SetProperty(ref _alergeni, value);
        }

        public string StatusDisponibilitate => Disponibil ? "" : "Indisponibil";

        public string AfisorAlergeni => Alergeni?.Count > 0 
            ? $"Alergeni: {string.Join(", ", Alergeni)}" 
            : "Fără alergeni";

        public ItemMeniuViewModel()
        {
            Imagini = new ObservableCollection<string>();
            Alergeni = new ObservableCollection<string>();
        }
    }
} 