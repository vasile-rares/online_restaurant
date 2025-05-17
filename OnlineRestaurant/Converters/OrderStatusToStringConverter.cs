using OnlineRestaurant.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace OnlineRestaurant.Converters
{
    public class OrderStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                switch (status)
                {
                    case OrderStatus.registered:
                        return "Înregistrată";
                    case OrderStatus.preparing:
                        return "În preparare";
                    case OrderStatus.out_for_delivery:
                        return "În livrare";
                    case OrderStatus.delivered:
                        return "Livrată";
                    case OrderStatus.canceled:
                        return "Anulată";
                    default:
                        return status.ToString();
                }
            }
            
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 