using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RDPro.Converters
{
    public class IsFavouriteToHeaderConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return "Remove from Favourites";
            return "Add to Favourites";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
