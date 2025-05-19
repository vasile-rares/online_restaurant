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
        private readonly OrderService _orderService;
        private readonly DishService _dishService;
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
            OrderService orderService,
            DishService dishService,
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
                ((RelayCommand)UpdateOrderStatusCommand).RaiseCanExecuteChanged();
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
                
                var orders = await _orderService.GetActiveOrdersAsync();
                ActiveOrders.Clear();
                foreach (var order in orders.OrderByDescending(o => o.OrderDate))
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
                
                // Update the order status
                SelectedOrder.Status = newStatus;
                await _orderService.UpdateAsync(SelectedOrder);
                
                // Refresh the active orders list
                await LoadActiveOrdersAsync();
                await LoadAllOrdersAsync();
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
        private void AddCategory() { /* Implementation would show dialog/form for adding a category */ }
        private void EditCategory() { /* Implementation would show dialog/form for editing the selected category */ }
        private async Task DeleteCategoryAsync()
        {
            try
            {
                if (SelectedCategory == null) return;
                
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                await _categoryService.DeleteAsync(SelectedCategory.IdCategory);
                Categories.Remove(SelectedCategory);
                SelectedCategory = null;
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
        
        private void AddDish() { /* Implementation would show dialog/form for adding a dish */ }
        private void EditDish() { /* Implementation would show dialog/form for editing the selected dish */ }
        private async Task DeleteDishAsync()
        {
            try
            {
                if (SelectedDish == null) return;
                
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                await _dishService.DeleteAsync(SelectedDish.IdDish);
                // Remove from any collections that contain this dish
                var dishToRemove = LowStockDishes.FirstOrDefault(d => d.IdDish == SelectedDish.IdDish);
                if (dishToRemove != null)
                    LowStockDishes.Remove(dishToRemove);
                
                SelectedDish = null;
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
        
        private void AddMenu() { /* Implementation would show dialog/form for adding a menu */ }
        private void EditMenu() { /* Implementation would show dialog/form for editing the selected menu */ }
        private async Task DeleteMenuAsync()
        {
            try
            {
                if (SelectedMenu == null) return;
                
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                await _menuService.DeleteAsync(SelectedMenu.IdMenu);
                Menus.Remove(SelectedMenu);
                SelectedMenu = null;
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
        
        private void AddAllergen() { /* Implementation would show dialog/form for adding an allergen */ }
        private void EditAllergen() { /* Implementation would show dialog/form for editing the selected allergen */ }
        private async Task DeleteAllergenAsync()
        {
            try
            {
                if (SelectedAllergen == null) return;
                
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                await _allergenService.DeleteAsync(SelectedAllergen.IdAllergen);
                Allergens.Remove(SelectedAllergen);
                SelectedAllergen = null;
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