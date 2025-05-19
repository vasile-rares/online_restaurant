using System.Windows;
using OnlineRestaurant.Models;

namespace OnlineRestaurant.Views
{
    public partial class AllergenDialog : Window
    {
        private bool _isEditMode;

        public Allergen Allergen { get; private set; }
        public string DialogTitle => _isEditMode ? "Edit Allergen" : "Add Allergen";

        // Constructor for adding a new allergen
        public AllergenDialog(Window owner)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = false;
            Allergen = new Allergen();
            DataContext = this;
        }

        // Constructor for editing an existing allergen
        public AllergenDialog(Window owner, Allergen allergenToEdit)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = true;
            
            // Create a copy of the allergen to edit
            Allergen = new Allergen
            {
                IdAllergen = allergenToEdit.IdAllergen,
                Name = allergenToEdit.Name
            };
            
            DataContext = this;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(Allergen.Name))
            {
                MessageBox.Show("Allergen name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAllergenName.Focus();
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