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

            CurrentViewModel = menuRestaurantViewModel;

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

            _shoppingCart.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ShoppingCartViewModel.ItemCount))
                {
                    OnPropertyChanged(nameof(ShoppingCart));
                }
            };

            _userViewModel.LogoutEvent += (sender, args) =>
            {
                _shoppingCart.ClearCart();
                NavigateToLogin();
            };

            _userViewModel.AutoLoginCompleted += (sender, args) =>
            {
                ((RelayCommand)NavigateToProfileCommand).RaiseCanExecuteChanged();
                ((RelayCommand)NavigateToCartCommand).RaiseCanExecuteChanged();
                ((RelayCommand)NavigateToEmployeeDashboardCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsEmployee));
            };
        }

        private void NavigateToMenu()
        {
            var menuViewModel = _serviceProvider.GetRequiredService<MenuRestaurantViewModel>();
            menuViewModel.ShoppingCart = _shoppingCart;
            menuViewModel.UserViewModel = _userViewModel;
            CurrentViewModel = menuViewModel;
        }

        private void NavigateToCart()
        {
            CurrentViewModel = _shoppingCart;
        }

        private bool CanNavigateToCart()
        {
            return UserViewModel.IsLoggedIn;
        }

        private void NavigateToLogin()
        {
            var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            CurrentViewModel = loginViewModel;

            // Subscribe to login success event
            loginViewModel.LoginSuccessful += (sender, args) =>
            {
                _shoppingCart.ClearCart();

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
            profileViewModel.UpdateProfileFromUser();
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
            if (CurrentViewModel is EmployeeViewModel)
                return;

            var employeeViewModel = _serviceProvider.GetRequiredService<EmployeeViewModel>();
            CurrentViewModel = employeeViewModel;
        }

        private bool CanNavigateToEmployeeDashboard()
        {
            string? role = UserViewModel.CurrentUser?.Role;
            bool isEmployee = role != null && role.Equals("Angajat", StringComparison.OrdinalIgnoreCase);

            return UserViewModel.IsLoggedIn && isEmployee;
        }

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