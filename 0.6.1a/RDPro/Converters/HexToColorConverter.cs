using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace RDPro.Converters
{
    public class HexToColorConverter : IValueConverter
    {
        public static readonly HexToColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string hex && Color.TryParse(hex, out var c))
                return c;

            return Colors.DodgerBlue; // fallback
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color c)
                return $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";

            return "#FF0078D7"; // fallback Windows blue
        }
    }
}
