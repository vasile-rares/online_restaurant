using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Commands;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;

namespace OnlineRestaurant.ViewModels
{
    public class EmployeeViewModel : BaseVM
    {
        private readonly OrderService _orderService;
        private readonly IRestaurantDataService<Dish> _dishService;
        private readonly IRestaurantDataService<Category> _categoryService;
        private readonly IRestaurantDataService<Menu> _menuService;
        private readonly IRestaurantDataService<Allergen> _allergenService;
        private readonly AppSettingsService _appSettingsService;
        private readonly UserViewModel _userViewModel;

        private ObservableCollection<Order> _allOrders;
        private ObservableCollection<Order> _activeOrders;
        private ObservableCollection<Dish> _lowStockDishes;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<Menu> _menus;
        private ObservableCollection<Allergen> _allergens;
        private ObservableCollection<Dish> _dishes;

        private Order _selectedOrder;
        private Category _selectedCategory;
        private Dish _selectedDish;
        private Menu _selectedMenu;
        private Allergen _selectedAllergen;

        private bool _isLoading;
        private string _errorMessage;

        public EmployeeViewModel(
            OrderService orderService,
            IRestaurantDataService<Dish> dishService,
            IRestaurantDataService<Category> categoryService,
            IRestaurantDataService<Menu> menuService,
            IRestaurantDataService<Allergen> allergenService,
            AppSettingsService appSettingsService,
            UserViewModel userViewModel)
        {
            _orderService = orderService;
            _dishService = dishService;
            _categoryService = categoryService;
            _menuService = menuService;
            _allergenService = allergenService;
            _appSettingsService = appSettingsService;
            _userViewModel = userViewModel;

            // Initialize collections
            _allOrders = new ObservableCollection<Order>();
            _activeOrders = new ObservableCollection<Order>();
            _lowStockDishes = new ObservableCollection<Dish>();
            _categories = new ObservableCollection<Category>();
            _menus = new ObservableCollection<Menu>();
            _allergens = new ObservableCollection<Allergen>();
            _dishes = new ObservableCollection<Dish>();

            // Initialize commands
            LoadAllOrdersCommand = new RelayCommand(async () => await LoadAllOrdersAsync());
            LoadActiveOrdersCommand = new RelayCommand(async () => await LoadActiveOrdersAsync());
            LoadLowStockDishesCommand = new RelayCommand(async () => await LoadLowStockDishesAsync());
            LoadCategoriesCommand = new RelayCommand(async () => await LoadCategoriesAsync());
            LoadMenusCommand = new RelayCommand(async () => await LoadMenusAsync());
            LoadAllergensCommand = new RelayCommand(async () => await LoadAllergensAsync());
            LoadDishesCommand = new RelayCommand(async () => await LoadDishesAsync());

            UpdateOrderStatusCommand = new RelayCommand<OrderStatus>(async status => await UpdateOrderStatusAsync(status), _ => SelectedOrder != null && CanUpdateOrderStatus());

            AddCategoryCommand = new RelayCommand(AddCategory);
            EditCategoryCommand = new RelayCommand(EditCategory, () => SelectedCategory != null);
            DeleteCategoryCommand = new RelayCommand(async () => await DeleteCategoryAsync(), () => SelectedCategory != null);

            AddDishCommand = new RelayCommand(AddDish);
            EditDishCommand = new RelayCommand(EditDish, () => SelectedDish != null);
            DeleteDishCommand = new RelayCommand(async () => await DeleteDishAsync(), () => SelectedDish != null);

            AddMenuCommand = new RelayCommand(AddMenu);
            EditMenuCommand = new RelayCommand(EditMenu, () => SelectedMenu != null);
            DeleteMenuCommand = new RelayCommand(async () => await DeleteMenuAsync(), () => SelectedMenu != null);

            AddAllergenCommand = new RelayCommand(AddAllergen);
            EditAllergenCommand = new RelayCommand(EditAllergen, () => SelectedAllergen != null);
            DeleteAllergenCommand = new RelayCommand(async () => await DeleteAllergenAsync(), () => SelectedAllergen != null);

            // Load data initially
            Task.Run(async () =>
            {
                await LoadAllOrdersAsync();
                await LoadActiveOrdersAsync();
                await LoadLowStockDishesAsync();
                await LoadCategoriesAsync();
                await LoadMenusAsync();
                await LoadAllergensAsync();
                await LoadDishesAsync();
            });
        }

