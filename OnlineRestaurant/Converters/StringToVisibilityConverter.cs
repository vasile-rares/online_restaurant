using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OnlineRestaurant.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                System.Diagnostics.Debug.WriteLine($"StringToVisibilityConverter: value is null, parameter: {parameter}");
                return Visibility.Collapsed;
            }
            
            string stringValue = value.ToString();
            string stringParameter = parameter?.ToString() ?? string.Empty;
            
            System.Diagnostics.Debug.WriteLine($"StringToVisibilityConverter: comparing '{stringValue}' with '{stringParameter}'");
            
            // Use case-insensitive comparison
            bool isMatch = string.Equals(stringValue, stringParameter, StringComparison.OrdinalIgnoreCase);
            
            System.Diagnostics.Debug.WriteLine($"StringToVisibilityConverter: match result: {isMatch}");
            
            return isMatch ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 