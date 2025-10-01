using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RDPro.Converters;

public class IntToBoolConverter : IValueConverter
{
    public int CompareTo { get; set; }
    public bool GreaterThan { get; set; } = false;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intVal)
        {
            if (GreaterThan)
                return intVal > CompareTo;
            else
                return intVal == CompareTo;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
