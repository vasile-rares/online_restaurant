using OnlineRestaurant.Commands;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using System;
using System.Windows.Input;

namespace OnlineRestaurant.ViewModels
{
    public class UserProfileViewModel : BaseVM
    {
        private readonly UtilizatorService _utilizatorService;
        private readonly UserViewModel _userViewModel;
        
        private string _nume;
        private string _prenume;
        private string _email;
        private string _telefon;
        private string _adresaLivrare;
        private string _rol;
        private string _errorMessage = string.Empty;
        
        public string Nume
        {
            get => _nume;
            set => SetProperty(ref _nume, value);
        }
        
        public string Prenume
        {
            get => _prenume;
            set => SetProperty(ref _prenume, value);
        }
        
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        
        public string Telefon
        {
            get => _telefon;
            set => SetProperty(ref _telefon, value);
        }
        
        public string AdresaLivrare
        {
            get => _adresaLivrare;
            set => SetProperty(ref _adresaLivrare, value);
        }
        
        public string Rol
        {
            get => _rol;
            set => SetProperty(ref _rol, value);
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        public ICommand LogoutCommand { get; }
        
        public UserProfileViewModel(UtilizatorService utilizatorService, UserViewModel userViewModel)
        {
            _utilizatorService = utilizatorService;
            _userViewModel = userViewModel;
            
            LogoutCommand = new RelayCommand(Logout);
            
            // Inițializează datele din UserViewModel
            UpdateProfileFromUser();
        }
        
        public void UpdateProfileFromUser()
        {
            if (_userViewModel.CurrentUser != null)
            {
                Nume = _userViewModel.CurrentUser.Nume;
                Prenume = _userViewModel.CurrentUser.Prenume;
                Email = _userViewModel.CurrentUser.Email;
                Telefon = _userViewModel.CurrentUser.Telefon ?? "";
                AdresaLivrare = _userViewModel.CurrentUser.AdresaLivrare ?? "";
                Rol = _userViewModel.CurrentUser.Rol;
            }
        }
        
        private void Logout()
        {
            _userViewModel.Logout();
        }
    }
} 