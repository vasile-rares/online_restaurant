using System.Collections.ObjectModel;

namespace OnlineRestaurant.ViewModels
{
    public class CategoryViewModel : BaseVM
    {
        private int _idCategory;
        private string _name = string.Empty;
        private ObservableCollection<MenuRestaurantViewModel.ItemMenuViewModel> _items;

        public int IdCategory
        {
            get => _idCategory;
            set => SetProperty(ref _idCategory, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<MenuRestaurantViewModel.ItemMenuViewModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public CategoryViewModel()
        {
            Items = new ObservableCollection<MenuRestaurantViewModel.ItemMenuViewModel>();
        }
    }
} 