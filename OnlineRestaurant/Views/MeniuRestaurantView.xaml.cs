using OnlineRestaurant.ViewModels;
using System.Windows.Controls;

namespace OnlineRestaurant.Views
{
    /// <summary>
    /// Interaction logic for MeniuRestaurantView.xaml
    /// </summary>
    public partial class MeniuRestaurantView : UserControl
    {
        public MeniuRestaurantView()
        {
            InitializeComponent();
        }

        public MeniuRestaurantView(MeniuRestaurantViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
} 