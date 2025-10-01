using Avalonia.Data.Converters;
using Avalonia.Controls;
using System;
using System.Globalization;

namespace RDPro.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return true; // In Avalonia, bind to bool directly for IsVisible
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b;
        return false;
    }
}
