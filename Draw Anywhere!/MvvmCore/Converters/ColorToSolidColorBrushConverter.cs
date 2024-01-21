using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DrawAnywhere.MvvmCore.Converters
{
    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Color color))
                throw new InvalidOperationException("Value must be a Color");

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported");
        }
    }
}
