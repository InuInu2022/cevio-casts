using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;
using CevioCasts;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;

namespace CastViewer.Core.Model;

public sealed class DisplayCast : Cast
{
	[Category("Basic")]
	new public string? Id { get; set; }
	[Category("Basic")]
	new public string? Cname { get; set; }
	[Category("Basic")]
	[ConditionTarget]
	new public Category Category { get; set; }

	[Category("Basic")]
	new public Product Product { get; set; }
	[Category("Basic")]
	new public Gender Gender { get; set; }
	[Category("Basic")]
	new public Lang Lang { get; set; }

	[Category("Basic")]
	[Editable(false)]
	new public BindingList<string>? Names { get; set; }

	[Category("Basic")]
	[Editable(false)]
	new public BindingList<string>? Versions { get; set; }

	[Category("Emotion")]
	[Editable(false)]
	[ReadOnly(true)]
	[ConditionTarget]
	new public bool HasEmotions { get; set; }

	[Category("Emotion")]
	[Editable(false)]
	[VisibilityPropertyCondition(nameof(HasEmotions), true)]
	new public BindingList<DisplayEmotion>? Emotions { get; set; }

	[Category("Emotion")]
	[Editable(false)]
	[VisibilityPropertyCondition(nameof(HasEmotions), true)]
	new public BindingList<DisplayEmotionOrder>? EmotionOrder { get; set; }

	[Category("Song")]
	[VisibilityPropertyCondition(nameof(IsShowRange), true)]
	new public DisplayVocalRange? VocalRange { get; set; }

	[Category("Song")]
	[VisibilityPropertyCondition(nameof(IsShowTempo), true)]
	new public DisplayVocalTempo? VocalTempo { get; set; }

	[Category("Song")]
	[Editable(false)]
	[VisibilityPropertyCondition(nameof(Category), CevioCasts.Category.SingerSong)]
	new public BindingList<DisplaySpSymbol>? SpSymbols { get; set; }





	[ConditionTarget]
	[IgnoreDataMember]
	[DependsOnProperty(nameof(Category))]
	public bool IsShowRange { get; set; }

	[ConditionTarget]
	[IgnoreDataMember]
	[DependsOnProperty(nameof(Category))]
	public bool IsShowTempo { get; set; } = false;

	public DisplayCast(Cast? instance)
	{
		if(instance is null){ return; }

		// リフレクションを使用してプロパティをコピー
		foreach (PropertyInfo property in typeof(Cast).GetProperties())
		{
			var value = property?.GetValue(instance);
			property?.SetValue(this, value);
		}

		Id = instance.Id ?? "ID NOT FOUND";
		Cname = instance.Cname ?? "CNAME NOT FOUND";
		Product = instance.Product;
		Gender = instance.Gender;
		Lang = instance.Lang;
		Names = new(instance.Names.Select(v => v.Display).ToList());
		Versions = new(instance.Versions);
		Category = instance.Category;
		HasEmotions = instance.HasEmotions;

		if(instance.VocalRange is not null)
		{
			IsShowRange =
				Category == CevioCasts.Category.SingerSong &&
				instance.VocalRange.High != "" && instance.VocalRange.Low != "";
			VocalRange = new(instance.VocalRange);
		}
		if(instance.VocalTempo is not null)
		{
			IsShowTempo =
				Category == CevioCasts.Category.SingerSong &&
				instance.VocalTempo.High != 0 && instance.VocalTempo.Low != 0;
			VocalTempo = new(instance.VocalTempo);
		}

		var emotions = (instance?.Emotions ?? [])
			.Select<Emotion, DisplayEmotion>(v => new(v));
		Emotions = [ .. emotions ];

		var sbs = instance?.SpSymbols ?? [];
		var symbols = sbs
			.Select<SpSymbol, DisplaySpSymbol>(v => new(v));
		SpSymbols = [.. symbols];

		var orders = instance?.EmotionOrder is null
		? []
		: instance.EmotionOrder
			.Where(item => item is not null)
			.ToList()
			.Select<EmotionOrder, DisplayEmotionOrder>(v => new(v))
			;
		EmotionOrder = orders is null
			? []
			: [.. orders];
	}
}

[Category("Song")]
[TypeConverter(typeof(ExpandableObjectConverter))]
public class DisplaySpSymbol
{
	[Category("Song")]
	[Editable(false)]
	public string? Id { get; set; }

	[Category("Song")]
	[Editable(false)]
    public BindingList<string>? Names { get; set; }

	[Category("Song")]
	[Editable(false)]
	public BindingList<string>? Symbols { get; set; }

	[Category("Song")]
	[Editable(false)]
	public BindingList<string>? Versions { get; set; }

	public DisplaySpSymbol(SpSymbol instance)
	{
		if (instance is null) { return; }

		Id = instance.Id;
		Names =
        new(instance.Names.Select(v => v.Display).ToList());
		Symbols = [.. instance.Symbols];
		Versions = instance.Versions is null
			? []
			: [.. instance.Versions];
	}

	public override string ToString()
	{
		return $"{Id}: {string.Join(", ", Names ?? [])}, {string.Join("|", Symbols ?? [])}";
	}
}

[Category("Emotion")]
[TypeConverter(typeof(ExpandableObjectConverter))]
public class DisplayEmotionOrder
{
	[Category("Emotion")]
	[Editable(false)]
	public string? Version { get; set; }

	[Category("Emotion")]
	[Editable(false)]
	public BindingList<string>? Order { get; set; }

	public DisplayEmotionOrder(EmotionOrder instance)
	{
		if (instance is null) { return; }

		Version = instance.Version;
		Order = [..instance.Order];
	}
}

[Category("Song")]
[TypeConverter(typeof(ExpandableObjectConverter))]
public class DisplayVocalRange
{
	public DisplayVocalRange(VocalRange instance)
	{
		if (instance is null) { return; }

		High = instance.High;
		Low = instance.Low;
	}

	[Category("Song")]
	[Editable(false)]
	public string? High { get; set; } = "UNKNOWN";

	[Category("Song")]
	[Editable(false)]
	[Watermark("low vocal range")]
	public string? Low { get; set; } = "UNKNOWN";
}

[Category("Song")]
[TypeConverter(typeof(ExpandableObjectConverter))]
public class DisplayVocalTempo
{
	public DisplayVocalTempo(VocalTempo instance)
	{
		if (instance is null) { return; }

		High = instance.High;
		Low = instance.Low;
		IsShowTempo = High > 0 && Low > 0 && High > Low;
	}

	[ConditionTarget]
	[IgnoreDataMember]
	public bool IsShowTempo { get; set; } = false;

	[Category("Song")]
	[Editable(false)]
	[VisibilityPropertyCondition(nameof(IsShowTempo), true)]
	[ProgressAttribute(0,250,"{0:G} bpm")]
	public double High { get; set; }

	[Category("Song")]
	[Editable(false)]
	[VisibilityPropertyCondition(nameof(IsShowTempo), true)]
	[ProgressAttribute(0,250,"{0:G} bpm")]
	public double Low { get; set; }
}