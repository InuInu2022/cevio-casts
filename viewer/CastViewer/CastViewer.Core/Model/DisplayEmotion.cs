using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using CevioCasts;

namespace CastViewer.Core.Model;

[Category("Emotion")]
[TypeConverter(typeof(ExpandableObjectConverter))]
public class DisplayEmotion(Emotion instance) : Emotion
{
    [Category("Emotion")]
    [Editable(false)]
    new public string Id { get; set; } = instance.Id;

    [Category("Emotion")]
    [Editable(false)]
    new public BindingList<string> Names { get; set; } =
        new(instance.Names.Select(v => v.Display).ToList());
}