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
                return Visibility.Collapsed;
            }
            
            // Special handling for numeric values (like decimal)
            if (value is decimal decimalValue)
            {
                return decimalValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            
            string stringValue = value.ToString();
            
            // Empty strings are collapsed
            if (string.IsNullOrEmpty(stringValue))
            {
                return Visibility.Collapsed;
            }
            
            // If a parameter is provided, compare with it
            if (parameter != null)
            {
                string stringParameter = parameter.ToString();
                // Use case-insensitive comparison
                bool isMatch = string.Equals(stringValue, stringParameter, StringComparison.OrdinalIgnoreCase);
                return isMatch ? Visibility.Visible : Visibility.Collapsed;
            }
            
            // Non-empty strings with no parameter are visible
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 