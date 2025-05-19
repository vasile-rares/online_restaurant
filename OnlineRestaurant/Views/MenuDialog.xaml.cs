using System.Collections.Generic;
using System.Windows;
using OnlineRestaurant.Models;

namespace OnlineRestaurant.Views
{
    public partial class MenuDialog : Window
    {
        private bool _isEditMode;

        public Menu Menu { get; private set; }
        public List<Category> Categories { get; set; }
        public string DialogTitle => _isEditMode ? "Edit Menu" : "Add Menu";

        // Constructor for adding a new menu
        public MenuDialog(Window owner, List<Category> categories)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = false;
            
            Categories = categories;
            Menu = new Menu();
            
            // Set a default category if available
            if (Categories.Count > 0)
            {
                Menu.IdCategory = Categories[0].IdCategory;
            }
            
            DataContext = this;
        }

        // Constructor for editing an existing menu
        public MenuDialog(Window owner, Menu menuToEdit, List<Category> categories)
        {
            InitializeComponent();
            Owner = owner;
            _isEditMode = true;
            
            Categories = categories;
            
            // Create a copy of the menu to edit
            Menu = new Menu
            {
                IdMenu = menuToEdit.IdMenu,
                Name = menuToEdit.Name,
                IdCategory = menuToEdit.IdCategory
            };
            
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