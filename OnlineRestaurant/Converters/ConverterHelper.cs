using System.Windows.Data;

namespace OnlineRestaurant.Converters
{
    public static class ConverterHelper
    {
        private static UniversalVisibilityConverter _visibilityConverter;

        public static UniversalVisibilityConverter VisibilityConverter =>
            _visibilityConverter ?? (_visibilityConverter = new UniversalVisibilityConverter());

        private static UniversalOrderStatusConverter _orderStatusConverter;

        public static UniversalOrderStatusConverter OrderStatusConverter =>
            _orderStatusConverter ?? (_orderStatusConverter = new UniversalOrderStatusConverter());

        private static UniversalValueConverter _valueConverter;

        public static UniversalValueConverter ValueConverter =>
            _valueConverter ?? (_valueConverter = new UniversalValueConverter());

        private static DefaultImageConverter _imageConverter;

        public static DefaultImageConverter ImageConverter =>
            _imageConverter ?? (_imageConverter = new DefaultImageConverter { DefaultImagePath = "/Images/default.jpg" });

        public static IValueConverter LessThanConverter =>
            new UniversalValueConverter { Operation = UniversalValueConverter.OperationType.LessThan };

        public static IValueConverter StringVisibilityConverter =>
            new UniversalVisibilityConverter { Mode = UniversalVisibilityConverter.ConversionMode.String };

        public static IValueConverter BooleanVisibilityConverter =>
            new UniversalVisibilityConverter { Mode = UniversalVisibilityConverter.ConversionMode.Boolean };

        public static IValueConverter InvertedBooleanVisibilityConverter =>
            new UniversalVisibilityConverter
            {
                Mode = UniversalVisibilityConverter.ConversionMode.Boolean,
                Invert = true
            };

        public static IValueConverter NullVisibilityConverter =>
            new UniversalVisibilityConverter { Mode = UniversalVisibilityConverter.ConversionMode.Null };

        public static IValueConverter OrderStatusStringConverter =>
            new UniversalOrderStatusConverter { Output = UniversalOrderStatusConverter.OutputType.String };

        public static IValueConverter OrderStatusColorConverter =>
            new UniversalOrderStatusConverter { Output = UniversalOrderStatusConverter.OutputType.Color };

        public static IValueConverter OrderStatusVisibilityConverter =>
            new UniversalOrderStatusConverter { Output = UniversalOrderStatusConverter.OutputType.Visibility };
    }
}