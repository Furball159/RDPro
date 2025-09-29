using System;
using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;

namespace RDPro.Views.Converters
{
    public class ConnectionNameConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            var t = value.GetType();
            // Prefer ConnectionName (friendly name). If not present, fall back to ServerAddress (the actual host), then Name.
            var prop = t.GetProperty("ConnectionName") ?? t.GetProperty("ServerAddress") ?? t.GetProperty("Name");
            if (prop != null)
            {
                var v = prop.GetValue(value);
                return v?.ToString() ?? string.Empty;
            }
            return value.ToString() ?? string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConnectionAddressConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            var t = value.GetType();
            var prop = t.GetProperty("ServerAddress") ?? t.GetProperty("Address");
            if (prop != null)
            {
                var v = prop.GetValue(value);
                return v?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
