using OnlineRestaurant.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OnlineRestaurant.Converters
{
    public class UniversalOrderStatusConverter : IValueConverter
    {
        public enum OutputType
        {
            String,    // Convertește în string localizat
            Color,     // Convertește în culoare
            Visibility // Convertește în Visibility
        }

        // Tipul de ieșire dorit
        public OutputType Output { get; set; } = OutputType.String;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Verifică dacă parametrul specifică modul de conversie
            if (parameter is string paramString)
            {
                foreach (OutputType type in Enum.GetValues(typeof(OutputType)))
                {
                    if (paramString.Contains(type.ToString()))
                    {
                        Output = type;
                        break;
                    }
                }
            }

            if (value is OrderStatus status)
            {
                switch (Output)
                {
                    case OutputType.String:
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

                    case OutputType.Color:
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

                    case OutputType.Visibility:
                        // O comandă poate fi anulată doar dacă este în starea "registered" sau "preparing"
                        return (status == OrderStatus.registered || status == OrderStatus.preparing) ? 
                               Visibility.Visible : Visibility.Collapsed;
                }
            }
            
            // Valori implicite pentru diferite tipuri de ieșire
            switch (Output)
            {
                case OutputType.String:
                    return string.Empty;
                case OutputType.Color:
                    return new SolidColorBrush(Color.FromRgb(149, 165, 166)); // Gray
                case OutputType.Visibility:
                    return Visibility.Collapsed;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 