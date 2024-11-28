using CevioCasts;

namespace CastViewer.Converters;

public sealed class CategoryToTextConverter : Epoxy.ValueConverter<Category, string>
{
    public override bool TryConvert(Category from, out string result)
    {
        result = from switch
        {
            Category.TextVocal => "Talk",
            Category.SingerSong => "Song",
            _ => from.ToString() // その他の場合はそのまま
        };
        return true;
    }

}
