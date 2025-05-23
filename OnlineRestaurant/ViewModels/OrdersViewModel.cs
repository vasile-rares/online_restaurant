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
        private string _statusMessage;
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

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
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

            _messageTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _messageTimer.Tick += (s, e) =>
            {
                StatusMessage = string.Empty;
                _messageTimer.Stop();
            };

            Application.Current.Dispatcher.InvokeAsync(async () => await LoadOrdersAsync());
        }

        private bool CanCancelOrder(Order order)
        {
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
                StatusMessage = string.Empty;

                await _orderService.UpdateOrderStatusAsync(order.IdOrder, OrderStatus.canceled);

                await LoadOrdersAsync();

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
                StatusMessage = message;
                _messageTimer.Stop();
                _messageTimer.Start();
            });
        }

        private async Task LoadOrdersAsync()
        {
            if (_userViewModel.CurrentUser == null)
            {
                StatusMessage = "Trebuie să fiți autentificat pentru a vedea comenzile.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = string.Empty;

                var userOrders = await _orderService.GetByUserAsync(_userViewModel.CurrentUser.IdUser);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Orders.Clear();
                    foreach (var order in userOrders)
                    {
                        Orders.Add(order);
                    }

                    if (Orders.Count == 0)
                    {
                        StatusMessage = "Nu aveți comenzi înregistrate.";
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
        { // Implementat in MainViewModel prin eveniment
        }
    }
}