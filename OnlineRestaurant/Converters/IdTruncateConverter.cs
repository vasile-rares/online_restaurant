using System;
using System.Globalization;
using System.Windows.Data;

namespace OnlineRestaurant.Converters
{
    public class IdTruncateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
                
            string stringValue = value.ToString();
            int length = 8; // Default length to truncate to
            
            // If a parameter is provided, try to use it as the length
            if (parameter != null && int.TryParse(parameter.ToString(), out int paramLength))
            {
                length = paramLength;
            }
            
            // If the string is longer than the desired length, truncate it
            if (stringValue.Length > length)
            {
                return stringValue.Substring(0, length);
            }
            
            // Otherwise return the original string
            return stringValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 