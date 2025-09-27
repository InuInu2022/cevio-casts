using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using CastViewer.Core.Model;
using CevioCasts;

namespace CastViewer.Converters;

public class ProductFilterConverter : IMultiValueConverter
{
    public static readonly ProductFilterConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2)
            return null;

        if (values[0] is not ObservableCollection<DisplayCast> castList)
            return null;

        if (values[1] is not string productString)
            return null;

        if (!Enum.TryParse<Product>(productString, out var product))
            return null;

        return castList.Where(cast => cast.Product == product).ToList();
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}