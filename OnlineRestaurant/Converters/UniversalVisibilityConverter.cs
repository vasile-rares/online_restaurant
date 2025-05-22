using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using OnlineRestaurant.Models;

namespace OnlineRestaurant.Converters
{
    public class UniversalVisibilityConverter : IValueConverter
    {
        public enum ConversionMode
        {
            Auto,       // Auto-detectează tipul de valoare
            Boolean,    // Tratează ca Boolean (true = Visible, false = Collapsed)
            String,     // Tratează ca String (non-null și non-empty = Visible)
            Null,       // Verifică doar nulitate (non-null = Visible)
            Number,     // Tratează ca număr (>0 = Visible)
            OrderStatus // Special pentru OrderStatus
        }

        // Mod implicit de conversie
        public ConversionMode Mode { get; set; } = ConversionMode.Auto;
        
        // Indică dacă rezultatul ar trebui inversat
        public bool Invert { get; set; } = false;

        // Valoarea de comparație opțională
        public object CompareValue { get; set; } = null;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Verifică dacă parametrul specifică un mod sau inversare
            if (parameter is string paramString)
            {
                if (paramString.Contains("Invert"))
                {
                    Invert = true;
                }
                
                foreach (ConversionMode mode in Enum.GetValues(typeof(ConversionMode)))
                {
                    if (paramString.Contains(mode.ToString()))
                    {
                        Mode = mode;
                        break;
                    }
                }
            }

            bool result = false;
            
            // Determină vizibilitatea în funcție de mod
            switch (Mode)
            {
                case ConversionMode.Boolean:
                    result = value is bool boolValue && boolValue;
                    break;
                    
                case ConversionMode.String:
                    if (value == null)
                    {
                        result = false;
                    }
                    else if (CompareValue != null)
                    {
                        result = string.Equals(value.ToString(), CompareValue.ToString(), 
                                           StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        result = !string.IsNullOrEmpty(value.ToString());
                    }
                    break;
                    
                case ConversionMode.Null:
                    result = value != null;
                    break;
                    
                case ConversionMode.Number:
                    if (value is int intValue)
                        result = intValue > 0;
                    else if (value is double doubleValue)
                        result = doubleValue > 0;
                    else if (value is decimal decimalValue)
                        result = decimalValue > 0;
                    else
                        result = false;
                    break;
                    
                case ConversionMode.OrderStatus:
                    if (value is OrderStatus status)
                    {
                        result = status == OrderStatus.registered || status == OrderStatus.preparing;
                    }
                    break;
                    
                case ConversionMode.Auto:
                default:
                    // Auto-detectare tip
                    if (value == null)
                    {
                        result = false;
                    }
                    else if (value is bool bVal)
                    {
                        result = bVal;
                    }
                    else if (value is int iVal)
                    {
                        result = iVal > 0;
                    }
                    else if (value is double dVal)
                    {
                        result = dVal > 0;
                    }
                    else if (value is decimal decVal)
                    {
                        result = decVal > 0;
                    }
                    else if (value is OrderStatus orderStatus)
                    {
                        result = orderStatus == OrderStatus.registered || orderStatus == OrderStatus.preparing;
                    }
                    else
                    {
                        // Pentru string-uri și alte obiecte
                        result = !string.IsNullOrEmpty(value.ToString());
                    }
                    break;
            }
            
            // Inversează rezultatul dacă este necesar
            if (Invert)
            {
                result = !result;
            }
            
            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 