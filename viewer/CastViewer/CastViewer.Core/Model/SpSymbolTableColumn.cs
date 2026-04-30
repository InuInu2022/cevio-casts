using CevioCasts;

namespace CastViewer.Core.Model;

public record SpSymbolTableColumn(
	Category Cat,
	string CastName,
	Product Product,
	IEnumerable<DisplaySpSymbol> SpSymbols
)
{
	public string Category =>
		Cat switch
		{
			CevioCasts.Category.SingerSong => "Song",
			CevioCasts.Category.TextVocal => "Talk",
			_ => "ERROR",
		};
	public string? LabelFalsetto => GetName(SpSymbols, ["$", "＄", "※"]);
	public string? LabelAtmark => GetName(SpSymbols, ["@", "＠"]);
	public string? LabelCaret => GetName(SpSymbols, ["^", "＾"]);
	public string? LabelPercent => GetName(SpSymbols, ["%", "％"]);
	public string? LabelEqual => GetName(SpSymbols, ["=", "＝"]);
	public string? LabelUnderscore => GetName(SpSymbols, ["_", "＿"]);
	public string? LabelPlus => GetName(SpSymbols, ["+", "＋"]);
	public string? LabelEdgeVoice => GetName(SpSymbols, ["(E)"]);
	public string? LabelEdgeEnd => GetName(SpSymbols, ["(D)"]);
	public string? LabelPitchFlipUp => GetName(SpSymbols, ["(P)"]);
	public string? LabelStrained => GetName(SpSymbols, ["(I)"]);
	public string? LabelTailFall => GetName(SpSymbols, ["(M)"]);
	public string? LabelExhale => GetName(SpSymbols, ["(H)"]);
	public string? LabelHeadFall => GetName(SpSymbols, ["(S)"]);

	private static string GetName(
		IEnumerable<DisplaySpSymbol> spSymbols,
		IReadOnlyList<string> labels
	)
	{
		var symbol = spSymbols
			.FirstOrDefault(s => s.Symbols?
				.Any(v => labels.Contains(v)) == true);
		var label = symbol?.Names?[0] ?? string.Empty;
		var version =
			symbol?.Versions?.Count == 1
				? symbol?.Versions?.LastOrDefault() ?? string.Empty
				: string.Empty;
		return (version is []) ? label : $"{label} ({version})";
	}
}
