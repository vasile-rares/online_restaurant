using System.Collections.ObjectModel;

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
            set => SetProperty(ref _available, value);
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

        public string AvailabilityStatus => Available ? "" : "Indisponibil";

        public string AllergenDisplay => Allergens?.Count > 0 
            ? $"Alergeni: {string.Join(", ", Allergens)}" 
            : "Fără alergeni";

        public ItemMenuViewModel()
        {
            Images = new ObservableCollection<string>();
            Allergens = new ObservableCollection<string>();
        }
    }
} 