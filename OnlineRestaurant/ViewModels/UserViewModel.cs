using OnlineRestaurant.Commands;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OnlineRestaurant.ViewModels
{
    public class UserViewModel : BaseVM
    {
        private readonly UserService _userService;
        private readonly UserCredentialsService _credentialsService;
        private User _currentUser;
        private bool _isLoggedIn;
        private string _displayName = "Guest";

        public event EventHandler LogoutEvent;

        public event EventHandler AutoLoginCompleted;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (SetProperty(ref _currentUser, value))
                {
                    bool newIsLoggedInValue = value != null;
                    IsLoggedIn = newIsLoggedInValue;
                    DisplayName = value != null ? value.FullName : "Guest";
                }
            }
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                System.Diagnostics.Debug.WriteLine($"IsLoggedIn changing from {_isLoggedIn} to {value}");
                SetProperty(ref _isLoggedIn, value);
                System.Diagnostics.Debug.WriteLine($"IsLoggedIn changed to {_isLoggedIn}");
            }
        }

        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        public ICommand LogoutCommand { get; }

        public UserViewModel(UserService userService, UserCredentialsService credentialsService)
        {
            _userService = userService;
            _credentialsService = credentialsService;
            LogoutCommand = new RelayCommand(Logout);

            Task.Run(async () => await TryAutoLoginAsync());
        }

        private async Task TryAutoLoginAsync()
        {
            try
            {
                var credentials = _credentialsService.LoadCredentials();
                if (credentials != null)
                {
                    var user = await _userService.Authenticate(credentials.Email, credentials.Password);
                    if (user != null)
                    {
                        CurrentUser = user;

                        System.Diagnostics.Debug.WriteLine("Auto-login completed successfully");
                        OnPropertyChanged(nameof(IsLoggedIn));
                        AutoLoginCompleted?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-login failed: {ex.Message}");
            }
        }

        public void Logout()
        {
            _credentialsService.ClearCredentials();

            CurrentUser = null;
            IsLoggedIn = false;
            DisplayName = "Guest";

            LogoutEvent?.Invoke(this, EventArgs.Empty);
        }

        public void ForceNotifyLoginStatus()
        {
            System.Diagnostics.Debug.WriteLine("Forcing notification of IsLoggedIn property");
            OnPropertyChanged(nameof(IsLoggedIn));
        }
    }
}