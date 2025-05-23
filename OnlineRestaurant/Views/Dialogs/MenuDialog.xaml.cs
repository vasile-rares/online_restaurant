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

namespace OnlineRestaurant.Views.Dialogs
{
    public partial class MenuDialog : Window, INotifyPropertyChanged
    {
        private bool _isEditMode;
        private decimal _discountPercentage;
        private decimal _originalPrice;
        private decimal _finalPrice;
        private readonly AppSettingsService _appSettingsService;
        private List<Dish> _allDishes;

        public OnlineRestaurant.Models.Menu Menu { get; private set; }
        public List<Category> Categories { get; set; }
        public string DialogTitle => _isEditMode ? "Editare Meniu" : "Adăugare Meniu";
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

        public MenuDialog(Window owner, List<Category> categories)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = false;

            var config = App.Configuration;
            _appSettingsService = new AppSettingsService(config);

            DiscountPercentage = _appSettingsService.GetMenuDiscountPercentage();

            Categories = categories;
            Menu = new OnlineRestaurant.Models.Menu();

            if (Categories.Count > 0)
            {
                Menu.IdCategory = Categories[0].IdCategory;
            }

            _allDishes = new List<Dish>();
            AvailableDishes = new ObservableCollection<Dish>();
            SelectedDishes = new ObservableCollection<MenuDishViewModel>();

            var tempCategories = new List<Category>(Categories);
            tempCategories.Insert(0, new Category { Name = "All Categories", IdCategory = -1 });
            cmbDishCategory.ItemsSource = tempCategories;
            cmbDishCategory.SelectedIndex = 0;

            DataContext = this;
        }

        public MenuDialog(Window owner, OnlineRestaurant.Models.Menu menuToEdit, List<Category> categories)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = true;

            var config = App.Configuration;
            _appSettingsService = new AppSettingsService(config);

            DiscountPercentage = _appSettingsService.GetMenuDiscountPercentage();

            Categories = categories;

            Menu = new OnlineRestaurant.Models.Menu
            {
                IdMenu = menuToEdit.IdMenu,
                Name = menuToEdit.Name,
                IdCategory = menuToEdit.IdCategory
            };

            _allDishes = new List<Dish>();
            AvailableDishes = new ObservableCollection<Dish>();
            SelectedDishes = new ObservableCollection<MenuDishViewModel>();

            var tempCategories = new List<Category>(Categories);
            tempCategories.Insert(0, new Category { Name = "All Categories", IdCategory = -1 });
            cmbDishCategory.ItemsSource = tempCategories;
            cmbDishCategory.SelectedIndex = 0;

            DataContext = this;
        }

        public MenuDialog(Window owner, List<Category> categories, List<Dish> dishes, AppSettingsService appSettingsService)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = false;
            _appSettingsService = appSettingsService;

            DiscountPercentage = _appSettingsService.GetMenuDiscountPercentage();

            Categories = categories;
            Menu = new OnlineRestaurant.Models.Menu();

            if (Categories.Count > 0)
            {
                Menu.IdCategory = Categories[0].IdCategory;
            }

            _allDishes = dishes;

            AvailableDishes = new ObservableCollection<Dish>();
            SelectedDishes = new ObservableCollection<MenuDishViewModel>();

            var tempCategories = new List<Category>(Categories);
            tempCategories.Insert(0, new Category { Name = "All Categories", IdCategory = -1 });
            cmbDishCategory.ItemsSource = tempCategories;
            cmbDishCategory.SelectedIndex = 0;

            foreach (var dish in _allDishes)
            {
                AvailableDishes.Add(dish);
            }

            DataContext = this;
        }

        public MenuDialog(Window owner, OnlineRestaurant.Models.Menu menuToEdit, List<Category> categories, List<Dish> dishes, List<MenuDish> menuDishes, AppSettingsService appSettingsService)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = true;
            _appSettingsService = appSettingsService;

            DiscountPercentage = _appSettingsService.GetMenuDiscountPercentage();

            Categories = categories;

            Menu = new OnlineRestaurant.Models.Menu
            {
                IdMenu = menuToEdit.IdMenu,
                Name = menuToEdit.Name,
                IdCategory = menuToEdit.IdCategory
            };

            _allDishes = dishes;

            AvailableDishes = new ObservableCollection<Dish>();
            SelectedDishes = new ObservableCollection<MenuDishViewModel>();

            var tempCategories = new List<Category>(Categories);
            tempCategories.Insert(0, new Category { Name = "All Categories", IdCategory = -1 });
            cmbDishCategory.ItemsSource = tempCategories;
            cmbDishCategory.SelectedIndex = 0;

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

            foreach (var dish in _allDishes)
            {
                if (!SelectedDishes.Any(sd => sd.Dish.IdDish == dish.IdDish))
                {
                    AvailableDishes.Add(dish);
                }
            }

            RecalculatePrice();

            DataContext = this;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Menu.Name))
            {
                MessageBox.Show("Numele meniului nu poate fi gol.", "Eroare de validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMenuName.Focus();
                return;
            }

            if (SelectedDishes.Count == 0)
            {
                MessageBox.Show("Vă rugăm să adăugați cel puțin un preparat în meniu.", "Eroare de validare", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                AvailableDishes.Clear();
                foreach (var dish in _allDishes)
                {
                    if (!SelectedDishes.Any(sd => sd.Dish.IdDish == dish.IdDish))
                    {
                        AvailableDishes.Add(dish);
                    }
                }
            }
        }

        private void FilterAvailableDishes(int categoryId)
        {
            AvailableDishes.Clear();

            var dishesInCategory = _allDishes.Where(d => d.IdCategory == categoryId);

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

            if (SelectedDishes.Any(sd => sd.Dish.IdDish == dish.IdDish))
            {
                MessageBox.Show("Acest preparat este deja adăugat în meniu.", "Informație", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var menuDish = new MenuDishViewModel
            {
                Dish = dish,
                Quantity = 1
            };

            SelectedDishes.Add(menuDish);

            var dishToRemove = AvailableDishes.FirstOrDefault(d => d.IdDish == dish.IdDish);
            if (dishToRemove != null)
            {
                AvailableDishes.Remove(dishToRemove);
            }

            RecalculatePrice();
        }

        private void BtnRemoveDish_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var menuDish = (MenuDishViewModel)button.DataContext;

            SelectedDishes.Remove(menuDish);

            Category selectedCategory = cmbDishCategory.SelectedItem as Category;

            if (selectedCategory == null ||
                selectedCategory.IdCategory == -1 ||
                menuDish.Dish.IdCategory == selectedCategory.IdCategory)
            {
                AvailableDishes.Add(menuDish.Dish);
            }

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

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                menuDish.Quantity = 1;
                textBox.Text = "1";
            }

            int quantity;
            if (!int.TryParse(textBox.Text, out quantity) || quantity < 1)
            {
                menuDish.Quantity = 1;
                textBox.Text = "1";
            }

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
            TotalPrice = Dish?.Price ?? 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}