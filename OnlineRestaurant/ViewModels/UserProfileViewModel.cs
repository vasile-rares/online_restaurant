using OnlineRestaurant.Commands;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using System;
using System.Windows.Input;

namespace OnlineRestaurant.ViewModels
{
    public class UserProfileViewModel : BaseVM
    {
        private readonly UserService _userService;
        private readonly UserViewModel _userViewModel;
        
        private string _lastName;
        private string _firstName;
        private string _email;
        private string _phone;
        private string _deliveryAddress;
        private string _role;
        private string _errorMessage = string.Empty;
        
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }
        
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }
        
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }
        
        public string DeliveryAddress
        {
            get => _deliveryAddress;
            set => SetProperty(ref _deliveryAddress, value);
        }
        
        public string Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        public ICommand LogoutCommand { get; }
        
        public UserProfileViewModel(UserService userService, UserViewModel userViewModel)
        {
            _userService = userService;
            _userViewModel = userViewModel;
            
            LogoutCommand = new RelayCommand(Logout);
            
            // Inițializează datele din UserViewModel
            UpdateProfileFromUser();
        }
        
        public void UpdateProfileFromUser()
        {
            if (_userViewModel.CurrentUser != null)
            {
                LastName = _userViewModel.CurrentUser.LastName;
                FirstName = _userViewModel.CurrentUser.FirstName;
                Email = _userViewModel.CurrentUser.Email;
                Phone = _userViewModel.CurrentUser.Phone ?? "";
                DeliveryAddress = _userViewModel.CurrentUser.DeliveryAddress ?? "";
                Role = _userViewModel.CurrentUser.Role;
            }
        }
        
        private void Logout()
        {
            _userViewModel.Logout();
        }
    }
} 