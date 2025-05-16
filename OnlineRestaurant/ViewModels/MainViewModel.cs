using OnlineRestaurant.Commands;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace OnlineRestaurant.ViewModels
{
    public class MainViewModel : BaseVM
    {
        private readonly IServiceProvider _serviceProvider;
        private BaseVM _currentViewModel;
        private readonly UserViewModel _userViewModel;

        public BaseVM CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public UserViewModel UserViewModel => _userViewModel;

        public ICommand NavigateToMenuCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToProfileCommand { get; }

        public MainViewModel(
            IServiceProvider serviceProvider,
            MenuRestaurantViewModel menuRestaurantViewModel,
            UserViewModel userViewModel)
        {
            _serviceProvider = serviceProvider;
            _userViewModel = userViewModel;
            
            // Set initial view
            CurrentViewModel = menuRestaurantViewModel;
            
            // Initialize navigation commands
            NavigateToMenuCommand = new RelayCommand(NavigateToMenu);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
            NavigateToProfileCommand = new RelayCommand(NavigateToProfile, CanNavigateToProfile);
            
            // Subscribe to property changed event to update commands
            _userViewModel.PropertyChanged += (sender, args) => 
            {
                if (args.PropertyName == nameof(UserViewModel.IsLoggedIn))
                {
                    ((RelayCommand)NavigateToProfileCommand).RaiseCanExecuteChanged();
                }
            };
            
            // Subscribe to logout event to navigate to login page
            _userViewModel.LogoutEvent += (sender, args) => NavigateToLogin();
        }

        private void NavigateToMenu()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<MenuRestaurantViewModel>();
        }
        
        private void NavigateToLogin()
        {
            var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            CurrentViewModel = loginViewModel;
            
            // Subscribe to login success event
            loginViewModel.LoginSuccessful += (sender, args) => NavigateToProfile();
            
            // Set handler for navigation to registration
            if (loginViewModel is LoginViewModel login)
            {
                // Use reflection to access private NavigateToCreateAccount method
                var navigateMethod = login.GetType().GetMethod("NavigateToCreateAccount", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (navigateMethod != null)
                {
                    var originalAction = navigateMethod.CreateDelegate(typeof(Action), login) as Action;
                    
                    // Redefine action to include navigation to RegisterViewModel
                    Action newAction = () =>
                    {
                        originalAction?.Invoke();
                        NavigateToRegister();
                    };
                    
                    // Set new delegate for CreateAccountCommand
                    var command = login.CreateAccountCommand as RelayCommand;
                    var field = typeof(RelayCommand).GetField("_execute", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (field != null && command != null)
                    {
                        field.SetValue(command, newAction);
                    }
                }
            }
        }
        
        private void NavigateToRegister()
        {
            var registerViewModel = _serviceProvider.GetRequiredService<RegisterViewModel>();
            CurrentViewModel = registerViewModel;
            
            // Subscribe to register success event
            registerViewModel.RegisterSuccessful += (sender, args) => NavigateToProfile();
            
            // Set handler for back navigation to login
            if (registerViewModel is RegisterViewModel register)
            {
                // Use reflection to access private NavigateBack method
                var navigateMethod = register.GetType().GetMethod("NavigateBack", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (navigateMethod != null)
                {
                    var originalAction = navigateMethod.CreateDelegate(typeof(Action), register) as Action;
                    
                    // Redefine action to include navigation to LoginViewModel
                    Action newAction = () =>
                    {
                        originalAction?.Invoke();
                        NavigateToLogin();
                    };
                    
                    // Set new delegate for CancelCommand
                    var command = register.CancelCommand as RelayCommand;
                    var field = typeof(RelayCommand).GetField("_execute", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (field != null && command != null)
                    {
                        field.SetValue(command, newAction);
                    }
                }
            }
        }
        
        private void NavigateToProfile()
        {
            var profileViewModel = _serviceProvider.GetRequiredService<UserProfileViewModel>();
            profileViewModel.UpdateProfileFromUser(); // Update data from user profile
            CurrentViewModel = profileViewModel;
        }
        
        private bool CanNavigateToProfile()
        {
            return UserViewModel.IsLoggedIn;
        }
    }
} 