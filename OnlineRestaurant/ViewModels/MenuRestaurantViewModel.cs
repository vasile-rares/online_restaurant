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
        // Nested enum
        public enum ItemMenuType
        {
            Dish,
            Menu
        }

        // Nested class for menu items
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

        // Clasă simplă pentru categoria de meniu - doar pentru a facilita binding-ul în UI
        public class CategoryGroup : BaseVM
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ObservableCollection<ItemMenuViewModel> Items { get; set; } = new();
        }

        private readonly IRestaurantDataService<Dish> _dishService;
        private readonly IRestaurantDataService<Category> _categoryService;
        private readonly IRestaurantDataService<Menu> _menuService;
        private readonly IRestaurantDataService<Allergen> _allergenService;
        private ShoppingCartViewModel _shoppingCart;
        private UserViewModel _userViewModel;

        private ObservableCollection<CategoryGroup> _categories;
        private string _searchKeyword = string.Empty;
        private bool _searchInverse;
        private bool _searchByAllergen;
        private bool _isSearching;
        private ObservableCollection<CategoryGroup> _categoryFilter;
        private CategoryGroup _selectedCategory;
        private bool _isResetting;

        public ObservableCollection<CategoryGroup> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<CategoryGroup> CategoryFilter
        {
            get => _categoryFilter;
            set => SetProperty(ref _categoryFilter, value);
        }

        public CategoryGroup SelectedCategory
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

        public ShoppingCartViewModel ShoppingCart
        {
            get => _shoppingCart;
            set => SetProperty(ref _shoppingCart, value);
        }

        public UserViewModel UserViewModel
        {
            get => _userViewModel;
            set
            {
                var oldViewModel = _userViewModel;
                if (SetProperty(ref _userViewModel, value))
                {
                    // Detach events from old instance
                    if (oldViewModel != null)
                    {
                        oldViewModel.PropertyChanged -= OnUserViewModelPropertyChanged;
                    }

                    // Attach events to new instance
                    if (_userViewModel != null)
                    {
                        _userViewModel.PropertyChanged += OnUserViewModelPropertyChanged;

                        // Update all items with the current login state
                        UpdateAllItemsLoginState(_userViewModel.IsLoggedIn);
                    }
                }
            }
        }

        public ICommand ResetSearchCommand { get; }

        public MenuRestaurantViewModel(
            IRestaurantDataService<Dish> dishService,
            IRestaurantDataService<Category> categoryService,
            IRestaurantDataService<Menu> menuService,
            IRestaurantDataService<Allergen> allergenService)
        {
            _dishService = dishService;
            _categoryService = categoryService;
            _menuService = menuService;
            _allergenService = allergenService;

            Categories = new ObservableCollection<CategoryGroup>();
            CategoryFilter = new ObservableCollection<CategoryGroup>();
            ResetSearchCommand = new RelayCommand(ResetSearch);

            // Încărcăm datele inițiale
            LoadDataAsync();
        }

        private void OnUserViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserViewModel.IsLoggedIn))
            {
                if (UserViewModel != null)
                {
                    UpdateAllItemsLoginState(UserViewModel.IsLoggedIn);
                }
            }
        }

        private void UpdateAllItemsLoginState(bool isLoggedIn)
        {
            if (Categories == null)
                return;

            foreach (var category in Categories)
            {
                if (category?.Items == null)
                    continue;

                foreach (var item in category.Items)
                {
                    if (item == null)
                        continue;

                    // Only update if the value actually changed
                    bool newCanAddToCart = item.Available && isLoggedIn;
                    if (item.CanAddToCart != newCanAddToCart)
                    {
                        item.CanAddToCart = newCanAddToCart;

                        // Update command execute state only if needed
                        if (item.AddToCartCommand is RelayCommand command)
                        {
                            command.RaiseCanExecuteChanged();
                        }
                    }
                }
            }
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
                CategoryFilter.Add(new CategoryGroup
                {
                    Id = 0,
                    Name = "Toate categoriile"
                });

                // Grupăm datele pe categorii pentru afișare
                foreach (var category in categories)
                {
                    var categoryGroup = new CategoryGroup
                    {
                        Id = category.IdCategory,
                        Name = category.Name
                    };

                    // Adaugă categoria în lista de filtre
                    CategoryFilter.Add(categoryGroup);

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
                        categoryGroup.Items.Add(dishVM);
                        InitializeAddToCartCommands(dishVM);
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
                            PortionSize = menu.MenuDishes.Sum(md => md.Quantity),
                            // Pentru meniuri, concatenăm cantitățile preparatelor
                            ContentDetails = string.Join(", ", menu.MenuDishes
                                .Select(mp => $"{mp.Dish.Name} ({mp.Quantity} g)")),
                            // Pentru meniuri, concatenăm alergenii unici din toate preparatele
                            Allergens = new ObservableCollection<string>(
                                menu.MenuDishes
                                    .SelectMany(md => md.Dish.DishAllergens)
                                    .Select(da => da.Allergen.Name)
                                    .Distinct()
                                    .OrderBy(name => name)
                                    .ToList())
                        };

                        // Adăugăm o imagine implicită pentru meniu
                        menuVM.Images.Add("/Images/default.jpg");

                        categoryGroup.Items.Add(menuVM);
                        InitializeAddToCartCommands(menuVM);
                    }

                    if (categoryGroup.Items.Count > 0)
                    {
                        Categories.Add(categoryGroup);
                    }
                }

                // Selectează implicit "Toate categoriile"
                _selectedCategory = CategoryFilter[0];
                OnPropertyChanged(nameof(SelectedCategory));

                // Update login state
                if (UserViewModel != null)
                {
                    UpdateAllItemsLoginState(UserViewModel.IsLoggedIn);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void ExecuteSearch()
        {
            IsSearching = !string.IsNullOrWhiteSpace(SearchKeyword) || (SelectedCategory != null && SelectedCategory.Id != 0);
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
                bool matchesCategory = SelectedCategory == null || SelectedCategory.Id == 0 || p.IdCategory == SelectedCategory.Id;

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
                bool matchesCategory = SelectedCategory == null || SelectedCategory.Id == 0 || m.IdCategory == SelectedCategory.Id;

                return matchesSearch && matchesCategory;
            }).ToList();

                // Construim lista de categorii filtrate
                Categories.Clear();

                // Procesare categorii - dacă e selectată o categorie anume, arată doar acea categorie
                IEnumerable<Category> categoriesToShow;
                if (SelectedCategory != null && SelectedCategory.Id != 0)
                {
                    categoriesToShow = categories.Where(c => c.IdCategory == SelectedCategory.Id);
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
                    var categoryGroup = new CategoryGroup
                    {
                        Id = category.IdCategory,
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
                        categoryGroup.Items.Add(dishVM);
                        InitializeAddToCartCommands(dishVM);
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
                            PortionSize = menu.MenuDishes.Sum(md => md.Quantity),
                            // Pentru meniuri, concatenăm cantitățile preparatelor
                            ContentDetails = string.Join(", ", menu.MenuDishes
                                .Select(mp => $"{mp.Dish.Name} ({mp.Quantity} g)")),
                            // Pentru meniuri, concatenăm alergenii unici din toate preparatele
                            Allergens = new ObservableCollection<string>(
                                menu.MenuDishes
                                    .SelectMany(md => md.Dish.DishAllergens)
                                    .Select(da => da.Allergen.Name)
                                    .Distinct()
                                    .OrderBy(name => name)
                                    .ToList())
                        };

                        menuVM.Images.Add("/Images/default.jpg");
                        categoryGroup.Items.Add(menuVM);
                        InitializeAddToCartCommands(menuVM);
                    }

                    if (categoryGroup.Items.Count > 0)
                    {
                        Categories.Add(categoryGroup);
                    }
                }

                // Update login state for all items
                if (UserViewModel != null)
                {
                    UpdateAllItemsLoginState(UserViewModel.IsLoggedIn);
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

        private void InitializeAddToCartCommands(ItemMenuViewModel item)
        {
            // Set the current login state
            bool isLoggedIn = UserViewModel?.IsLoggedIn ?? false;
            item.CanAddToCart = item.Available && isLoggedIn;

            // Initialize the command without capturing event handlers
            item.AddToCartCommand = new RelayCommand(
                () => ShoppingCart?.AddToCart(item),
                () => item.Available && (UserViewModel?.IsLoggedIn ?? false)
            );
        }

        // Override OnDispose to clean up resources
        protected override void OnDispose()
        {
            // Clean up managed resources
            if (_userViewModel != null)
            {
                _userViewModel.PropertyChanged -= OnUserViewModelPropertyChanged;
            }

            // Clear collections
            if (Categories != null)
            {
                foreach (var category in Categories)
                {
                    if (category?.Items != null)
                    {
                        category.Items.Clear();
                    }
                }
                Categories.Clear();
            }

            if (CategoryFilter != null)
            {
                CategoryFilter.Clear();
            }

            base.OnDispose();
        }
    }
}