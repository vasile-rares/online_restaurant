using OnlineRestaurant.ViewModels;
using System.Windows.Controls;

namespace OnlineRestaurant.Views
{
    /// <summary>
    /// Interaction logic for MenuRestaurantView.xaml
    /// </summary>
    public partial class MenuRestaurantView : UserControl
    {
        public MenuRestaurantView()
        {
            InitializeComponent();
        }

        public MenuRestaurantView(MenuRestaurantViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
} 