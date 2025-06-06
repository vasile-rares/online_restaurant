using System;
using System.Globalization;
using System.Windows.Data;

namespace OnlineRestaurant.Converters
{
    public class UniversalValueConverter : IValueConverter
    {
        public enum OperationType
        {
            LessThan
        }

        // Tipul operației
        public OperationType Operation { get; set; } = OperationType.LessThan;

        public object OperationParameter { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string paramString)
            {
                foreach (OperationType op in Enum.GetValues(typeof(OperationType)))
                {
                    if (paramString.Contains(op.ToString()))
                    {
                        Operation = op;
                        break;
                    }
                }

                string[] parts = paramString.Split(':');
                if (parts.Length > 1 && double.TryParse(parts[1], out double paramValue))
                {
                    OperationParameter = paramValue;
                }
            }

            if (OperationParameter == null && parameter != null)
            {
                OperationParameter = parameter;
            }

            switch (Operation)
            {
                case OperationType.LessThan:
                    return ProcessLessThan(value);

                default:
                    return value;
            }
        }

        private object ProcessLessThan(object value)
        {
            if (value == null || OperationParameter == null)
                return false;

            try
            {
                double compareValue = System.Convert.ToDouble(OperationParameter);

                if (value is int intValue)
                    return intValue < compareValue;
                else if (value is double doubleValue)
                    return doubleValue < compareValue;
                else if (value is decimal decimalValue)
                    return (double)decimalValue < compareValue;
                else if (double.TryParse(value.ToString(), out double parsedValue))
                    return parsedValue < compareValue;
            }
            catch
            {
                return false;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}