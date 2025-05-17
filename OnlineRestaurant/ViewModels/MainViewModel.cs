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
    public class MainViewModel : BaseVM, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private BaseVM _currentViewModel;
        private readonly UserViewModel _userViewModel;
        private readonly ShoppingCartViewModel _shoppingCart;
        private bool _isDisposed;

        public BaseVM CurrentViewModel
        {
            get => _currentViewModel;
            set 
            {
                // Dispose of previous view model if it's disposable
                if (_currentViewModel is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                SetProperty(ref _currentViewModel, value);
            }
        }

        public UserViewModel UserViewModel => _userViewModel;
        
        public ShoppingCartViewModel ShoppingCart => _shoppingCart;

        public ICommand NavigateToMenuCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand NavigateToCartCommand { get; }
        public ICommand NavigateToOrdersCommand { get; }

        public MainViewModel(
            IServiceProvider serviceProvider,
            MenuRestaurantViewModel menuRestaurantViewModel,
            UserViewModel userViewModel,
            ShoppingCartViewModel shoppingCartViewModel)
        {
            _serviceProvider = serviceProvider;
            _userViewModel = userViewModel;
            _shoppingCart = shoppingCartViewModel;
            
            // Set initial view
            CurrentViewModel = menuRestaurantViewModel;
            
            // Make sure the menu view has access to the shopping cart and user info
            if (menuRestaurantViewModel is MenuRestaurantViewModel menuVM)
            {
                menuVM.ShoppingCart = _shoppingCart;
                menuVM.UserViewModel = _userViewModel;
            }
            
            // Initialize navigation commands
            NavigateToMenuCommand = new RelayCommand(NavigateToMenu);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
            NavigateToProfileCommand = new RelayCommand(NavigateToProfile, CanNavigateToProfile);
            NavigateToCartCommand = new RelayCommand(NavigateToCart, CanNavigateToCart);
            NavigateToOrdersCommand = new RelayCommand(NavigateToOrders, CanNavigateToProfile);
            
            // Subscribe to property changed event to update commands
            _userViewModel.PropertyChanged += (sender, args) => 
            {
                if (args.PropertyName == nameof(UserViewModel.IsLoggedIn))
                {
                    ((RelayCommand)NavigateToProfileCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)NavigateToCartCommand).RaiseCanExecuteChanged();
                }
            };
            
            // Subscribe to shopping cart changes
            _shoppingCart.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ShoppingCartViewModel.ItemCount))
                {
                    // Update any UI that shows cart items count
                    OnPropertyChanged(nameof(ShoppingCart));
                }
            };
            
            // Subscribe to logout event to navigate to login page and reset cart
            _userViewModel.LogoutEvent += (sender, args) => 
            {
                // Reset shopping cart when user logs out
                _shoppingCart.ClearCart();
                NavigateToLogin();
            };
            
            // Subscribe to auto-login event to update UI when auto-login completes
            _userViewModel.AutoLoginCompleted += (sender, args) =>
            {
                // We don't need to navigate anywhere - just update commands
                ((RelayCommand)NavigateToProfileCommand).RaiseCanExecuteChanged();
                ((RelayCommand)NavigateToCartCommand).RaiseCanExecuteChanged();
            };
        }

        private void NavigateToMenu()
        {
            var menuViewModel = _serviceProvider.GetRequiredService<MenuRestaurantViewModel>();
            // Ensure the menu has access to the shopping cart and user info
            menuViewModel.ShoppingCart = _shoppingCart;
            menuViewModel.UserViewModel = _userViewModel;
            CurrentViewModel = menuViewModel;
        }
        
        private void NavigateToCart()
        {
            // Creating a CartViewModel that's a wrapper around the ShoppingCart
            // This is a simplified pattern - in a larger app you might want to create a dedicated CartViewModel
            CurrentViewModel = _shoppingCart;
        }
        
        private bool CanNavigateToCart()
        {
            // Pentru a naviga în coș, utilizatorul trebuie să fie doar autentificat
            // Nu mai verificăm dacă coșul are produse
            return UserViewModel.IsLoggedIn;
        }
        
        private void NavigateToLogin()
        {
            var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            CurrentViewModel = loginViewModel;
            
            // Subscribe to login success event
            loginViewModel.LoginSuccessful += (sender, args) => 
            {
                // Reset shopping cart when user logs in
                _shoppingCart.ClearCart();
                NavigateToProfile();
            };
            
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
        
        private void NavigateToOrders()
        {
            var ordersViewModel = _serviceProvider.GetRequiredService<OrdersViewModel>();
            CurrentViewModel = ordersViewModel;
            
            // Set handler for back navigation
            if (ordersViewModel is OrdersViewModel orders)
            {
                // Use reflection to access private NavigateBack method
                var navigateMethod = orders.GetType().GetMethod("NavigateBack", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (navigateMethod != null)
                {
                    var originalAction = navigateMethod.CreateDelegate(typeof(Action), orders) as Action;
                    
                    // Redefine action to include navigation to profile
                    Action newAction = () =>
                    {
                        originalAction?.Invoke();
                        NavigateToProfile();
                    };
                    
                    // Set new delegate for BackCommand
                    var command = orders.BackCommand as RelayCommand;
                    var field = typeof(RelayCommand).GetField("_execute", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (field != null && command != null)
                    {
                        field.SetValue(command, newAction);
                    }
                }
            }
        }

        // Implement IDisposable pattern correctly
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Clean up managed resources
                    if (_currentViewModel is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                
                // Clean up unmanaged resources
                
                _isDisposed = true;
            }
        }
        
        ~MainViewModel()
        {
            Dispose(false);
        }
    }
} 