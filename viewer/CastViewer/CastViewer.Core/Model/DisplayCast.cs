using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using CevioCasts;
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

	[Category("Song")]
	[Editable(false)]
	[VisibilityPropertyCondition(nameof(Category), CevioCasts.Category.SingerSong)]
	new public BindingList<DisplaySpSymbol>? SpSymbols { get; set; }

	[Category("Emotion")]
	[Editable(false)]
	[VisibilityPropertyCondition(nameof(HasEmotions), true)]
	new public BindingList<DisplayEmotionOrder>? EmotionOrder { get; set; }

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

	public DisplaySpSymbol(SpSymbol instance)
	{
		if (instance is null) { return; }

		Id = instance.Id;
		Names =
        new(instance.Names.Select(v => v.Display).ToList());
		Symbols = [.. instance.Symbols];
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