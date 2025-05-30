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
