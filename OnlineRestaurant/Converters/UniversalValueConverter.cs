using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OnlineRestaurant.Converters
{
    public class UniversalValueConverter : IValueConverter
    {
        public enum OperationType
        {
            None,
            LessThan,       // Verifică dacă valoarea este mai mică decât un parametru
            GreaterThan,    // Verifică dacă valoarea este mai mare decât un parametru
            Equals,         // Verifică dacă valoarea este egală cu un parametru
            TruncateText,   // Truncheazã textul la un anumit număr de caractere
            Substring,      // Extrage un substring
            FormatCurrency, // Formatează ca monedă
            FormatNumber    // Formatează ca număr
        }

        // Tipul operației
        public OperationType Operation { get; set; } = OperationType.None;
        
        // Parametru pentru operație (ex. limita pentru comparație sau lungimea pentru truncare)
        public object OperationParameter { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Verifică dacă parametrul specifică operația
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
                
                // Încearcă să extragă parametrul numeric din string
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

            // Procesează valoarea în funcție de operație
            switch (Operation)
            {
                case OperationType.LessThan:
                    return ProcessLessThan(value);
                    
                case OperationType.GreaterThan:
                    return ProcessGreaterThan(value);
                    
                case OperationType.Equals:
                    return ProcessEquals(value);
                    
                case OperationType.TruncateText:
                    return ProcessTruncateText(value);
                    
                case OperationType.Substring:
                    return ProcessSubstring(value);
                    
                case OperationType.FormatCurrency:
                    return ProcessFormatCurrency(value, culture);
                    
                case OperationType.FormatNumber:
                    return ProcessFormatNumber(value, culture);
                    
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

        private object ProcessGreaterThan(object value)
        {
            if (value == null || OperationParameter == null)
                return false;

            try
            {
                double compareValue = System.Convert.ToDouble(OperationParameter);
                
                if (value is int intValue)
                    return intValue > compareValue;
                else if (value is double doubleValue)
                    return doubleValue > compareValue;
                else if (value is decimal decimalValue)
                    return (double)decimalValue > compareValue;
                else if (double.TryParse(value.ToString(), out double parsedValue))
                    return parsedValue > compareValue;
            }
            catch
            {
                return false;
            }
            
            return false;
        }

        private object ProcessEquals(object value)
        {
            if (value == null)
                return OperationParameter == null;

            try
            {
                if (OperationParameter != null && value.GetType() == OperationParameter.GetType())
                    return value.Equals(OperationParameter);
                else if (double.TryParse(value.ToString(), out double valueDouble) && 
                         double.TryParse(OperationParameter?.ToString(), out double paramDouble))
                    return Math.Abs(valueDouble - paramDouble) < 0.0001;
                else
                    return value.ToString().Equals(OperationParameter?.ToString());
            }
            catch
            {
                return false;
            }
        }

        private object ProcessTruncateText(object value)
        {
            if (value == null)
                return string.Empty;
                
            string stringValue = value.ToString();
            int length = 8; // Lungime implicită
            
            if (OperationParameter != null && int.TryParse(OperationParameter.ToString(), out int paramLength))
            {
                length = paramLength;
            }
            
            if (stringValue.Length > length)
            {
                return stringValue.Substring(0, length);
            }
            
            return stringValue;
        }

        private object ProcessSubstring(object value)
        {
            if (value == null)
                return string.Empty;
                
            string stringValue = value.ToString();
            int start = 0;
            int length = stringValue.Length;
            
            if (OperationParameter != null && OperationParameter.ToString().Contains(","))
            {
                string[] parts = OperationParameter.ToString().Split(',');
                if (parts.Length >= 2)
                {
                    if (int.TryParse(parts[0], out int paramStart))
                        start = paramStart;
                        
                    if (int.TryParse(parts[1], out int paramLength))
                        length = paramLength;
                }
            }
            
            if (start < stringValue.Length)
            {
                if (start + length > stringValue.Length)
                    length = stringValue.Length - start;
                    
                return stringValue.Substring(start, length);
            }
            
            return string.Empty;
        }

        private object ProcessFormatCurrency(object value, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
                
            try
            {
                if (value is double doubleValue)
                    return string.Format(culture, "{0:C}", doubleValue);
                else if (value is decimal decimalValue)
                    return string.Format(culture, "{0:C}", decimalValue);
                else if (value is int intValue)
                    return string.Format(culture, "{0:C}", intValue);
                else if (double.TryParse(value.ToString(), out double parsedValue))
                    return string.Format(culture, "{0:C}", parsedValue);
            }
            catch
            {
                return value;
            }
            
            return value;
        }

        private object ProcessFormatNumber(object value, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
                
            try
            {
                string format = "{0:0.##}";
                
                if (OperationParameter != null)
                    format = OperationParameter.ToString();
                    
                if (value is double doubleValue)
                    return string.Format(culture, format, doubleValue);
                else if (value is decimal decimalValue)
                    return string.Format(culture, format, decimalValue);
                else if (value is int intValue)
                    return string.Format(culture, format, intValue);
                else if (double.TryParse(value.ToString(), out double parsedValue))
                    return string.Format(culture, format, parsedValue);
            }
            catch
            {
                return value;
            }
            
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}