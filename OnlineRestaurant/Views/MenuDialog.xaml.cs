using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;

namespace OnlineRestaurant.Views
{
    public partial class MenuDialog : Window, INotifyPropertyChanged
    {
        private bool _isEditMode;
        private decimal _discountPercentage;
        private decimal _originalPrice;
        private decimal _finalPrice;
        private readonly AppSettingsService _appSettingsService;
        private List<Dish> _allDishes; // List to keep all dishes

        public OnlineRestaurant.Models.Menu Menu { get; private set; }
        public List<Category> Categories { get; set; }
        public string DialogTitle => _isEditMode ? "Edit Menu" : "Add Menu";
        public ObservableCollection<Dish> AvailableDishes { get; set; }
        public ObservableCollection<MenuDishViewModel> SelectedDishes { get; set; }

        public decimal DiscountPercentage
        {
            get => _discountPercentage;
            set
            {
                _discountPercentage = value;
                OnPropertyChanged();
            }
        }

        public decimal OriginalPrice
        {
            get => _originalPrice;
            set
            {
                _originalPrice = value;
                OnPropertyChanged();
                CalculateFinalPrice();
            }
        }

        public decimal FinalPrice
        {
            get => _finalPrice;
            set
            {
                _finalPrice = value;
                OnPropertyChanged();
            }
        }

        // Constructor for adding a new menu - simplified version for EmployeeViewModel
        public MenuDialog(Window owner, List<Category> categories)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = false;
            
            // Get configuration from App
            var config = App.Configuration;
            _appSettingsService = new AppSettingsService(config);
            
            // Default discount percentage
            DiscountPercentage = _appSettingsService.GetMenuDiscountPercentage();
            
            Categories = categories;
            Menu = new OnlineRestaurant.Models.Menu();
            
            // Set a default category if available
            if (Categories.Count > 0)
            {
                Menu.IdCategory = Categories[0].IdCategory;
            }
            
            // Initialize empty lists
            _allDishes = new List<Dish>();
            AvailableDishes = new ObservableCollection<Dish>();
            SelectedDishes = new ObservableCollection<MenuDishViewModel>();
            
            // Add an "All Categories" category to the category filter
            var tempCategories = new List<Category>(Categories);
            tempCategories.Insert(0, new Category { Name = "All Categories", IdCategory = -1 });
            cmbDishCategory.ItemsSource = tempCategories;
            cmbDishCategory.SelectedIndex = 0;
            
            DataContext = this;
        }

        // Constructor for editing an existing menu - simplified version for EmployeeViewModel
        public MenuDialog(Window owner, OnlineRestaurant.Models.Menu menuToEdit, List<Category> categories)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = true;
            
            // Get configuration from App
            var config = App.Configuration;
            _appSettingsService = new AppSettingsService(config);
            
            // Default discount percentage
            DiscountPercentage = _appSettingsService.GetMenuDiscountPercentage();
            
            Categories = categories;
            
            // Create a copy of the menu to edit
            Menu = new OnlineRestaurant.Models.Menu
            {
                IdMenu = menuToEdit.IdMenu,
                Name = menuToEdit.Name,
                IdCategory = menuToEdit.IdCategory
            };
            
            // Initialize empty lists
            _allDishes = new List<Dish>();
            AvailableDishes = new ObservableCollection<Dish>();
            SelectedDishes = new ObservableCollection<MenuDishViewModel>();
            
            // Add an "All Categories" category to the category filter
            var tempCategories = new List<Category>(Categories);
            tempCategories.Insert(0, new Category { Name = "All Categories", IdCategory = -1 });
            cmbDishCategory.ItemsSource = tempCategories;
            cmbDishCategory.SelectedIndex = 0;
            
