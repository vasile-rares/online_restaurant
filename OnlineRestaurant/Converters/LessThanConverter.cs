using System;
using System.Globalization;
using System.Windows.Data;

namespace OnlineRestaurant.Converters
{
    public class LessThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return false;
            }

            double numValue;
            double threshold;

            // Try to convert the input value to a double
            if (!double.TryParse(value.ToString(), out numValue))
            {
                return false;
            }

            // Try to convert the parameter to a double
            if (!double.TryParse(parameter.ToString(), out threshold))
            {
                return false;
            }

            // Return true if the value is less than the threshold
            return numValue < threshold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 