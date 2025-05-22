using System.Windows.Data;

namespace OnlineRestaurant.Converters
{
    public static class ConverterHelper
    {
        // Singleton pentru UniversalVisibilityConverter
        private static UniversalVisibilityConverter _visibilityConverter;
        public static UniversalVisibilityConverter VisibilityConverter => 
            _visibilityConverter ?? (_visibilityConverter = new UniversalVisibilityConverter());
            
        // Singleton pentru UniversalOrderStatusConverter
        private static UniversalOrderStatusConverter _orderStatusConverter;
        public static UniversalOrderStatusConverter OrderStatusConverter =>
            _orderStatusConverter ?? (_orderStatusConverter = new UniversalOrderStatusConverter());
            
        // Singleton pentru DefaultImageConverter
        private static DefaultImageConverter _imageConverter;
        public static DefaultImageConverter ImageConverter =>
            _imageConverter ?? (_imageConverter = new DefaultImageConverter { DefaultImagePath = "/Images/default.jpg" });
            
        // Singleton pentru IdTruncateConverter
        private static IdTruncateConverter _idTruncateConverter;
        public static IdTruncateConverter IdTruncateConverter =>
            _idTruncateConverter ?? (_idTruncateConverter = new IdTruncateConverter());
            
        // Metode utilitare pentru configurarea rapidă a converterelor
        
        // UniversalVisibilityConverter configurat pentru string-uri
        public static IValueConverter StringVisibilityConverter => 
            new UniversalVisibilityConverter { Mode = UniversalVisibilityConverter.ConversionMode.String };
            
        // UniversalVisibilityConverter configurat pentru valori boolean
        public static IValueConverter BooleanVisibilityConverter =>
            new UniversalVisibilityConverter { Mode = UniversalVisibilityConverter.ConversionMode.Boolean };
            
        // UniversalVisibilityConverter configurat pentru valori boolean inversate
        public static IValueConverter InvertedBooleanVisibilityConverter =>
            new UniversalVisibilityConverter { 
                Mode = UniversalVisibilityConverter.ConversionMode.Boolean,
                Invert = true
            };
            
        // UniversalVisibilityConverter configurat pentru null check
        public static IValueConverter NullVisibilityConverter =>
            new UniversalVisibilityConverter { Mode = UniversalVisibilityConverter.ConversionMode.Null };
            
        // UniversalOrderStatusConverter configurat pentru string
        public static IValueConverter OrderStatusStringConverter =>
            new UniversalOrderStatusConverter { Output = UniversalOrderStatusConverter.OutputType.String };
            
        // UniversalOrderStatusConverter configurat pentru culoare
        public static IValueConverter OrderStatusColorConverter =>
            new UniversalOrderStatusConverter { Output = UniversalOrderStatusConverter.OutputType.Color };
            
        // UniversalOrderStatusConverter configurat pentru vizibilitate
        public static IValueConverter OrderStatusVisibilityConverter =>
            new UniversalOrderStatusConverter { Output = UniversalOrderStatusConverter.OutputType.Visibility };
    }
} 