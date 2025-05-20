using System.Windows;
using OnlineRestaurant.Models;

namespace OnlineRestaurant.Views.Dialogs
{
    public partial class CategoryDialog : Window
    {
        private bool _isEditMode;

        public Category Category { get; private set; }
        public string DialogTitle => _isEditMode ? "Edit Category" : "Add Category";

        // Constructor for adding a new category
        public CategoryDialog(Window owner)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = false;
            Category = new Category();
            DataContext = this;
        }

        // Constructor for editing an existing category
        public CategoryDialog(Window owner, Category categoryToEdit)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = true;
            Category = categoryToEdit;
            DataContext = this;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(Category.Name))
            {
                MessageBox.Show("Category name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCategoryName.Focus();
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