using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace RDPro.Converters
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return true;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return false;
        }
    }
}
