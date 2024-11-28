using CevioCasts;

namespace CastViewer.Converters;

public sealed class ProductToTextConverter : Epoxy.ValueConverter<Product, string>
{
    public override bool TryConvert(Product from, out string result)
    {
        result = from switch
        {
            Product.VoiSona => "VS",
            Product.CeVIO_AI => "AI",
            Product.CeVIO_CS => "CS",
            _ => "Other" // その他
        };
        return true;
    }

}
