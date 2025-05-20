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

        // Eveniment pentru a notifica autentificarea reușită
        public event EventHandler LoginSuccessful;
        
        // Eveniment pentru a notifica cererea de navigare la ecranul de înregistrare
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
            
            // We don't need to load credentials here anymore since UserViewModel handles auto-login now
            // LoadSavedCredentials();
        }

        private void LoadSavedCredentials()
        {
            try
            {
                var savedCredentials = _credentialsService.LoadCredentials();
                if (savedCredentials != null)
                {
                    Email = savedCredentials.Email;
                    Password = savedCredentials.Password;
                    RememberMe = true;
                    
                    // Auto login if credentials are available
                    Login();
                }
            }
            catch (Exception ex)
            {
                // If there's an error loading credentials, just continue without auto-login
                System.Diagnostics.Debug.WriteLine($"Error loading credentials: {ex.Message}");
            }
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
                    
                    // Autentificare reușită
                    _userViewModel.CurrentUser = user;
                    
                    // Declanșăm evenimentul de succes
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    // Autentificare eșuată
                    ErrorMessage = "Email sau parolă incorecte. Vă rugăm să încercați din nou.";
                    
                    // Clear saved credentials if authentication failed
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
            // Declanșăm evenimentul pentru a notifica că utilizatorul dorește să navigheze la ecranul de înregistrare
            NavigateToRegisterRequest?.Invoke(this, EventArgs.Empty);
        }

        private void ClearErrorMessage()
        {
            ErrorMessage = string.Empty;
        }
    }
} 