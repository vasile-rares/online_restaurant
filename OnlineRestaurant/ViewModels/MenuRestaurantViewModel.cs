using OnlineRestaurant.Commands;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OnlineRestaurant.ViewModels
{
    public class MenuRestaurantViewModel : BaseVM
    {
        private readonly DishService _dishService;
        private readonly CategoryService _categoryService;
        private readonly IRestaurantDataService<Menu> _menuService;
        private readonly IRestaurantDataService<Allergen> _allergenService;

        private ObservableCollection<CategoryViewModel> _categories;
        private string _searchKeyword = string.Empty;
        private bool _searchInverse;
        private bool _searchByAllergen;
        private bool _isSearching;
        private ObservableCollection<CategoryViewModel> _categoryFilter;
        private CategoryViewModel _selectedCategory;
        private bool _isResetting;

        public ObservableCollection<CategoryViewModel> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<CategoryViewModel> CategoryFilter
        {
            get => _categoryFilter;
            set => SetProperty(ref _categoryFilter, value);
        }

        public CategoryViewModel SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value) && !_isResetting)
                {
                    ExecuteSearch();
                }
            }
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    ExecuteSearch();
                }
            }
        }

        public bool SearchInverse
        {
            get => _searchInverse;
            set
            {
                if (SetProperty(ref _searchInverse, value))
                {
                    ExecuteSearch();
                }
            }
        }

        public bool SearchByAllergen
        {
            get => _searchByAllergen;
            set
            {
                if (SetProperty(ref _searchByAllergen, value))
                {
                    ExecuteSearch();
                }
            }
        }

        public bool IsSearching
        {
            get => _isSearching;
            set => SetProperty(ref _isSearching, value);
        }

        public ICommand ResetSearchCommand { get; }

        public MenuRestaurantViewModel(
            DishService dishService,
            CategoryService categoryService,
            IRestaurantDataService<Menu> menuService,
            IRestaurantDataService<Allergen> allergenService)
        {
            _dishService = dishService;
            _categoryService = categoryService;
            _menuService = menuService;
            _allergenService = allergenService;

            Categories = new ObservableCollection<CategoryViewModel>();
            CategoryFilter = new ObservableCollection<CategoryViewModel>();
            ResetSearchCommand = new RelayCommand(ResetSearch);

            // Încărcăm datele inițiale
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Categories.Clear();
            CategoryFilter.Clear();

            try
            {
                var categories = await _categoryService.GetAllAsync();
                var dishes = await _dishService.GetAllAsync();
                var menus = await _menuService.GetAllAsync();
                var allergens = await _allergenService.GetAllAsync();

                // Adăugăm o opțiune "Toate categoriile" la începutul listei de filtre
                CategoryFilter.Add(new CategoryViewModel
                {
                    IdCategory = 0,
                    Name = "Toate categoriile"
                });

                // Grupăm datele pe categorii pentru afișare
                foreach (var category in categories)
                {
                    var categoryVM = new CategoryViewModel
                    {
                        IdCategory = category.IdCategory,
                        Name = category.Name
                    };

                    // Adaugă categoria în lista de filtre
                    CategoryFilter.Add(categoryVM);

                    // Adăugăm preparatele din această categorie
                    var dishesInCategory = dishes.Where(p => p.IdCategory == category.IdCategory).ToList();
                    foreach (var dish in dishesInCategory)
                    {
                        var dishVM = new ItemMenuViewModel
                        {
                            Id = dish.IdDish,
                            Type = ItemMenuType.Dish,
                            Name = dish.Name,
                            Price = dish.Price,
                            PortionSize = dish.PortionSize,
                            Available = dish.InStock,
                            Images = new ObservableCollection<string>(
                                dish.Photos.Select(f => f.Url).ToList()),
                            Allergens = new ObservableCollection<string>(
                                dish.DishAllergens.Select(pa => pa.Allergen.Name).ToList())
                        };
                        categoryVM.Items.Add(dishVM);
                    }

                    // Adăugăm meniurile din această categorie
                    var menusInCategory = menus.Where(m => m.IdCategory == category.IdCategory).ToList();
                    foreach (var menu in menusInCategory)
                    {
                        // Verifică dacă toate preparatele din meniu sunt disponibile
                        bool menuAvailable = menu.MenuDishes
                            .All(mp => mp.Dish.InStock);

                        var menuVM = new ItemMenuViewModel
                        {
                            Id = menu.IdMenu,
                            Type = ItemMenuType.Menu,
                            Name = menu.Name,
                            Price = menu.TotalPrice,
                            Available = menuAvailable,
                            // Pentru meniuri, concatenăm cantitățile preparatelor
                            ContentDetails = string.Join(", ", menu.MenuDishes
                                .Select(mp => $"{mp.Dish.Name} ({mp.Quantity} g)")),
                            // Pentru meniuri, concatenăm alergenii unici din toate preparatele
                            Allergens = new ObservableCollection<string>(
                                menu.MenuDishes
                                    .SelectMany(mp => mp.Dish.DishAllergens.Select(pa => pa.Allergen.Name))
                                    .Distinct()
                                    .ToList())
                        };
                        
                        // Adăugăm o imagine implicită pentru meniu
                        menuVM.Images.Add("/Images/default.jpg");
                        
                        categoryVM.Items.Add(menuVM);
                    }

                    if (categoryVM.Items.Count > 0)
                    {
                        Categories.Add(categoryVM);
                    }
                }

                // Selectează implicit "Toate categoriile"
                _selectedCategory = CategoryFilter[0];
                OnPropertyChanged(nameof(SelectedCategory));
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void ExecuteSearch()
        {
            IsSearching = !string.IsNullOrWhiteSpace(SearchKeyword) || (SelectedCategory != null && SelectedCategory.IdCategory != 0);
            // Run the filter operation directly
            FilterDishesAndMenusAsync();
        }

        private async void FilterDishesAndMenusAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                var dishes = await _dishService.GetAllAsync();
                var menus = await _menuService.GetAllAsync();

                string keyword = SearchKeyword?.Trim().ToLower() ?? string.Empty;

                // Filtru pentru preparate
                var filteredDishes = dishes.Where(p =>
                {
                    bool matchesSearch = true;
                    
                    // Filter by search term
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        matchesSearch = p.Name.ToLower().Contains(keyword);
                    }

                    // Filter by category if a specific category is selected
                    bool matchesCategory = SelectedCategory == null || SelectedCategory.IdCategory == 0 || p.IdCategory == SelectedCategory.IdCategory;

                    return matchesSearch && matchesCategory;
                }).ToList();

                // Filtru pentru meniuri
                var filteredMenus = menus.Where(m =>
                {
                    bool matchesSearch = true;
                    
                    // Filter by search term
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        matchesSearch = m.Name.ToLower().Contains(keyword);
                    }

                    // Filter by category if a specific category is selected
                    bool matchesCategory = SelectedCategory == null || SelectedCategory.IdCategory == 0 || m.IdCategory == SelectedCategory.IdCategory;

                    return matchesSearch && matchesCategory;
                }).ToList();

                // Construim lista de categorii filtrate
                Categories.Clear();

                // Procesare categorii - dacă e selectată o categorie anume, arată doar acea categorie
                IEnumerable<Category> categoriesToShow;
                if (SelectedCategory != null && SelectedCategory.IdCategory != 0)
                {
                    categoriesToShow = categories.Where(c => c.IdCategory == SelectedCategory.IdCategory);
                }
                else
                {
                    // Găsim toate categoriile care au preparate sau meniuri filtrate
                    categoriesToShow = categories
                        .Where(c => 
                            filteredDishes.Any(p => p.IdCategory == c.IdCategory) || 
                            filteredMenus.Any(m => m.IdCategory == c.IdCategory));
                }

                foreach (var category in categoriesToShow)
                {
                    var categoryVM = new CategoryViewModel
                    {
                        IdCategory = category.IdCategory,
                        Name = category.Name
                    };

                    // Add filtered dishes for this category
                    var dishesInCategory = filteredDishes.Where(p => p.IdCategory == category.IdCategory).ToList();
                    foreach (var dish in dishesInCategory)
                    {
                        var dishVM = new ItemMenuViewModel
                        {
                            Id = dish.IdDish,
                            Type = ItemMenuType.Dish,
                            Name = dish.Name,
                            Price = dish.Price,
                            PortionSize = dish.PortionSize,
                            Available = dish.InStock,
                            Images = new ObservableCollection<string>(
                                dish.Photos.Select(f => f.Url).ToList()),
                            Allergens = new ObservableCollection<string>(
                                dish.DishAllergens.Select(pa => pa.Allergen.Name).ToList())
                        };
                        categoryVM.Items.Add(dishVM);
                    }

                    // Add filtered menus for this category
                    var menusInCategory = filteredMenus.Where(m => m.IdCategory == category.IdCategory).ToList();
                    foreach (var menu in menusInCategory)
                    {
                        bool menuAvailable = menu.MenuDishes
                            .All(mp => mp.Dish.InStock);

                        var menuVM = new ItemMenuViewModel
                        {
                            Id = menu.IdMenu,
                            Type = ItemMenuType.Menu,
                            Name = menu.Name,
                            Price = menu.TotalPrice,
                            Available = menuAvailable,
                            ContentDetails = string.Join(", ", menu.MenuDishes
                                .Select(mp => $"{mp.Dish.Name} ({mp.Quantity} g)")),
                            Allergens = new ObservableCollection<string>(
                                menu.MenuDishes
                                    .SelectMany(mp => mp.Dish.DishAllergens.Select(pa => pa.Allergen.Name))
                                    .Distinct()
                                    .ToList())
                        };
                        
                        menuVM.Images.Add("/Images/default.jpg");
                        categoryVM.Items.Add(menuVM);
                    }

                    if (categoryVM.Items.Count > 0)
                    {
                        Categories.Add(categoryVM);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                System.Diagnostics.Debug.WriteLine($"Error filtering data: {ex.Message}");
            }
        }

        private async void ResetSearch()
        {
            try
            {
                _isResetting = true;
                SearchKeyword = string.Empty;
                SelectedCategory = CategoryFilter[0]; // Reset to "All categories"
                SearchInverse = false;
                SearchByAllergen = false;
                IsSearching = false;
                
                // Reload data after all properties are reset
                await Task.Run(() => LoadDataAsync());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting search: {ex.Message}");
            }
            finally
            {
                _isResetting = false;
            }
        }
    }
} 