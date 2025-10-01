using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace RDPro.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        // If Invert==false: returns true when string is not null or whitespace
        // If Invert==true: returns true when string is null or whitespace
        public bool Invert { get; set; } = false;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool result = false;
            if (value is string s)
            {
                result = !string.IsNullOrWhiteSpace(s);
            }
            if (Invert) result = !result;
            return result;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
