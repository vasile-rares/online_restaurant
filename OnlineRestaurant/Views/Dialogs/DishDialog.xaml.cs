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

namespace OnlineRestaurant.Views.Dialogs
{
    // Helper class for allergen selection in the UI
    public class AllergenViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int IdAllergen { get; set; }
        public string Name { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class DishDialog : Window, INotifyPropertyChanged
    {
        private bool _isEditMode;
        private string _selectedPhotoUrl;
        private bool _hasSelectedPhoto;
        private string _tempPhotoPath;
        private List<AllergenViewModel> _allergens;

        public Dish Dish { get; private set; }
        public List<Category> Categories { get; set; }

        public List<AllergenViewModel> Allergens
        {
            get => _allergens;
            set
            {
                _allergens = value;
                OnPropertyChanged();
            }
        }

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

        public DishDialog(Window owner, List<Category> categories, List<Allergen> allergens)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = false;

            Categories = categories;
            Dish = new Dish();

            if (Categories.Count > 0)
            {
                Dish.IdCategory = Categories[0].IdCategory;
            }

            InitializeAllergens(allergens, new List<DishAllergen>());

            SelectedPhotoUrl = null;
            HasSelectedPhoto = false;

            DataContext = this;
        }

        public DishDialog(Window owner, Dish dishToEdit, List<Category> categories, List<Allergen> allergens)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = true;

            Categories = categories;

            Dish = new Dish
            {
                IdDish = dishToEdit.IdDish,
                Name = dishToEdit.Name,
                IdCategory = dishToEdit.IdCategory,
                Price = dishToEdit.Price,
                PortionSize = dishToEdit.PortionSize,
                TotalQuantity = dishToEdit.TotalQuantity,
                Photos = new List<DishPhoto>()
            };

            InitializeAllergens(allergens, dishToEdit.DishAllergens?.ToList() ?? new List<DishAllergen>());

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

        private void InitializeAllergens(List<Allergen> allergens, List<DishAllergen> dishAllergens)
        {
            Allergens = allergens.Select(a => new AllergenViewModel
            {
                IdAllergen = a.IdAllergen,
                Name = a.Name,
                IsSelected = dishAllergens.Any(da => da.IdAllergen == a.IdAllergen)
            }).ToList();
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
                    string selectedFilePath = openFileDialog.FileName;
                    string fileName = Path.GetFileName(selectedFilePath);
                    string formattedPath = $"/Images/Dishes/{fileName}";

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
            SelectedPhotoUrl = null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Dish.Name))
            {
                MessageBox.Show("Numele preparatului nu poate fi gol.", "Eroare de validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDishName.Focus();
                return;
            }

            if (Dish.Price <= 0)
            {
                MessageBox.Show("Vă rugăm să introduceți un preț valid.", "Eroare de validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return;
            }

            if (Dish.PortionSize <= 0)
            {
                MessageBox.Show("Mărimea porției trebuie să fie mai mare decât zero.", "Eroare de validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPortionSize.Focus();
                return;
            }

            if (Dish.TotalQuantity < 0)
            {
                MessageBox.Show("Cantitatea totală nu poate fi negativă.", "Eroare de validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTotalQuantity.Focus();
                return;
            }

            if (Dish.Photos == null)
            {
                Dish.Photos = new List<DishPhoto>();
            }
            else
            {
                Dish.Photos.Clear();
            }

            if (HasSelectedPhoto && !string.IsNullOrEmpty(SelectedPhotoUrl))
            {
                Dish.Photos.Add(new DishPhoto
                {
                    IdDish = Dish.IdDish,
                    Url = SelectedPhotoUrl
                });
            }

            if (Dish.DishAllergens == null)
            {
                Dish.DishAllergens = new List<DishAllergen>();
            }
            else
            {
                Dish.DishAllergens.Clear();
            }

            foreach (var allergen in Allergens.Where(a => a.IsSelected))
            {
                Dish.DishAllergens.Add(new DishAllergen
                {
                    IdDish = Dish.IdDish,
                    IdAllergen = allergen.IdAllergen
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}