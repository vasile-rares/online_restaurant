using OnlineRestaurant.Commands;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace OnlineRestaurant.ViewModels
{
    public class CartItemViewModel : BaseVM
    {
        private int _id;
        private MenuRestaurantViewModel.ItemMenuType _type;
        private string _name;
        private decimal _price;
        private int _quantity;
        private string _imageUrl;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public MenuRestaurantViewModel.ItemMenuType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public decimal TotalPrice => Price * Quantity;

        public ICommand IncreaseQuantityCommand { get; set; }
        public ICommand DecreaseQuantityCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
    }

    public class ShoppingCartViewModel : BaseVM
    {
        private ObservableCollection<CartItemViewModel> _items;
        private decimal _totalPrice;
        private decimal _originalPrice;
        private decimal _discountAmount;
        private decimal _shippingCost;
        private bool _isEligibleForValueDiscount;
        private bool _isEligibleForLoyaltyDiscount;
        private int _itemCount;
        private readonly OrderService _orderService;
        private readonly UserViewModel _userViewModel;
        private readonly AppSettingsService _appSettings;
        private string _message;
        private bool _isProcessing;
        private DispatcherTimer _messageTimer;

        public ObservableCollection<CartItemViewModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public decimal TotalPrice
        {
            get => _totalPrice;
            set => SetProperty(ref _totalPrice, value);
        }

        public decimal OriginalPrice
        {
            get => _originalPrice;
            set => SetProperty(ref _originalPrice, value);
        }

        public decimal DiscountAmount
        {
            get => _discountAmount;
            set => SetProperty(ref _discountAmount, value);
        }

        public decimal ShippingCost
        {
            get => _shippingCost;
            set => SetProperty(ref _shippingCost, value);
        }

        public bool IsEligibleForValueDiscount
        {
            get => _isEligibleForValueDiscount;
            set => SetProperty(ref _isEligibleForValueDiscount, value);
        }

        public bool IsEligibleForLoyaltyDiscount
        {
            get => _isEligibleForLoyaltyDiscount;
            set => SetProperty(ref _isEligibleForLoyaltyDiscount, value);
        }

        public int ItemCount
        {
            get => _itemCount;
            set => SetProperty(ref _itemCount, value);
        }

        public bool HasItems => Items.Count > 0;

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public ICommand ClearCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        public ShoppingCartViewModel(OrderService orderService, UserViewModel userViewModel, AppSettingsService appSettings)
        {
            _orderService = orderService;
            _userViewModel = userViewModel;
            _appSettings = appSettings;
            
            Items = new ObservableCollection<CartItemViewModel>();
            ClearCartCommand = new RelayCommand(ClearCart);
            CheckoutCommand = new RelayCommand(async () => await CheckoutAsync(), CanCheckout);
            
            // Inițializăm timer-ul pentru mesaje
            _messageTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _messageTimer.Tick += (s, e) =>
            {
                Message = string.Empty;
                _messageTimer.Stop();
            };
            
            // Initialize cart summary
            _ = UpdateCartSummary();
        }

        public void AddToCart(MenuRestaurantViewModel.ItemMenuViewModel item)
        {
            if (item == null || !item.Available)
                return;

            var existingItem = Items.FirstOrDefault(i => 
                i.Id == item.Id && i.Type == item.Type);

            if (existingItem != null)
            {
                // Incrementează cantitatea dacă produsul există deja în coș
                existingItem.Quantity++;
            }
            else
            {
                // Adaugă un nou produs în coș
                string imagePath = "/Images/default.jpg";
                
                if (item.Images != null && item.Images.Count > 0)
                {
                    // Asigură-te că avem calea relativă corectă către imagine
                    imagePath = item.Images[0];
                    if (!imagePath.StartsWith("/"))
                    {
                        imagePath = "/" + imagePath;
                    }
                }
                
                var cartItem = new CartItemViewModel
                {
                    Id = item.Id,
                    Type = item.Type,
                    Name = item.Name,
                    Price = item.Price,
                    Quantity = 1,
                    ImageUrl = imagePath
                };

                // Inițializează comenzile pentru acest item
                cartItem.IncreaseQuantityCommand = new RelayCommand(() => IncreaseQuantity(cartItem));
                cartItem.DecreaseQuantityCommand = new RelayCommand(() => DecreaseQuantity(cartItem));
                cartItem.RemoveCommand = new RelayCommand(() => RemoveFromCart(cartItem));

                Items.Add(cartItem);
            }

            _ = UpdateCartSummary();
        }

        private void IncreaseQuantity(CartItemViewModel item)
        {
            item.Quantity++;
            _ = UpdateCartSummary();
        }

        private void DecreaseQuantity(CartItemViewModel item)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
                _ = UpdateCartSummary();
            }
            else
            {
                RemoveFromCart(item);
            }
        }

        public void RemoveFromCart(CartItemViewModel item)
        {
            Items.Remove(item);
            _ = UpdateCartSummary();
        }

        public void ClearCart()
        {
            Items.Clear();
            _ = UpdateCartSummary();
        }

        private async Task UpdateCartSummary()
        {
            OriginalPrice = Items.Sum(i => i.TotalPrice);
            DiscountAmount = 0;
            ShippingCost = 0;

            if (_userViewModel.IsLoggedIn)
            {
                // Verificăm dacă utilizatorul este eligibil pentru discount pe baza comenzilor anterioare
                try
                {
                    var userOrders = await _orderService.GetByUserAsync(_userViewModel.CurrentUser.IdUser);
                    
                    // Calculează numărul de comenzi din perioada specificată
                    var timeInterval = _appSettings.GetTimeIntervalDays();
                    var orderThreshold = _appSettings.GetOrderCountThreshold();
                    
                    var orderCount = userOrders
                        .Count(o => o.OrderDate >= DateTime.Now.AddDays(-timeInterval) && 
                               o.Status != OrderStatus.canceled);
                    
                    IsEligibleForLoyaltyDiscount = orderCount >= orderThreshold;
                    
                    // Verificăm dacă valoarea comenzii depășește pragul pentru discount
                    var valueThreshold = _appSettings.GetValueThreshold();
                    IsEligibleForValueDiscount = OriginalPrice >= valueThreshold;
                    
                    // Aplicăm discount doar dacă utilizatorul este eligibil pentru cel puțin unul din discounturi
                    if (IsEligibleForValueDiscount || IsEligibleForLoyaltyDiscount)
                    {
                        var discountPercentage = _appSettings.GetDiscountPercentage();
                        DiscountAmount = Math.Round(OriginalPrice * discountPercentage / 100, 2);
                    }
                    
                    // Determinăm costul de livrare
                    var freeShippingThreshold = _appSettings.GetFreeShippingThreshold();
                    if (OriginalPrice < freeShippingThreshold)
                    {
                        ShippingCost = _appSettings.GetShippingCost();
                    }
                }
                catch (Exception)
                {
                    // În caz de eroare, nu aplicăm niciun discount
                    IsEligibleForValueDiscount = false;
                    IsEligibleForLoyaltyDiscount = false;
                }
            }
            else
            {
                // Determinăm doar costul de livrare pentru utilizatori neautentificați
                var freeShippingThreshold = _appSettings.GetFreeShippingThreshold();
                if (OriginalPrice < freeShippingThreshold)
                {
                    ShippingCost = _appSettings.GetShippingCost();
                }
            }
            
            // Calculăm prețul total inclusiv discount și livrare
            TotalPrice = OriginalPrice - DiscountAmount + ShippingCost;
            ItemCount = Items.Sum(i => i.Quantity);
            
            // Notifică schimbarea proprietății HasItems
            OnPropertyChanged(nameof(HasItems));
            
            // Actualizează starea comenzii de Checkout
            ((RelayCommand)CheckoutCommand).RaiseCanExecuteChanged();
        }
        
        private bool CanCheckout()
        {
            return Items.Count > 0 && !IsProcessing && _userViewModel.IsLoggedIn;
        }
        
        private async Task CheckoutAsync()
        {
            try
            {
                IsProcessing = true;
                
                if (!_userViewModel.IsLoggedIn)
                {
                    ShowTimedMessage("Trebuie să fiți autentificat pentru a finaliza comanda.");
                    return;
                }
                
                // Create a new order
                var order = new Order
                {
                    IdUser = _userViewModel.CurrentUser.IdUser,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.registered
                };
                
                // Add dishes to the order
                foreach (var item in Items)
                {
                    if (item.Type == MenuRestaurantViewModel.ItemMenuType.Dish)
                    {
                        order.OrderDishes.Add(new OrderDish
                        {
                            IdDish = item.Id,
                            Quantity = item.Quantity
                        });
                    }
                    else if (item.Type == MenuRestaurantViewModel.ItemMenuType.Menu)
                    {
                        order.OrderMenus.Add(new OrderMenu
                        {
                            IdMenu = item.Id,
                            Quantity = item.Quantity
                        });
                    }
                }
                
                // Setăm suma totală a comenzii cu discount și taxe aplicate
                order.TotalAmount = TotalPrice;
                
                // Save the order to database
                await _orderService.AddAsync(order);
                await _orderService.SaveChangesAsync();
                
                // Clear the cart after successful checkout
                ClearCart();
                ShowTimedMessage("Comanda a fost plasată cu succes!");
            }
            catch (Exception ex)
            {
                ShowTimedMessage($"Eroare la plasarea comenzii: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
                ((RelayCommand)CheckoutCommand).RaiseCanExecuteChanged();
            }
        }
        
        private void ShowTimedMessage(string message)
        {
            Message = message;
            _messageTimer.Stop();
            _messageTimer.Start();
        }
    }
} 