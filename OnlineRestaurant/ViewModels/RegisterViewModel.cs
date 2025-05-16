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
        private readonly UtilizatorService _utilizatorService;
        private readonly UserViewModel _userViewModel;
        
        private string _nume = string.Empty;
        private string _prenume = string.Empty;
        private string _email = string.Empty;
        private string _telefon = string.Empty;
        private string _adresaLivrare = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _selectedRole = "Client";
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        // Eveniment pentru a notifica înregistrarea reușită
        public event EventHandler RegisterSuccessful;

        public string Nume
        {
            get => _nume;
            set
            {
                SetProperty(ref _nume, value);
                ClearErrorMessage();
            }
        }

        public string Prenume
        {
            get => _prenume;
            set
            {
                SetProperty(ref _prenume, value);
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

        public string Telefon
        {
            get => _telefon;
            set
            {
                SetProperty(ref _telefon, value);
                ClearErrorMessage();
            }
        }

        public string AdresaLivrare
        {
            get => _adresaLivrare;
            set
            {
                SetProperty(ref _adresaLivrare, value);
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

        public RegisterViewModel(UtilizatorService utilizatorService, UserViewModel userViewModel)
        {
            _utilizatorService = utilizatorService;
            _userViewModel = userViewModel;
            
            RegisterCommand = new RelayCommand(Register, CanRegister);
            CancelCommand = new RelayCommand(NavigateBack);
        }

        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Nume) && 
                   !string.IsNullOrWhiteSpace(Prenume) && 
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

                var utilizator = new Utilizator
                {
                    Nume = Nume,
                    Prenume = Prenume,
                    Email = Email,
                    Telefon = string.IsNullOrWhiteSpace(Telefon) ? null : Telefon,
                    AdresaLivrare = string.IsNullOrWhiteSpace(AdresaLivrare) ? null : AdresaLivrare,
                    Rol = SelectedRole
                };

                var createdUser = await _utilizatorService.Inregistrare(utilizator, Password);
                
                if (createdUser != null)
                {
                    // Înregistrare reușită și logare automată
                    _userViewModel.CurrentUser = createdUser;
                    
                    // Declanșăm evenimentul de succes
                    RegisterSuccessful?.Invoke(this, EventArgs.Empty);
                    
                    NavigateBack();
                }
                else
                {
                    // Înregistrare eșuată
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

        private void NavigateBack()
        {
            // Va fi implementat în MainViewModel prin eveniment
        }

        private void ClearErrorMessage()
        {
            ErrorMessage = string.Empty;
        }
    }
} 