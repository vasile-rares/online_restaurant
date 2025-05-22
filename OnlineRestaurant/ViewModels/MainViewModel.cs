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
        private readonly UserViewModel _userViewModel;
        private readonly ShoppingCartViewModel _shoppingCart;
        private BaseVM _currentViewModel;

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
        
        public bool IsEmployee => 
            UserViewModel.IsLoggedIn && 
            UserViewModel.CurrentUser?.Role != null && 
            UserViewModel.CurrentUser.Role.Equals("Angajat", StringComparison.OrdinalIgnoreCase);

        public ICommand NavigateToMenuCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand NavigateToCartCommand { get; }
        public ICommand NavigateToOrdersCommand { get; }
        public ICommand NavigateToEmployeeDashboardCommand { get; }

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
            NavigateToEmployeeDashboardCommand = new RelayCommand(NavigateToEmployeeDashboard, CanNavigateToEmployeeDashboard);
            
            // Subscribe to property changed event to update commands
            _userViewModel.PropertyChanged += (sender, args) => 
            {
                if (args.PropertyName == nameof(UserViewModel.IsLoggedIn) || args.PropertyName == nameof(UserViewModel.CurrentUser))
                {
                    ((RelayCommand)NavigateToProfileCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)NavigateToCartCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)NavigateToEmployeeDashboardCommand).RaiseCanExecuteChanged();
                    OnPropertyChanged(nameof(IsEmployee));
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
                ((RelayCommand)NavigateToEmployeeDashboardCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsEmployee));
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
                
                // Actualizăm explicit comenzile și proprietățile relevante pentru UI
                ((RelayCommand)NavigateToProfileCommand).RaiseCanExecuteChanged();
                ((RelayCommand)NavigateToCartCommand).RaiseCanExecuteChanged();
                ((RelayCommand)NavigateToOrdersCommand).RaiseCanExecuteChanged();
                ((RelayCommand)NavigateToEmployeeDashboardCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsEmployee));
                
                NavigateToProfile();
            };
            
            // Subscribe to register navigation request
            loginViewModel.NavigateToRegisterRequest += (sender, args) => NavigateToRegister();
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

        private void NavigateToEmployeeDashboard()
        {
            var employeeViewModel = _serviceProvider.GetRequiredService<EmployeeViewModel>();
            CurrentViewModel = employeeViewModel;
        }
        
        private bool CanNavigateToEmployeeDashboard()
        {
            // Make role comparison case insensitive and check for "Angajat" (Romanian for Employee)
            string? role = UserViewModel.CurrentUser?.Role;
            bool isEmployee = role != null && role.Equals("Angajat", StringComparison.OrdinalIgnoreCase);
            
            return UserViewModel.IsLoggedIn && isEmployee;
        }

        // Override OnDispose to clean up resources
        protected override void OnDispose()
        {
            // Clean up managed resources
            if (_currentViewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
            
            base.OnDispose();
        }
    }
} 