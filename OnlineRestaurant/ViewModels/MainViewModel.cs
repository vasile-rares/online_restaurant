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

        public ICommand NavigateToMeniuCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToProfileCommand { get; }

        public MainViewModel(
            IServiceProvider serviceProvider,
            MeniuRestaurantViewModel meniuRestaurantViewModel,
            UserViewModel userViewModel)
        {
            _serviceProvider = serviceProvider;
            _userViewModel = userViewModel;
            
            // Setăm view-ul inițial
            CurrentViewModel = meniuRestaurantViewModel;
            
            // Inițializăm comenzile de navigare
            NavigateToMeniuCommand = new RelayCommand(NavigateToMeniu);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
            NavigateToProfileCommand = new RelayCommand(NavigateToProfile, CanNavigateToProfile);
            
            // Abonăm-ne la evenimentul de schimbare a proprietăților pentru a actualiza comenzile
            _userViewModel.PropertyChanged += (sender, args) => 
            {
                if (args.PropertyName == nameof(UserViewModel.IsLoggedIn))
                {
                    ((RelayCommand)NavigateToProfileCommand).RaiseCanExecuteChanged();
                }
            };
            
            // Abonăm-ne la evenimentul de delogare pentru a naviga către pagina de login
            _userViewModel.LogoutEvent += (sender, args) => NavigateToLogin();
        }

        private void NavigateToMeniu()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<MeniuRestaurantViewModel>();
        }
        
        private void NavigateToLogin()
        {
            var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            CurrentViewModel = loginViewModel;
            
            // Abonăm-ne la evenimentul de succes pentru autentificare
            loginViewModel.LoginSuccessful += (sender, args) => NavigateToProfile();
            
            // Setăm handler pentru navigarea la înregistrare
            if (loginViewModel is LoginViewModel login)
            {
                // Folosim reflecția pentru a accesa metoda privată NavigateToCreateAccount
                var navigateMethod = login.GetType().GetMethod("NavigateToCreateAccount", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (navigateMethod != null)
                {
                    var originalAction = navigateMethod.CreateDelegate(typeof(Action), login) as Action;
                    
                    // Redefinim acțiunea pentru a include navigarea către RegisterViewModel
                    Action newAction = () =>
                    {
                        originalAction?.Invoke();
                        NavigateToRegister();
                    };
                    
                    // Setăm noul delegat pentru comanda CreateAccountCommand
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
            
            // Setăm handler pentru navigarea înapoi la login
            if (registerViewModel is RegisterViewModel register)
            {
                // Folosim reflecția pentru a accesa metoda privată NavigateBack
                var navigateMethod = register.GetType().GetMethod("NavigateBack", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (navigateMethod != null)
                {
                    var originalAction = navigateMethod.CreateDelegate(typeof(Action), register) as Action;
                    
                    // Redefinim acțiunea pentru a include navigarea către LoginViewModel
                    Action newAction = () =>
                    {
                        originalAction?.Invoke();
                        NavigateToLogin();
                    };
                    
                    // Setăm noul delegat pentru comanda CancelCommand
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
            profileViewModel.UpdateProfileFromUser(); // Actualizăm datele din profilul de utilizator
            CurrentViewModel = profileViewModel;
        }
        
        private bool CanNavigateToProfile()
        {
            return UserViewModel.IsLoggedIn;
        }
    }
} 