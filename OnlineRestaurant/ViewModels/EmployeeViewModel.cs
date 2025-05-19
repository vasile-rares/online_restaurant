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
    public class EmployeeViewModel : BaseVM, IDisposable
    {
        private readonly IRestaurantDataService<Order> _orderService;
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
        private bool _isDisposed;

        // Constructor
        public EmployeeViewModel(
            IRestaurantDataService<Order> orderService,
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
                AllOrders.Clear();
                foreach (var order in orders.OrderByDescending(o => o.OrderDate))
                {
                    AllOrders.Add(order);
                }
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

                var allOrders = await _orderService.GetAllAsync();
                var activeOrders = allOrders.Where(o => o.Status != OrderStatus.delivered && o.Status != OrderStatus.canceled)
                                           .OrderBy(o => o.OrderDate);

                ActiveOrders.Clear();
                foreach (var order in activeOrders)
                {
                    ActiveOrders.Add(order);
                }
            }
            catch (Exception ex)
            {
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

                // Retrieve the threshold value from appsettings.json
                int lowStockThreshold = _appSettingsService.GetLowStockThreshold();

                var dishes = await _dishService.GetAllAsync();
                LowStockDishes.Clear();
                foreach (var dish in dishes.Where(d => d.TotalQuantity <= lowStockThreshold))
                {
                    LowStockDishes.Add(dish);
                }
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
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
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
                Menus.Clear();
                foreach (var menu in menus)
                {
                    Menus.Add(menu);
                }
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
                Allergens.Clear();
                foreach (var allergen in allergens)
                {
                    Allergens.Add(allergen);
                }
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
                Dishes.Clear();
                foreach (var dish in dishes)
                {
                    Dishes.Add(dish);
                }
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
                    // Direct database approach with minimal entity tracking
                    using (var context = new Data.RestaurantDbContext(
                        new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                            .UseSqlServer(_appSettingsService.ConnectionString)
                            .Options))
                    {
                        var order = await context.Orders.FindAsync(SelectedOrder.IdOrder);
                        if (order != null)
                        {
                            order.Status = newStatus;
                            await context.SaveChangesAsync();

                            // Update the UI model
                            SelectedOrder.Status = newStatus;
                        }
                    }

                    // Refresh lists after successful update
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

            // Can only update orders that are not delivered or canceled
            return SelectedOrder.Status != OrderStatus.delivered && SelectedOrder.Status != OrderStatus.canceled;
        }

        // CRUD operations for Categories, Dishes, Menus, and Allergens
        // These methods would create/update dialogs or navigate to dedicated forms
        private async void AddCategory()
        {
            try
            {
                // Get the MainWindow as the owner for the dialog
                var mainWindow = System.Windows.Application.Current.MainWindow;

                // Create and show the dialog for adding a new category
                var dialog = new Views.CategoryDialog(mainWindow);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    // Add the new category to the database
                    await _categoryService.AddAsync(dialog.Category);
                    await _categoryService.SaveChangesAsync();

                    // Add the new category to the collection
                    Categories.Add(dialog.Category);

                    // Select the newly added category
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

                // Get the MainWindow as the owner for the dialog
                var mainWindow = System.Windows.Application.Current.MainWindow;

                // Create a detached copy of the selected category to edit
                var categoryToEdit = new Category
                {
                    IdCategory = categoryId,
                    Name = originalName
                };

                // Create and show the dialog for editing the category
                var dialog = new Views.CategoryDialog(mainWindow, categoryToEdit);
                bool? result = dialog.ShowDialog();

                if (result == true && dialog.Category.Name != originalName)
                {
                    try
                    {
                        // Use direct SQL connection to avoid entity tracking issues
                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            // Find the category by ID without tracking
                            var category = await context.Categories
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => c.IdCategory == categoryId);

                            if (category != null)
                            {
                                // Create a new instance to update
                                var updatedCategory = new Category
                                {
                                    IdCategory = categoryId,
                                    Name = dialog.Category.Name
                                };

                                // Attach with modified state
                                context.Categories.Attach(updatedCategory);
                                context.Entry(updatedCategory).Property(c => c.Name).IsModified = true;
                                
                                // Save changes
                                await context.SaveChangesAsync();
                                
                                // Update the UI model
                                SelectedCategory.Name = dialog.Category.Name;
                                
                                // Refresh the categories list
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

                // Ask for confirmation
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

                        // Use direct database approach to ensure deletion
                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            // Find the category by ID
                            var category = await context.Categories.FindAsync(categoryId);
                            if (category != null)
                            {
                                // Remove from context and save changes
                                context.Categories.Remove(category);
                                await context.SaveChangesAsync();
                                
                                // Remove from UI collection
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
                // Get the MainWindow as the owner for the dialog
                var mainWindow = System.Windows.Application.Current.MainWindow;
                
                // Get categories for the dish dialog
                var categories = new List<Category>(Categories);
                
                // Create and show the dialog for adding a new dish
                var dialog = new Views.DishDialog(mainWindow, categories);
                bool? result = dialog.ShowDialog();
                
                if (result == true)
                {
                    // Add the new dish to the database
                    await _dishService.AddAsync(dialog.Dish);
                    await _dishService.SaveChangesAsync();
                    
                    // Add the new dish to the collection
                    Dishes.Add(dialog.Dish);
                    
                    // Also check if it should be added to low stock dishes
                    int lowStockThreshold = _appSettingsService.GetLowStockThreshold();
                    if (dialog.Dish.TotalQuantity <= lowStockThreshold)
                    {
                        LowStockDishes.Add(dialog.Dish);
                    }
                    
                    // Select the newly added dish
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
                
                // Store the original values for comparison
                string originalName = SelectedDish.Name;
                int originalCategoryId = SelectedDish.IdCategory;
                decimal originalPrice = SelectedDish.Price;
                int originalPortionSize = SelectedDish.PortionSize;
                int originalTotalQuantity = SelectedDish.TotalQuantity;
                
                // Get the MainWindow as the owner for the dialog
                var mainWindow = System.Windows.Application.Current.MainWindow;
                
                // Get categories for the dish dialog
                var categories = new List<Category>(Categories);
                
                // Create and show the dialog for editing the dish
                var dialog = new Views.DishDialog(mainWindow, SelectedDish, categories);
                bool? result = dialog.ShowDialog();
                
                if (result == true)
                {
                    try
                    {
                        // Use direct database approach to avoid entity tracking issues
                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            // Find the dish by ID without tracking
                            var dishFromDb = await context.Dishes
                                .AsNoTracking()
                                .FirstOrDefaultAsync(d => d.IdDish == SelectedDish.IdDish);
                                
                            if (dishFromDb != null)
                            {
                                // Create a new instance to update
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
                                
                                // Save changes
                                await context.SaveChangesAsync();
                                
                                // Update the UI model
                                SelectedDish.Name = dialog.Dish.Name;
                                SelectedDish.IdCategory = dialog.Dish.IdCategory;
                                SelectedDish.Price = dialog.Dish.Price;
                                SelectedDish.PortionSize = dialog.Dish.PortionSize;
                                SelectedDish.TotalQuantity = dialog.Dish.TotalQuantity;
                                
                                // Check if LowStockDishes collection needs to be updated
                                int lowStockThreshold = _appSettingsService.GetLowStockThreshold();
                                bool wasLowStock = originalTotalQuantity <= lowStockThreshold;
                                bool isLowStock = SelectedDish.TotalQuantity <= lowStockThreshold;
                                
                                if (wasLowStock && !isLowStock)
                                {
                                    // Remove from low stock collection
                                    var dishToRemove = LowStockDishes.FirstOrDefault(d => d.IdDish == SelectedDish.IdDish);
                                    if (dishToRemove != null)
                                    {
                                        LowStockDishes.Remove(dishToRemove);
                                    }
                                }
                                else if (!wasLowStock && isLowStock)
                                {
                                    // Add to low stock collection
                                    LowStockDishes.Add(SelectedDish);
                                }
                                
                                // Refresh the dishes list
                                await LoadDishesAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Error updating dish in database: {ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error editing dish: {ex.Message}";
            }
        }

        private async Task DeleteDishAsync()
        {
            try
            {
                if (SelectedDish == null) return;

                IsLoading = true;
                ErrorMessage = string.Empty;

                // Ask for confirmation
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

                        // Use direct database approach to ensure deletion
                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            // Find the dish by ID
                            var dish = await context.Dishes.FindAsync(dishId);
                            if (dish != null)
                            {
                                // Remove from context and save changes
                                context.Dishes.Remove(dish);
                                await context.SaveChangesAsync();
                                
                                // Remove from UI collections
                                Dishes.Remove(SelectedDish);
                                
                                // Remove from low stock dishes if present
                                var dishInLowStock = LowStockDishes.FirstOrDefault(d => d.IdDish == dishId);
                                if (dishInLowStock != null)
                                {
                                    LowStockDishes.Remove(dishInLowStock);
                                }
                                
                                SelectedDish = null;
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
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting dish: {ex.Message}";
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
                // Get the MainWindow as the owner for the dialog
                var mainWindow = System.Windows.Application.Current.MainWindow;
                
                // Get categories for the menu dialog
                var categories = new List<Category>(Categories);
                
                // Create and show the dialog for adding a new menu
                var dialog = new Views.MenuDialog(mainWindow, categories);
                bool? result = dialog.ShowDialog();
                
                if (result == true)
                {
                    // Add the new menu to the database
                    await _menuService.AddAsync(dialog.Menu);
                    await _menuService.SaveChangesAsync();
                    
                    // Add the new menu to the collection
                    Menus.Add(dialog.Menu);
                    
                    // Select the newly added menu
                    SelectedMenu = dialog.Menu;
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
                
                // Store the original values for comparison
                string originalName = SelectedMenu.Name;
                int originalCategoryId = SelectedMenu.IdCategory;
                
                // Get the MainWindow as the owner for the dialog
                var mainWindow = System.Windows.Application.Current.MainWindow;
                
                // Get categories for the menu dialog
                var categories = new List<Category>(Categories);
                
                // Create and show the dialog for editing the menu
                var dialog = new Views.MenuDialog(mainWindow, SelectedMenu, categories);
                bool? result = dialog.ShowDialog();
                
                if (result == true)
                {
                    try
                    {
                        // Use direct database approach to avoid entity tracking issues
                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            // Find the menu by ID without tracking
                            var menuFromDb = await context.Menus
                                .AsNoTracking()
                                .FirstOrDefaultAsync(m => m.IdMenu == SelectedMenu.IdMenu);
                                
                            if (menuFromDb != null)
                            {
                                // Create a new instance to update
                                var updatedMenu = new Menu
                                {
                                    IdMenu = SelectedMenu.IdMenu,
                                    Name = dialog.Menu.Name,
                                    IdCategory = dialog.Menu.IdCategory
                                };
                                
                                // Attach with modified state
                                context.Menus.Attach(updatedMenu);
                                context.Entry(updatedMenu).Property(m => m.Name).IsModified = true;
                                context.Entry(updatedMenu).Property(m => m.IdCategory).IsModified = true;
                                
                                // Save changes
                                await context.SaveChangesAsync();
                                
                                // Update the UI model
                                SelectedMenu.Name = dialog.Menu.Name;
                                SelectedMenu.IdCategory = dialog.Menu.IdCategory;
                                
                                // Refresh the menus list
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

                // Ask for confirmation
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

                        // Use direct database approach to ensure deletion
                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            // Find the menu by ID
                            var menu = await context.Menus.FindAsync(menuId);
                            if (menu != null)
                            {
                                // Remove from context and save changes
                                context.Menus.Remove(menu);
                                await context.SaveChangesAsync();
                                
                                // Remove from UI collection
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
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting menu: {ex.Message}";
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
                // Get the MainWindow as the owner for the dialog
                var mainWindow = System.Windows.Application.Current.MainWindow;
                
                // Create and show the dialog for adding a new allergen
                var dialog = new Views.AllergenDialog(mainWindow);
                bool? result = dialog.ShowDialog();
                
                if (result == true)
                {
                    // Add the new allergen to the database
                    await _allergenService.AddAsync(dialog.Allergen);
                    await _allergenService.SaveChangesAsync();
                    
                    // Add the new allergen to the collection
                    Allergens.Add(dialog.Allergen);
                    
                    // Select the newly added allergen
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
                
                // Store the original name and ID
                string originalName = SelectedAllergen.Name;
                int allergenId = SelectedAllergen.IdAllergen;
                
                // Get the MainWindow as the owner for the dialog
                var mainWindow = System.Windows.Application.Current.MainWindow;
                
                // Create a detached copy of the selected allergen to edit
                var allergenToEdit = new Allergen
                {
                    IdAllergen = allergenId,
                    Name = originalName
                };
                
                // Create and show the dialog for editing the allergen
                var dialog = new Views.AllergenDialog(mainWindow, allergenToEdit);
                bool? result = dialog.ShowDialog();
                
                if (result == true && dialog.Allergen.Name != originalName)
                {
                    try
                    {
                        // Use direct database approach to avoid entity tracking issues
                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            // Find the allergen by ID without tracking
                            var allergen = await context.Allergens
                                .AsNoTracking()
                                .FirstOrDefaultAsync(a => a.IdAllergen == allergenId);
                                
                            if (allergen != null)
                            {
                                // Create a new instance to update
                                var updatedAllergen = new Allergen
                                {
                                    IdAllergen = allergenId,
                                    Name = dialog.Allergen.Name
                                };
                                
                                // Attach with modified state
                                context.Allergens.Attach(updatedAllergen);
                                context.Entry(updatedAllergen).Property(a => a.Name).IsModified = true;
                                
                                // Save changes
                                await context.SaveChangesAsync();
                                
                                // Update the UI model
                                SelectedAllergen.Name = dialog.Allergen.Name;
                                
                                // Refresh the allergens list
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

                // Ask for confirmation
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

                        // Use direct database approach to ensure deletion
                        using (var context = new Data.RestaurantDbContext(
                            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Data.RestaurantDbContext>()
                                .UseSqlServer(_appSettingsService.ConnectionString)
                                .Options))
                        {
                            // Find the allergen by ID
                            var allergen = await context.Allergens.FindAsync(allergenId);
                            if (allergen != null)
                            {
                                // Remove from context and save changes
                                context.Allergens.Remove(allergen);
                                await context.SaveChangesAsync();
                                
                                // Remove from UI collection
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                // Cleanup
            }

            _isDisposed = true;
        }
    }
}