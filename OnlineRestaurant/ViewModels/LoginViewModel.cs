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
        private readonly UtilizatorService _utilizatorService;
        private readonly UserViewModel _userViewModel;
        
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        // Eveniment pentru a notifica autentificarea reușită
        public event EventHandler LoginSuccessful;

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

        public LoginViewModel(UtilizatorService utilizatorService, UserViewModel userViewModel)
        {
            _utilizatorService = utilizatorService;
            _userViewModel = userViewModel;
            
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
                
                var user = await _utilizatorService.Autentificare(Email, Password);
                
                if (user != null)
                {
                    // Autentificare reușită
                    _userViewModel.CurrentUser = user;
                    ClearFields();
                    
                    // Declanșăm evenimentul de succes
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    // Autentificare eșuată
                    ErrorMessage = "Email sau parolă incorecte. Vă rugăm să încercați din nou.";
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
            // Va fi implementat în MainViewModel prin eveniment
        }

        private void ClearErrorMessage()
        {
            ErrorMessage = string.Empty;
        }

        private void ClearFields()
        {
            Email = string.Empty;
            Password = string.Empty;
        }
    }
} 