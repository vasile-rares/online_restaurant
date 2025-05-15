using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace OnlineRestaurant.ViewModels
{
    public class MainViewModel : BaseVM
    {
        private readonly PreparatService _preparatService;
        private readonly CategorieService _categorieService;
        private ObservableCollection<Categorie> _categorii;
        private ObservableCollection<Preparat> _preparate;

        public ObservableCollection<Categorie> Categorii
        {
            get => _categorii;
            set => SetProperty(ref _categorii, value);
        }

        public ObservableCollection<Preparat> Preparate
        {
            get => _preparate;
            set => SetProperty(ref _preparate, value);
        }

        public MainViewModel(PreparatService preparatService, CategorieService categorieService)
        {
            _preparatService = preparatService;
            _categorieService = categorieService;
            
            // Inițializare colecții goale
            Categorii = new ObservableCollection<Categorie>();
            Preparate = new ObservableCollection<Preparat>();
            
            // Încărcare date la inițializare
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadCategories();
            await LoadPreparate();
        }

        private async Task LoadCategories()
        {
            var categorii = await _categorieService.GetAllAsync();
            Categorii.Clear();
            foreach (var categorie in categorii)
            {
                Categorii.Add(categorie);
            }
        }

        private async Task LoadPreparate()
        {
            var preparate = await _preparatService.GetAllAsync();
            Preparate.Clear();
            foreach (var preparat in preparate)
            {
                Preparate.Add(preparat);
            }
        }
    }
} 