using OnlineRestaurant.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OnlineRestaurant.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            this.Loaded += LoginView_Loaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                PasswordBox.Password = viewModel.Password;
                PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }
    }
}