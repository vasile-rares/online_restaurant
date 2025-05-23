using OnlineRestaurant.Commands;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OnlineRestaurant.ViewModels
{
    public class RegisterViewModel : BaseVM
    {
        private readonly UserService _userService;
        private readonly UserViewModel _userViewModel;

        private string _lastName = string.Empty;
        private string _firstName = string.Empty;
        private string _email = string.Empty;
        private string _phone = string.Empty;
        private string _deliveryAddress = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _selectedRole = "Client";
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        // Eveniment pentru a notifica înregistrarea reușită
        public event EventHandler RegisterSuccessful;

        public string LastName
        {
            get => _lastName;
            set
            {
                SetProperty(ref _lastName, value);
                ClearErrorMessage();
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                SetProperty(ref _firstName, value);
                ClearErrorMessage();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                ClearErrorMessage();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                SetProperty(ref _phone, value);
                ClearErrorMessage();
            }
        }

        public string DeliveryAddress
        {
            get => _deliveryAddress;
            set
            {
                SetProperty(ref _deliveryAddress, value);
                ClearErrorMessage();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ClearErrorMessage();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                ClearErrorMessage();
            }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        public ObservableCollection<string> AvailableRoles { get; } = new ObservableCollection<string> { "Client", "Angajat" };

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        public RegisterViewModel(UserService userService, UserViewModel userViewModel)
        {
            _userService = userService;
            _userViewModel = userViewModel;

            RegisterCommand = new RelayCommand(Register, CanRegister);
            CancelCommand = new RelayCommand(NavigateBack);
        }

        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   !IsLoading;
        }

        private async void Register()
        {
            if (!Validate())
                return;

            try
            {
                IsLoading = true;
                ClearErrorMessage();

                var user = new User
                {
                    LastName = LastName,
                    FirstName = FirstName,
                    Email = Email,
                    Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone,
                    DeliveryAddress = string.IsNullOrWhiteSpace(DeliveryAddress) ? null : DeliveryAddress,
                    Role = SelectedRole
                };

                var createdUser = await _userService.Register(user, Password);

                if (createdUser != null)
                {
                    _userViewModel.CurrentUser = createdUser;

                    RegisterSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = "Acest email este deja folosit. Vă rugăm să folosiți alt email.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la înregistrare: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool Validate()
        {
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Parolele nu coincid. Vă rugăm să verificați.";
                return false;
            }

            // Validare email
            if (!Email.Contains("@") || !Email.Contains("."))
            {
                ErrorMessage = "Vă rugăm să introduceți o adresă de email validă.";
                return false;
            }

            return true;
        }

        private void NavigateBack() // Implementat în MainViewModel prin eveniment
        {
        }

        private void ClearErrorMessage()
        {
            ErrorMessage = string.Empty;
        }
    }
}