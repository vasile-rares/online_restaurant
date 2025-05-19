using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OnlineRestaurant.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter != null && bool.TryParse(parameter.ToString(), out bool invertValue) && invertValue;
            
            bool isNull = value == null;
            
            if (invert)
            {
                return isNull ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return isNull ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 