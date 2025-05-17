using OnlineRestaurant.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OnlineRestaurant.Converters
{
    public class OrderStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                switch (status)
                {
                    case OrderStatus.registered:
                        return new SolidColorBrush(Color.FromRgb(243, 156, 18)); // Orange
                    case OrderStatus.preparing:
                        return new SolidColorBrush(Color.FromRgb(155, 89, 182)); // Purple
                    case OrderStatus.out_for_delivery:
                        return new SolidColorBrush(Color.FromRgb(52, 152, 219)); // Blue
                    case OrderStatus.delivered:
                        return new SolidColorBrush(Color.FromRgb(39, 174, 96)); // Dark Green
                    case OrderStatus.canceled:
                        return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Red
                    default:
                        return new SolidColorBrush(Color.FromRgb(149, 165, 166)); // Gray
                }
            }
            
            return new SolidColorBrush(Color.FromRgb(149, 165, 166)); // Default gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 