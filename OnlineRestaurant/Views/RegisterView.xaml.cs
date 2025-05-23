using OnlineRestaurant.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OnlineRestaurant.Views
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
            this.Loaded += RegisterView_Loaded;
        }

        private void RegisterView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel)
            {
                PasswordBox.Password = viewModel.Password;
                PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;

                ConfirmPasswordBox.Password = viewModel.ConfirmPassword;
                ConfirmPasswordBox.PasswordChanged += ConfirmPasswordBox_PasswordChanged;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel)
            {
                viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
            }
        }
    }
}