            DataContext = this;
        }

        // Constructor for adding a new menu with complete data
        public MenuDialog(Window owner, List<Category> categories, List<Dish> dishes, AppSettingsService appSettingsService)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = false;
            _appSettingsService = appSettingsService;
            
            // Get discount percentage from settings
            DiscountPercentage = _appSettingsService.GetMenuDiscountPercentage();
            
            Categories = categories;
            Menu = new OnlineRestaurant.Models.Menu();
            
            // Set a default category if available
            if (Categories.Count > 0)
            {
                Menu.IdCategory = Categories[0].IdCategory;
            }
            
            // Store all dishes
            _allDishes = dishes;
            
            // Initialize collections
            AvailableDishes = new ObservableCollection<Dish>();
            SelectedDishes = new ObservableCollection<MenuDishViewModel>();
            
            // Add an "All Categories" category to the category filter
            var tempCategories = new List<Category>(Categories);
            tempCategories.Insert(0, new Category { Name = "All Categories", IdCategory = -1 });
            cmbDishCategory.ItemsSource = tempCategories;
            cmbDishCategory.SelectedIndex = 0;
            
            // Initialize available dishes with all dishes
            foreach (var dish in _allDishes)
            {
                AvailableDishes.Add(dish);
            }
            
            DataContext = this;
        }

        // Constructor for editing an existing menu with complete data
        public MenuDialog(Window owner, OnlineRestaurant.Models.Menu menuToEdit, List<Category> categories, List<Dish> dishes, List<MenuDish> menuDishes, AppSettingsService appSettingsService)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = true;
            _appSettingsService = appSettingsService;
            
            // Get discount percentage from settings
            DiscountPercentage = _appSettingsService.GetMenuDiscountPercentage();
            
            Categories = categories;
            
            // Create a copy of the menu to edit
            Menu = new OnlineRestaurant.Models.Menu
            {
                IdMenu = menuToEdit.IdMenu,
                Name = menuToEdit.Name,
                IdCategory = menuToEdit.IdCategory
            };
            
            // Store all dishes
            _allDishes = dishes;
            
            // Initialize collections
            AvailableDishes = new ObservableCollection<Dish>();
            SelectedDishes = new ObservableCollection<MenuDishViewModel>();
            
            // Add an "All Categories" category to the category filter
            var tempCategories = new List<Category>(Categories);
            tempCategories.Insert(0, new Category { Name = "All Categories", IdCategory = -1 });
            cmbDishCategory.ItemsSource = tempCategories;
            cmbDishCategory.SelectedIndex = 0;
            
            // Add existing menu dishes
            foreach (var menuDish in menuDishes.Where(md => md.IdMenu == menuToEdit.IdMenu))
            {
                var dish = dishes.FirstOrDefault(d => d.IdDish == menuDish.IdDish);
                if (dish != null)
                {
                    SelectedDishes.Add(new MenuDishViewModel
                    {
                        Dish = dish,
                        Quantity = menuDish.Quantity
                    });
                }
            }
            
            // Initialize available dishes with dishes not in the menu
            foreach (var dish in _allDishes)
            {
                if (!SelectedDishes.Any(sd => sd.Dish.IdDish == dish.IdDish))
                {
                    AvailableDishes.Add(dish);
                }
            }
            
            // Calculate initial price
            RecalculatePrice();
            
            DataContext = this;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(Menu.Name))
            {
                MessageBox.Show("Menu name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMenuName.Focus();
                return;
            }

            if (SelectedDishes.Count == 0)
            {
                MessageBox.Show("Please add at least one dish to the menu.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CmbDishCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDishCategory.SelectedItem is Category selectedCategory)
            {
                FilterAvailableDishes(selectedCategory.IdCategory);
            }
            else if (cmbDishCategory.SelectedIndex == 0)
            {
                // If the first item ("All Categories") is selected, show all dishes
                AvailableDishes.Clear();
                foreach (var dish in _allDishes)
                {
                    // Skip dishes that are already selected
                    if (!SelectedDishes.Any(sd => sd.Dish.IdDish == dish.IdDish))
                    {
                        AvailableDishes.Add(dish);
                    }
                }
            }
        }

        private void FilterAvailableDishes(int categoryId)
        {
            // Clear current available dishes
            AvailableDishes.Clear();
            
            // Get dishes of the selected category
            var dishesInCategory = _allDishes.Where(d => d.IdCategory == categoryId);
            
            // Add dishes that are not already selected
            foreach (var dish in dishesInCategory)
            {
                if (!SelectedDishes.Any(sd => sd.Dish.IdDish == dish.IdDish))
                {
                    AvailableDishes.Add(dish);
                }
            }
        }

        private void BtnAddDish_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var dish = (Dish)button.DataContext;
            
            // Check if dish is already in the selected dishes
            if (SelectedDishes.Any(sd => sd.Dish.IdDish == dish.IdDish))
            {
                MessageBox.Show("This dish is already added to the menu.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // Add to selected dishes
            var menuDish = new MenuDishViewModel
            {
                Dish = dish,
                Quantity = 1 // Default quantity
            };
            
            SelectedDishes.Add(menuDish);
            
            // Remove from available dishes
            var dishToRemove = AvailableDishes.FirstOrDefault(d => d.IdDish == dish.IdDish);
            if (dishToRemove != null)
            {
                AvailableDishes.Remove(dishToRemove);
            }
            
            // Recalculate price
            RecalculatePrice();
        }

        private void BtnRemoveDish_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var menuDish = (MenuDishViewModel)button.DataContext;
            
            // Remove from selected dishes
            SelectedDishes.Remove(menuDish);
            
            // Add back to available dishes if it matches the current category filter
            Category selectedCategory = cmbDishCategory.SelectedItem as Category;
            
            if (selectedCategory == null || 
                selectedCategory.IdCategory == -1 || // "All Categories"
                menuDish.Dish.IdCategory == selectedCategory.IdCategory)
            {
                AvailableDishes.Add(menuDish.Dish);
            }
            
            // Recalculate price
            RecalculatePrice();
        }
        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        
        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var menuDish = (MenuDishViewModel)textBox.DataContext;
            
            // If textbox is empty, default quantity to 1
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                menuDish.Quantity = 1;
                textBox.Text = "1";
            }
            
            // Ensure quantity is at least 1
            int quantity;
            if (!int.TryParse(textBox.Text, out quantity) || quantity < 1)
            {
                menuDish.Quantity = 1;
                textBox.Text = "1";
            }
            
            // Recalculate price
            RecalculatePrice();
        }
        
        private void RecalculatePrice()
        {
            decimal total = 0;
            
            foreach (var menuDish in SelectedDishes)
            {
                menuDish.CalculateTotalPrice();
                total += menuDish.TotalPrice;
            }
            
            OriginalPrice = total;
        }
        
        private void CalculateFinalPrice()
        {
            FinalPrice = OriginalPrice * (1 - (DiscountPercentage / 100));
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MenuDishViewModel : INotifyPropertyChanged
    {
        private Dish _dish;
        private int _quantity;
        private decimal _totalPrice;

        public Dish Dish
        {
            get => _dish;
            set
            {
                _dish = value;
                OnPropertyChanged();
                CalculateTotalPrice();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                CalculateTotalPrice();
            }
        }

        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                _totalPrice = value;
                OnPropertyChanged();
            }
        }

        public void CalculateTotalPrice()
        {
            TotalPrice = Dish?.Price * Quantity ?? 0;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 