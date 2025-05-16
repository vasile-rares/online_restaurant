using System.Collections.ObjectModel;

namespace OnlineRestaurant.ViewModels
{
    public class CategorieViewModel : BaseVM
    {
        private int _idCategorie;
        private string _nume = string.Empty;
        private ObservableCollection<ItemMeniuViewModel> _items;

        public int IdCategorie
        {
            get => _idCategorie;
            set => SetProperty(ref _idCategorie, value);
        }

        public string Nume
        {
            get => _nume;
            set => SetProperty(ref _nume, value);
        }

        public ObservableCollection<ItemMeniuViewModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public CategorieViewModel()
        {
            Items = new ObservableCollection<ItemMeniuViewModel>();
        }
    }
} 