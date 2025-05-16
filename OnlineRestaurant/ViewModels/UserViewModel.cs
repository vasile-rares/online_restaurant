using OnlineRestaurant.Commands;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using System;
using System.Windows.Input;

namespace OnlineRestaurant.ViewModels
{
    public class UserViewModel : BaseVM
    {
        private readonly UserService _userService;
        private User _currentUser;
        private bool _isLoggedIn;
        private string _displayName = "Guest";

        // Eveniment pentru a notifica delogarea
        public event EventHandler LogoutEvent;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (SetProperty(ref _currentUser, value))
                {
                    IsLoggedIn = value != null;
                    DisplayName = value != null ? value.FullName : "Guest";
                }
            }
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        public ICommand LogoutCommand { get; }

        public UserViewModel(UserService userService)
        {
            _userService = userService;
            LogoutCommand = new RelayCommand(Logout);
        }

        public void Logout()
        {
            CurrentUser = null;
            IsLoggedIn = false;
            DisplayName = "Guest";
            
            // Declanșăm evenimentul de delogare
            LogoutEvent?.Invoke(this, EventArgs.Empty);
        }
    }
} 