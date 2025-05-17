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
        private DispatcherTimer _messageTimer;
        
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
        public ICommand CancelOrderCommand { get; }
        
        public OrdersViewModel(OrderService orderService, UserViewModel userViewModel)
        {
            _orderService = orderService;
            _userViewModel = userViewModel;
            _orders = new ObservableCollection<Order>();
            
            RefreshCommand = new RelayCommand(async () => await LoadOrdersAsync());
            BackCommand = new RelayCommand(NavigateBack);
            CancelOrderCommand = new RelayCommand<Order>(async (order) => await CancelOrderAsync(order), CanCancelOrder);
            
            // Inițializăm timer-ul pentru mesaje
            _messageTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _messageTimer.Tick += (s, e) =>
            {
                ErrorMessage = string.Empty;
                _messageTimer.Stop();
            };
            
            // Încărcăm comenzile utilizatorului curent - utilizăm Dispatcher.InvokeAsync
            // în loc de Task.Run pentru a evita eroarea de thread
            Application.Current.Dispatcher.InvokeAsync(async () => await LoadOrdersAsync());
        }
        
        private bool CanCancelOrder(Order order)
        {
            // O comandă poate fi anulată doar dacă este în starea "registered" sau "preparing"
            return order != null && 
                   (order.Status == OrderStatus.registered || 
                    order.Status == OrderStatus.preparing);
        }
        
        private async Task CancelOrderAsync(Order order)
        {
            if (order == null)
                return;
                
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                // Setăm starea comenzii la "canceled"
                order.Status = OrderStatus.canceled;
                
                // Actualizăm comanda în baza de date
                await _orderService.UpdateAsync(order);
                await _orderService.SaveChangesAsync();
                
                // Reîncărcăm lista de comenzi
                await LoadOrdersAsync();
                
                // Afișăm un mesaj de succes temporar
                ShowTimedMessage("Comanda a fost anulată cu succes.");
            }
            catch (Exception ex)
            {
                ShowTimedMessage($"Eroare la anularea comenzii: {ex.Message}");
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsLoading = false;
                });
            }
        }
        
        private void ShowTimedMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorMessage = message;
                _messageTimer.Stop();
                _messageTimer.Start();
            });
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
                ShowTimedMessage($"Eroare la încărcarea comenzilor: {ex.Message}");
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