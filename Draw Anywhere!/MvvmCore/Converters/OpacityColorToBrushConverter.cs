using System;
using System.Globalization;
using System.Windows.Data;
using DrawAnywhere.ViewModels.Helpers;

namespace DrawAnywhere.MvvmCore.Converters
{
    internal class OpacityColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OpacityColor opacityColor)
            {
                return opacityColor.ToBrush();
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
