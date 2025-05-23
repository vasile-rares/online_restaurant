using OnlineRestaurant.Commands;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace OnlineRestaurant.ViewModels
{
    public class LoginViewModel : BaseVM
    {
        private readonly UserService _userService;
        private readonly UserViewModel _userViewModel;
        private readonly UserCredentialsService _credentialsService;

        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private bool _rememberMe;

        public event EventHandler LoginSuccessful;

        public event EventHandler NavigateToRegisterRequest;

        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
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

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

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

        public ICommand LoginCommand { get; }
        public ICommand CreateAccountCommand { get; }

        public LoginViewModel(UserService userService, UserViewModel userViewModel, UserCredentialsService credentialsService)
        {
            _userService = userService;
            _userViewModel = userViewModel;
            _credentialsService = credentialsService;

            LoginCommand = new RelayCommand(Login, CanLogin);
            CreateAccountCommand = new RelayCommand(NavigateToCreateAccount);
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !IsLoading;
        }

        private async void Login()
        {
            try
            {
                IsLoading = true;
                ClearErrorMessage();

                var user = await _userService.Authenticate(Email, Password);

                if (user != null)
                {
                    // Save credentials if RememberMe is checked
                    _credentialsService.SaveCredentials(Email, Password, RememberMe);

                    System.Diagnostics.Debug.WriteLine("Login successful, setting CurrentUser");
                    _userViewModel.CurrentUser = user;

                    _userViewModel.ForceNotifyLoginStatus();

                    System.Diagnostics.Debug.WriteLine("Triggering LoginSuccessful event");
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = "Email sau parolă incorecte. Vă rugăm să încercați din nou.";

                    if (RememberMe == false)
                    {
                        _credentialsService.ClearCredentials();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la autentificare: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NavigateToCreateAccount()
        {
            NavigateToRegisterRequest?.Invoke(this, EventArgs.Empty);
        }

        private void ClearErrorMessage()
        {
            ErrorMessage = string.Empty;
        }
    }
}