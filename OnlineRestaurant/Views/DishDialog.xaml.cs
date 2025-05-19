using System;
using System.Collections.Generic;
using System.Windows;
using OnlineRestaurant.Models;

namespace OnlineRestaurant.Views
{
    public partial class DishDialog : Window
    {
        private bool _isEditMode;

        public Dish Dish { get; private set; }
        public List<Category> Categories { get; set; }
        public string DialogTitle => _isEditMode ? "Edit Dish" : "Add Dish";

        // Constructor for adding a new dish
        public DishDialog(Window owner, List<Category> categories)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = false;
            
            Categories = categories;
            Dish = new Dish();
            
            // Set a default category if available
            if (Categories.Count > 0)
            {
                Dish.IdCategory = Categories[0].IdCategory;
            }
            
            DataContext = this;
        }

        // Constructor for editing an existing dish
        public DishDialog(Window owner, Dish dishToEdit, List<Category> categories)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = true;
            
            Categories = categories;
            
            // Create a copy of the dish to edit
            Dish = new Dish
            {
                IdDish = dishToEdit.IdDish,
                Name = dishToEdit.Name,
                IdCategory = dishToEdit.IdCategory,
                Price = dishToEdit.Price,
                PortionSize = dishToEdit.PortionSize,
                TotalQuantity = dishToEdit.TotalQuantity
            };
            
            DataContext = this;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(Dish.Name))
            {
                MessageBox.Show("Dish name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDishName.Focus();
                return;
            }

            if (Dish.Price <= 0)
            {
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return;
            }

            if (Dish.PortionSize <= 0)
            {
                MessageBox.Show("Portion size must be greater than zero.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPortionSize.Focus();
                return;
            }

            if (Dish.TotalQuantity < 0)
            {
                MessageBox.Show("Total quantity cannot be negative.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTotalQuantity.Focus();
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
    }
} 