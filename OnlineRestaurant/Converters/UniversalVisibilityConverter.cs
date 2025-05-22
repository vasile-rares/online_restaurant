using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OnlineRestaurant.Converters
{
    public class UniversalVisibilityConverter : IValueConverter
    {
        public enum ConversionMode
        {
            Boolean,    // Tratează ca Boolean (true = Visible, false = Collapsed)
            String,     // Tratează ca String (non-null și non-empty = Visible)
            Null        // Verifică doar nulitate (non-null = Visible)
        }

        // Mod implicit de conversie
        public ConversionMode Mode { get; set; } = ConversionMode.Boolean;
        
        // Indică dacă rezultatul ar trebui inversat
        public bool Invert { get; set; } = false;

        // Valoarea de comparație opțională pentru string-uri
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