        // Properties
        public ObservableCollection<Order> AllOrders
        {
            get => _allOrders;
            set => SetProperty(ref _allOrders, value);
        }

        public ObservableCollection<Order> ActiveOrders
        {
            get => _activeOrders;
            set => SetProperty(ref _activeOrders, value);
        }

        public ObservableCollection<Dish> LowStockDishes
        {
            get => _lowStockDishes;
            set => SetProperty(ref _lowStockDishes, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<Menu> Menus
        {
            get => _menus;
            set => SetProperty(ref _menus, value);
        }

        public ObservableCollection<Allergen> Allergens
        {
            get => _allergens;
            set => SetProperty(ref _allergens, value);
        }

        public ObservableCollection<Dish> Dishes
        {
            get => _dishes;
            set => SetProperty(ref _dishes, value);
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                SetProperty(ref _selectedOrder, value);
                ((RelayCommand<OrderStatus>)UpdateOrderStatusCommand).RaiseCanExecuteChanged();
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                ((RelayCommand)EditCategoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCategoryCommand).RaiseCanExecuteChanged();
            }
        }

        public Dish SelectedDish
        {
            get => _selectedDish;
            set
            {
                SetProperty(ref _selectedDish, value);
                ((RelayCommand)EditDishCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteDishCommand).RaiseCanExecuteChanged();
            }
        }

        public Menu SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                SetProperty(ref _selectedMenu, value);
                ((RelayCommand)EditMenuCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteMenuCommand).RaiseCanExecuteChanged();
            }
        }

