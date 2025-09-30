using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace RDPro.Converters;

public class ColorToHexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        return "#FFFFFFFF"; // fallback white
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && Avalonia.Media.Color.TryParse(s, out var color))
        {
            return color;
        }
        return Colors.White;
    }
}
