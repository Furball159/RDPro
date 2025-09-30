using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RDPro.Converters
{
    public class CountToBoolConverter : IValueConverter
    {
        public bool Invert { get; set; } = false;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                var result = count > 0;
                return Invert ? !result : result;
            }

            return Invert; // fallback
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