        public Allergen SelectedAllergen
        {
            get => _selectedAllergen;
            set
            {
                SetProperty(ref _selectedAllergen, value);
                ((RelayCommand)EditAllergenCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteAllergenCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Commands
        public ICommand LoadAllOrdersCommand { get; }

        public ICommand LoadActiveOrdersCommand { get; }
        public ICommand LoadLowStockDishesCommand { get; }
        public ICommand LoadCategoriesCommand { get; }
        public ICommand LoadMenusCommand { get; }
        public ICommand LoadAllergensCommand { get; }
        public ICommand LoadDishesCommand { get; }

        public ICommand UpdateOrderStatusCommand { get; }

        public ICommand AddCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        public ICommand AddDishCommand { get; }
        public ICommand EditDishCommand { get; }
        public ICommand DeleteDishCommand { get; }

        public ICommand AddMenuCommand { get; }
        public ICommand EditMenuCommand { get; }
        public ICommand DeleteMenuCommand { get; }

        public ICommand AddAllergenCommand { get; }
        public ICommand EditAllergenCommand { get; }
        public ICommand DeleteAllergenCommand { get; }

        // Implementation methods
        private async Task LoadAllOrdersAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var orders = await _orderService.GetAllAsync();

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AllOrders.Clear();
                    foreach (var order in orders.OrderByDescending(o => o.OrderDate))
                    {
                        AllOrders.Add(order);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading orders: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadActiveOrdersAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                System.Diagnostics.Debug.WriteLine("Starting LoadActiveOrdersAsync");

                var activeOrders = await _orderService.GetActiveOrdersAsync();
                System.Diagnostics.Debug.WriteLine($"Received {(activeOrders?.Count() ?? 0)} orders from service");

                // Get the dispatcher from the current application
                var dispatcher = System.Windows.Application.Current.Dispatcher;

                // Update collection on UI thread
                await dispatcher.InvokeAsync(() =>
                {
                    ActiveOrders.Clear();
                    if (activeOrders != null)
                    {
                        foreach (var order in activeOrders)
                        {
                            ActiveOrders.Add(order);
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Added {ActiveOrders.Count} orders to collection");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadActiveOrdersAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ErrorMessage = $"Error loading active orders: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadLowStockDishesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                int lowStockThreshold = _appSettingsService.GetLowStockThreshold();
                var dishes = await _dishService.GetAllAsync();

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    LowStockDishes.Clear();
                    foreach (var dish in dishes.Where(d => d.TotalQuantity <= lowStockThreshold))
                    {
                        LowStockDishes.Add(dish);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading low stock dishes: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var categories = await _categoryService.GetAllAsync();

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Categories.Clear();
                    foreach (var category in categories)
                    {
                        Categories.Add(category);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading categories: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadMenusAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var menus = await _menuService.GetAllAsync();

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Menus.Clear();
                    foreach (var menu in menus)
                    {
                        Menus.Add(menu);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading menus: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAllergensAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var allergens = await _allergenService.GetAllAsync();

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Allergens.Clear();
                    foreach (var allergen in allergens)
                    {
                        Allergens.Add(allergen);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading allergens: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDishesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var dishes = await _dishService.GetAllAsync();

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Dishes.Clear();
                    foreach (var dish in dishes)
                    {
                        Dishes.Add(dish);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading dishes: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateOrderStatusAsync(OrderStatus newStatus)
        {
            try
            {
                if (SelectedOrder == null) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                try
                {
                    var updatedOrder = await _orderService.UpdateOrderStatusAsync(SelectedOrder.IdOrder, newStatus);
                    if (updatedOrder != null)
                    {
                        // Update the UI model
                        SelectedOrder.Status = newStatus;
                    }

                    await LoadActiveOrdersAsync();
                    await LoadAllOrdersAsync();
                }
                catch (Exception dbEx)
                {
                    ErrorMessage = $"Database error: {dbEx.Message}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating order status: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanUpdateOrderStatus()
        {
            if (SelectedOrder == null) return false;

            return SelectedOrder.Status != OrderStatus.delivered && SelectedOrder.Status != OrderStatus.canceled;
        }

        private async void AddCategory()
        {
            try
            {
                var mainWindow = System.Windows.Application.Current.MainWindow;

                var dialog = new Views.Dialogs.CategoryDialog(mainWindow);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    // Add the new category to the database
                    await _categoryService.AddAsync(dialog.Category);
                    await _categoryService.SaveChangesAsync();

                    Categories.Add(dialog.Category);

                    SelectedCategory = dialog.Category;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding category: {ex.Message}";
            }
        }

        private async void EditCategory()
        {
            try
            {
                if (SelectedCategory == null) return;

                // Store the original name and ID
                string originalName = SelectedCategory.Name;
                int categoryId = SelectedCategory.IdCategory;

                var mainWindow = System.Windows.Application.Current.MainWindow;

                var categoryToEdit = new Category
                {
                    IdCategory = categoryId,
                    Name = originalName
                };

                var dialog = new Views.Dialogs.CategoryDialog(mainWindow, categoryToEdit);
                bool? result = dialog.ShowDialog();

                if (result == true && dialog.Category.Name != originalName)
                {
                    try
                    {
                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            var category = await context.Categories
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => c.IdCategory == categoryId);

                            if (category != null)
                            {
                                var updatedCategory = new Category
                                {
                                    IdCategory = categoryId,
                                    Name = dialog.Category.Name
                                };

                                context.Categories.Attach(updatedCategory);
                                context.Entry(updatedCategory).Property(c => c.Name).IsModified = true;

                                // Save changes
                                await context.SaveChangesAsync();

                                SelectedCategory.Name = dialog.Category.Name;

                                await LoadCategoriesAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Error updating category in database: {ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error editing category: {ex.Message}";
            }
        }

        private async Task DeleteCategoryAsync()
        {
            try
            {
                if (SelectedCategory == null) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                var result = System.Windows.MessageBox.Show(
                    $"Are you sure you want to delete the category '{SelectedCategory.Name}'?",
                    "Confirm Delete",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        int categoryId = SelectedCategory.IdCategory;

                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            var category = await context.Categories.FindAsync(categoryId);
                            if (category != null)
                            {
                                context.Categories.Remove(category);
                                await context.SaveChangesAsync();

                                Categories.Remove(SelectedCategory);
                                SelectedCategory = null;
                            }
                            else
                            {
                                ErrorMessage = "Category not found in database.";
                            }
                        }
                    }
                    catch (Exception dbEx)
                    {
                        ErrorMessage = $"Database error: {dbEx.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting category: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void AddDish()
        {
            try
            {
                var mainWindow = System.Windows.Application.Current.MainWindow;

                var categories = new List<Category>(Categories);
                var allergens = new List<Allergen>(Allergens);

                var dialog = new Views.Dialogs.DishDialog(mainWindow, categories, allergens);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    await _dishService.AddAsync(dialog.Dish);
                    await _dishService.SaveChangesAsync();

                    Dishes.Add(dialog.Dish);

                    int lowStockThreshold = _appSettingsService.GetLowStockThreshold();
                    if (dialog.Dish.TotalQuantity <= lowStockThreshold)
                    {
                        LowStockDishes.Add(dialog.Dish);
                    }

                    SelectedDish = dialog.Dish;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding dish: {ex.Message}";
            }
        }

        private async void EditDish()
        {
            try
            {
                if (SelectedDish == null) return;

                string originalName = SelectedDish.Name;
                int originalCategoryId = SelectedDish.IdCategory;
                decimal originalPrice = SelectedDish.Price;
                int originalPortionSize = SelectedDish.PortionSize;
                int originalTotalQuantity = SelectedDish.TotalQuantity;

                var mainWindow = System.Windows.Application.Current.MainWindow;

                var categories = new List<Category>(Categories);

                var allergens = new List<Allergen>(Allergens);

                var dialog = new Views.Dialogs.DishDialog(mainWindow, SelectedDish, categories, allergens);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    try
                    {
                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            var dishFromDb = await context.Dishes
                                .AsNoTracking()
                                .FirstOrDefaultAsync(d => d.IdDish == SelectedDish.IdDish);

                            if (dishFromDb != null)
                            {
                                var updatedDish = new Dish
                                {
                                    IdDish = SelectedDish.IdDish,
                                    Name = dialog.Dish.Name,
                                    IdCategory = dialog.Dish.IdCategory,
                                    Price = dialog.Dish.Price,
                                    PortionSize = dialog.Dish.PortionSize,
                                    TotalQuantity = dialog.Dish.TotalQuantity
                                };

                                // Attach with modified state
                                context.Dishes.Attach(updatedDish);
                                context.Entry(updatedDish).Property(d => d.Name).IsModified = true;
                                context.Entry(updatedDish).Property(d => d.IdCategory).IsModified = true;
                                context.Entry(updatedDish).Property(d => d.Price).IsModified = true;
                                context.Entry(updatedDish).Property(d => d.PortionSize).IsModified = true;
                                context.Entry(updatedDish).Property(d => d.TotalQuantity).IsModified = true;

                                await context.SaveChangesAsync();

                                if (dialog.Dish.Photos != null && dialog.Dish.Photos.Any())
                                {
                                    var existingPhotos = await context.DishPhotos
                                        .Where(dp => dp.IdDish == SelectedDish.IdDish)
                                        .ToListAsync();

                                    if (existingPhotos.Any())
                                    {
                                        context.DishPhotos.RemoveRange(existingPhotos);
                                        await context.SaveChangesAsync();
                                    }

                                    foreach (var photo in dialog.Dish.Photos)
                                    {
                                        var newPhoto = new DishPhoto
                                        {
                                            IdDish = SelectedDish.IdDish,
                                            Url = photo.Url
                                        };

                                        context.DishPhotos.Add(newPhoto);
                                    }

                                    await context.SaveChangesAsync();
                                }

                                if (dialog.Dish.DishAllergens != null)
                                {
                                    var existingAllergens = await context.DishAllergens
                                        .Where(da => da.IdDish == SelectedDish.IdDish)
                                        .ToListAsync();

                                    if (existingAllergens.Any())
                                    {
                                        context.DishAllergens.RemoveRange(existingAllergens);
                                        await context.SaveChangesAsync();
                                    }

                                    foreach (var allergen in dialog.Dish.DishAllergens)
                                    {
                                        var newDishAllergen = new DishAllergen
                                        {
                                            IdDish = SelectedDish.IdDish,
                                            IdAllergen = allergen.IdAllergen
                                        };

                                        context.DishAllergens.Add(newDishAllergen);
                                    }

                                    await context.SaveChangesAsync();
                                }

                                SelectedDish.Name = dialog.Dish.Name;
                                SelectedDish.IdCategory = dialog.Dish.IdCategory;
                                SelectedDish.Price = dialog.Dish.Price;
                                SelectedDish.PortionSize = dialog.Dish.PortionSize;
                                SelectedDish.TotalQuantity = dialog.Dish.TotalQuantity;
                                SelectedDish.Photos = new List<DishPhoto>(dialog.Dish.Photos);
                                SelectedDish.DishAllergens = new List<DishAllergen>(dialog.Dish.DishAllergens);

                                int lowStockThreshold = _appSettingsService.GetLowStockThreshold();
                                bool wasLowStock = originalTotalQuantity <= lowStockThreshold;
                                bool isLowStock = SelectedDish.TotalQuantity <= lowStockThreshold;

                                if (wasLowStock && !isLowStock)
                                {
                                    var dishToRemove = LowStockDishes.FirstOrDefault(d => d.IdDish == SelectedDish.IdDish);
                                    if (dishToRemove != null)
                                    {
                                        LowStockDishes.Remove(dishToRemove);
                                    }
                                }
                                else if (!wasLowStock && isLowStock)
                                {
                                    LowStockDishes.Add(SelectedDish);
                                }

                                await LoadDishesAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Error updating dish in database: {ex.Message}";
                        System.Diagnostics.Debug.WriteLine($"Edit Dish Error: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error editing dish: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Edit Dish Outer Error: {ex}");
            }
        }

        private async Task DeleteDishAsync()
        {
            try
            {
                if (SelectedDish == null) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                var result = System.Windows.MessageBox.Show(
                    $"Are you sure you want to delete the dish '{SelectedDish.Name}'?",
                    "Confirm Delete",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        int dishId = SelectedDish.IdDish;

                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            var dish = await context.Dishes.FindAsync(dishId);
                            if (dish != null)
                            {
                                var dishPhotos = await context.DishPhotos
                                    .Where(dp => dp.IdDish == dishId)
                                    .ToListAsync();

                                if (dishPhotos.Any())
                                {
                                    context.DishPhotos.RemoveRange(dishPhotos);
                                    await context.SaveChangesAsync();
                                }

                                var dishAllergens = await context.DishAllergens
                                    .Where(da => da.IdDish == dishId)
                                    .ToListAsync();

                                if (dishAllergens.Any())
                                {
                                    context.DishAllergens.RemoveRange(dishAllergens);
                                    await context.SaveChangesAsync();
                                }

                                var menuDishes = await context.MenuDishes
                                    .Where(md => md.IdDish == dishId)
                                    .ToListAsync();

                                if (menuDishes.Any())
                                {
                                    context.MenuDishes.RemoveRange(menuDishes);
                                    await context.SaveChangesAsync();
                                }

                                context.Dishes.Remove(dish);
                                await context.SaveChangesAsync();

                                Dishes.Remove(SelectedDish);

                                var dishInLowStock = LowStockDishes.FirstOrDefault(d => d.IdDish == dishId);
                                if (dishInLowStock != null)
                                {
                                    LowStockDishes.Remove(dishInLowStock);
                                }

                                SelectedDish = null;

                                await LoadDishesAsync();
                                await LoadMenusAsync();
                            }
                            else
                            {
                                ErrorMessage = "Dish not found in database.";
                            }
                        }
                    }
                    catch (Exception dbEx)
                    {
                        ErrorMessage = $"Database error: {dbEx.Message}";
                        System.Diagnostics.Debug.WriteLine($"DELETE ERROR DETAILS: {dbEx}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting dish: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"DELETE ERROR OUTER: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void AddMenu()
        {
            try
            {
                var mainWindow = System.Windows.Application.Current.MainWindow;

                var categories = new List<Category>(Categories);

                var dishes = new List<Dish>(Dishes);

                var dialog = new Views.Dialogs.MenuDialog(mainWindow, categories, dishes, _appSettingsService);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    using (var context = new Data.RestaurantDbContext(
                        new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                            .UseSqlServer(_appSettingsService.ConnectionString)
                            .Options))
                    {
                        context.Menus.Add(dialog.Menu);
                        await context.SaveChangesAsync();

                        foreach (var menuDish in dialog.SelectedDishes)
                        {
                            context.MenuDishes.Add(new MenuDish
                            {
                                IdMenu = dialog.Menu.IdMenu,
                                IdDish = menuDish.Dish.IdDish,
                                Quantity = menuDish.Quantity
                            });
                        }

                        await context.SaveChangesAsync();

                        Menus.Add(dialog.Menu);
                        SelectedMenu = dialog.Menu;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding menu: {ex.Message}";
            }
        }

        private async void EditMenu()
        {
            try
            {
                if (SelectedMenu == null) return;

                string originalName = SelectedMenu.Name;
                int originalCategoryId = SelectedMenu.IdCategory;

                var mainWindow = System.Windows.Application.Current.MainWindow;

                var categories = new List<Category>(Categories);
                var dishes = new List<Dish>(Dishes);

                List<MenuDish> menuDishes = new List<MenuDish>();
                try
                {
                    using (var context = new Data.RestaurantDbContext(
                        new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                            .UseSqlServer(_appSettingsService.ConnectionString)
                            .Options))
                    {
                        menuDishes = await context.MenuDishes
                            .AsNoTracking()
                            .Where(md => md.IdMenu == SelectedMenu.IdMenu)
                            .ToListAsync();
                    }
                }
                catch (Exception dbEx)
                {
                    ErrorMessage = $"Error loading menu details: {dbEx.Message}";
                    return;
                }

                var dialog = new Views.Dialogs.MenuDialog(mainWindow, SelectedMenu, categories, dishes, menuDishes, _appSettingsService);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    try
                    {
                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            var menuFromDb = await context.Menus
                                .AsNoTracking()
                                .FirstOrDefaultAsync(m => m.IdMenu == SelectedMenu.IdMenu);

                            if (menuFromDb != null)
                            {
                                var updatedMenu = new Menu
                                {
                                    IdMenu = SelectedMenu.IdMenu,
                                    Name = dialog.Menu.Name,
                                    IdCategory = dialog.Menu.IdCategory
                                };

                                context.Menus.Attach(updatedMenu);
                                context.Entry(updatedMenu).Property(m => m.Name).IsModified = true;
                                context.Entry(updatedMenu).Property(m => m.IdCategory).IsModified = true;

                                var existingMenuDishes = await context.MenuDishes
                                    .Where(md => md.IdMenu == SelectedMenu.IdMenu)
                                    .ToListAsync();

                                if (existingMenuDishes.Any())
                                {
                                    context.MenuDishes.RemoveRange(existingMenuDishes);
                                }

                                foreach (var menuDish in dialog.SelectedDishes)
                                {
                                    context.MenuDishes.Add(new MenuDish
                                    {
                                        IdMenu = SelectedMenu.IdMenu,
                                        IdDish = menuDish.Dish.IdDish,
                                        Quantity = menuDish.Quantity
                                    });
                                }

                                await context.SaveChangesAsync();

                                SelectedMenu.Name = dialog.Menu.Name;
                                SelectedMenu.IdCategory = dialog.Menu.IdCategory;

                                await LoadMenusAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Error updating menu in database: {ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error editing menu: {ex.Message}";
            }
        }

        private async Task DeleteMenuAsync()
        {
            try
            {
                if (SelectedMenu == null) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                var result = System.Windows.MessageBox.Show(
                    $"Are you sure you want to delete the menu '{SelectedMenu.Name}'?",
                    "Confirm Delete",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        int menuId = SelectedMenu.IdMenu;

                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            var menu = await context.Menus.FindAsync(menuId);
                            if (menu != null)
                            {
                                var menuDishes = await context.MenuDishes
                                    .Where(md => md.IdMenu == menuId)
                                    .ToListAsync();

                                if (menuDishes.Any())
                                {
                                    context.MenuDishes.RemoveRange(menuDishes);
                                    await context.SaveChangesAsync();
                                }

                                var orderMenus = await context.OrderMenus
                                    .Where(om => om.IdMenu == menuId)
                                    .ToListAsync();

                                if (orderMenus.Any())
                                {
                                    context.OrderMenus.RemoveRange(orderMenus);
                                    await context.SaveChangesAsync();
                                }

                                context.Menus.Remove(menu);
                                await context.SaveChangesAsync();

                                Menus.Remove(SelectedMenu);
                                SelectedMenu = null;
                            }
                            else
                            {
                                ErrorMessage = "Menu not found in database.";
                            }
                        }
                    }
                    catch (Exception dbEx)
                    {
                        ErrorMessage = $"Database error: {dbEx.Message}";
                        System.Diagnostics.Debug.WriteLine($"DELETE MENU ERROR DETAILS: {dbEx}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting menu: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"DELETE MENU ERROR OUTER: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void AddAllergen()
        {
            try
            {
                var mainWindow = System.Windows.Application.Current.MainWindow;

                var dialog = new Views.Dialogs.AllergenDialog(mainWindow);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    await _allergenService.AddAsync(dialog.Allergen);
                    await _allergenService.SaveChangesAsync();

                    Allergens.Add(dialog.Allergen);

                    SelectedAllergen = dialog.Allergen;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding allergen: {ex.Message}";
            }
        }

        private async void EditAllergen()
        {
            try
            {
                if (SelectedAllergen == null) return;

                string originalName = SelectedAllergen.Name;
                int allergenId = SelectedAllergen.IdAllergen;

                var mainWindow = System.Windows.Application.Current.MainWindow;

                var allergenToEdit = new Allergen
                {
                    IdAllergen = allergenId,
                    Name = originalName
                };

                var dialog = new Views.Dialogs.AllergenDialog(mainWindow, allergenToEdit);
                bool? result = dialog.ShowDialog();

                if (result == true && dialog.Allergen.Name != originalName)
                {
                    try
                    {
                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            var allergen = await context.Allergens
                                .AsNoTracking()
                                .FirstOrDefaultAsync(a => a.IdAllergen == allergenId);

                            if (allergen != null)
                            {
                                var updatedAllergen = new Allergen
                                {
                                    IdAllergen = allergenId,
                                    Name = dialog.Allergen.Name
                                };

                                context.Allergens.Attach(updatedAllergen);
                                context.Entry(updatedAllergen).Property(a => a.Name).IsModified = true;

                                await context.SaveChangesAsync();

                                SelectedAllergen.Name = dialog.Allergen.Name;

                                await LoadAllergensAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Error updating allergen in database: {ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error editing allergen: {ex.Message}";
            }
        }

        private async Task DeleteAllergenAsync()
        {
            try
            {
                if (SelectedAllergen == null) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                var result = System.Windows.MessageBox.Show(
                    $"Are you sure you want to delete the allergen '{SelectedAllergen.Name}'?",
                    "Confirm Delete",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        int allergenId = SelectedAllergen.IdAllergen;

                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            var allergen = await context.Allergens.FindAsync(allergenId);
                            if (allergen != null)
                            {
                                context.Allergens.Remove(allergen);
                                await context.SaveChangesAsync();

                                Allergens.Remove(SelectedAllergen);
                                SelectedAllergen = null;
                            }
                            else
                            {
                                ErrorMessage = "Allergen not found in database.";
                            }
                        }
                    }
                    catch (Exception dbEx)
                    {
                        ErrorMessage = $"Database error: {dbEx.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting allergen: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected override void OnDispose()
        {
            // Cleanup resources
            base.OnDispose();
        }
    }
}