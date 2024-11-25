

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using CevioCasts;

namespace CastViewer.Converters;

public class ProductToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Product p) return value ?? "";
        return p switch
        {
            Product.VoiSona => "VS",
            Product.CeVIO_AI => "AI",
            Product.CeVIO_CS => "CS",
            _ => "Other" // その他
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
