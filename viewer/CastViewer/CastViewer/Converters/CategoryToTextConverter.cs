

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using CevioCasts;

namespace CastViewer.Converters;

public class CategoryToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Category v) return value ?? "";
        return v switch
        {
            Category.TextVocal => "Talk",
            Category.SingerSong => "Song",
            _ => value // その他の場合はそのまま
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
