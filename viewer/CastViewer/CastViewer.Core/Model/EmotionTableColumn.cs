using System.ComponentModel;
using CevioCasts;

namespace CastViewer.Core.Model;

public record EmotionTableColumn
{
	public string Category { get; }
	public string CastName { get; }
	public Product Product { get; }
	public int EmotionCount { get; }
	public string Emo1 { get; } = string.Empty;
	public string Emo2 { get; } = string.Empty;
	public string Emo3 { get; } = string.Empty;
	public string Emo4 { get; } = string.Empty;
	public string Emo5 { get; } = string.Empty;
	public string Emo6 { get; } = string.Empty;
	public string Emo7 { get; } = string.Empty;
	public string Emo8 { get; } = string.Empty;

	public EmotionTableColumn(
		string castName,
		Category category,
		Product product,
		BindingList<DisplayEmotion> emotions
	)
	{
		Category = category switch
		{
			CevioCasts.Category.SingerSong => "Song",
			CevioCasts.Category.TextVocal => "Talk",
			_ => "ERROR"
		};
		CastName = castName;
		this.Product = product;

		if (emotions.Count == 0) { return; }

		int length = emotions.Count;
		EmotionCount = length;

		this.Emo1 = emotions?[0]?.Names?[0] ?? "NO NAME";
		if(length < 2) return;
		this.Emo2 = emotions?[1]?.Names?[0] ?? "NO NAME";
		if(length < 3) return;
		this.Emo3 = emotions?[2]?.Names?[0] ?? "NO NAME";
		if(length < 4) return;
		this.Emo4 = emotions?[3]?.Names?[0] ?? "NO NAME";
		if(length < 5) return;
		this.Emo5 = emotions?[4]?.Names?[0] ?? "NO NAME";
		if(length < 6) return;
		this.Emo6 = emotions?[5]?.Names?[0] ?? "NO NAME";
		if(length < 7) return;
		this.Emo7 = emotions?[6]?.Names?[0] ?? "NO NAME";
		if(length < 8) return;
		this.Emo8 = emotions?[7]?.Names?[0] ?? "NO NAME";
	}
}
