using OnlineRestaurant.Commands;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace OnlineRestaurant.ViewModels
{
    public class OrdersViewModel : BaseVM
    {
        private readonly OrderService _orderService;
        private readonly UserViewModel _userViewModel;
        private ObservableCollection<Order> _orders;
        private bool _isLoading;
        private string _errorMessage;
        
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }
        
        public OrdersViewModel(OrderService orderService, UserViewModel userViewModel)
        {
            _orderService = orderService;
            _userViewModel = userViewModel;
            _orders = new ObservableCollection<Order>();
            
            RefreshCommand = new RelayCommand(async () => await LoadOrdersAsync());
            BackCommand = new RelayCommand(NavigateBack);
            
            // Încărcăm comenzile utilizatorului curent - utilizăm Dispatcher.InvokeAsync
            // în loc de Task.Run pentru a evita eroarea de thread
            Application.Current.Dispatcher.InvokeAsync(async () => await LoadOrdersAsync());
        }
        
        private async Task LoadOrdersAsync()
        {
            if (_userViewModel.CurrentUser == null)
            {
                ErrorMessage = "Trebuie să fiți autentificat pentru a vedea comenzile.";
                return;
            }
            
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                var userOrders = await _orderService.GetByUserAsync(_userViewModel.CurrentUser.IdUser);
                
                // Ne asigurăm că actualizăm colecția pe thread-ul UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Orders.Clear();
                    foreach (var order in userOrders)
                    {
                        Orders.Add(order);
                    }
                    
                    if (Orders.Count == 0)
                    {
                        ErrorMessage = "Nu aveți comenzi înregistrate.";
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = $"Eroare la încărcarea comenzilor: {ex.Message}";
                });
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsLoading = false;
                });
            }
        }
        
        private void NavigateBack()
        {
            // Va fi implementat în MainViewModel
        }
    }
} 