using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using OnlineRestaurant.Models;

namespace OnlineRestaurant.Views
{
    public partial class DishDialog : Window, INotifyPropertyChanged
    {
        private bool _isEditMode;
        private string _selectedPhotoUrl;
        private bool _hasSelectedPhoto;
        private string _tempPhotoPath;

        public Dish Dish { get; private set; }
        public List<Category> Categories { get; set; }
        public string DialogTitle => _isEditMode ? "Edit Dish" : "Add Dish";
        
        public string SelectedPhotoUrl
        {
            get => _selectedPhotoUrl;
            set
            {
                _selectedPhotoUrl = value;
                OnPropertyChanged();
                HasSelectedPhoto = !string.IsNullOrEmpty(value);
            }
        }
        
        public bool HasSelectedPhoto
        {
            get => _hasSelectedPhoto;
            set
            {
                _hasSelectedPhoto = value;
                OnPropertyChanged();
            }
        }

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
            
            // Initialize photo properties
            SelectedPhotoUrl = null;
            HasSelectedPhoto = false;
            
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
                TotalQuantity = dishToEdit.TotalQuantity,
                Photos = new List<DishPhoto>() // Initialize Photos collection
            };
            
            // Load dish photo if available
            if (dishToEdit.Photos != null && dishToEdit.Photos.Any())
            {
                SelectedPhotoUrl = dishToEdit.Photos.First().Url;
                HasSelectedPhoto = true;
            }
            else
            {
                SelectedPhotoUrl = null;
                HasSelectedPhoto = false;
            }
            
            DataContext = this;
        }

        private void BtnSelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Image",
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif, *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Multiselect = false,
                // Set initial directory to Images/Dishes folder if it exists
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Dishes")
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Get the selected file path
                    string selectedFilePath = openFileDialog.FileName;

                    // Extract just the filename
                    string fileName = Path.GetFileName(selectedFilePath);

                    // Format the path in the specified format
                    string formattedPath = $"/Images/Dishes/{fileName}";

                    // Update the UI with the formatted path
                    SelectedPhotoUrl = formattedPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing image path: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnRemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            // Clear the selected photo - just remove the reference, don't delete the file
            SelectedPhotoUrl = null;
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

            // Clear existing photos before adding the new one
            if (Dish.Photos == null)
            {
                Dish.Photos = new List<DishPhoto>();
            }
            else
            {
                Dish.Photos.Clear();
            }
            
            // If a photo is selected, add it to the dish
            if (HasSelectedPhoto && !string.IsNullOrEmpty(SelectedPhotoUrl))
            {
                // Add the photo to the dish
                Dish.Photos.Add(new DishPhoto
                {
                    IdDish = Dish.IdDish, // Set the dish ID for new photos
                    Url = SelectedPhotoUrl
                });
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 