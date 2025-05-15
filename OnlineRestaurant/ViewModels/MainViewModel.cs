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

        public BaseVM CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand NavigateToMeniuCommand { get; }

        public MainViewModel(
            IServiceProvider serviceProvider,
            MeniuRestaurantViewModel meniuRestaurantViewModel)
        {
            _serviceProvider = serviceProvider;
            
            // Setăm view-ul inițial
            CurrentViewModel = meniuRestaurantViewModel;
            
            // Inițializăm comenzile de navigare
            NavigateToMeniuCommand = new RelayCommand(() => 
                CurrentViewModel = _serviceProvider.GetRequiredService<MeniuRestaurantViewModel>());
        }
    }
} 