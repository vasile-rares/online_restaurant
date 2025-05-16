using System.Collections.ObjectModel;
using System.Windows.Input;
using OnlineRestaurant.Commands;

namespace OnlineRestaurant.ViewModels
{
    public enum ItemMenuType
    {
        Dish,
        Menu
    }

    public class ItemMenuViewModel : BaseVM
    {
        private int _id;
        private ItemMenuType _type;
        private string _name = string.Empty;
        private decimal _price;
        private int _portionSize;
        private bool _available;
        private string _contentDetails = string.Empty;
        private ObservableCollection<string> _images;
        private ObservableCollection<string> _allergens;
        private bool _canAddToCart;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public ItemMenuType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public int PortionSize
        {
            get => _portionSize;
            set => SetProperty(ref _portionSize, value);
        }

        public bool Available
        {
            get => _available;
            set
            {
                if (_available != value)
                {
                    _available = value;
                    OnPropertyChanged(nameof(Available));
                    
                    OnPropertyChanged(nameof(AddToCartMessage));
                    
                    OnPropertyChanged(nameof(AvailabilityStatus));
                }
            }
        }

        public string ContentDetails
        {
            get => _contentDetails;
            set => SetProperty(ref _contentDetails, value);
        }

        public ObservableCollection<string> Images
        {
            get => _images;
            set => SetProperty(ref _images, value);
        }

        public ObservableCollection<string> Allergens
        {
            get => _allergens;
            set => SetProperty(ref _allergens, value);
        }

        public bool CanAddToCart
        {
            get => _canAddToCart;
            set
            {
                if (_canAddToCart != value)
                {
                    _canAddToCart = value;
                    OnPropertyChanged(nameof(CanAddToCart));
                    
                    OnPropertyChanged(nameof(AddToCartMessage));
                }
            }
        }

        public string AvailabilityStatus => Available ? "" : "Indisponibil";

        public string AllergenDisplay => Allergens?.Count > 0 
            ? $"Alergeni: {string.Join(", ", Allergens)}" 
            : "Fără alergeni";

        // Mesaj pentru utilizator privind adăugarea în coș
        public string AddToCartMessage
        {
            get
            {
                if (!Available) 
                    return "Indisponibil";
                    
                return (!CanAddToCart) ? "Conectați-vă pentru a adăuga în coș" : "";
            }
        }

        public ICommand AddToCartCommand { get; set; }

        public ItemMenuViewModel()
        {
            Images = new ObservableCollection<string>();
            Allergens = new ObservableCollection<string>();
        }
    }
} 