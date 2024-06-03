using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CevioCasts;

#if NET8_0_OR_GREATER

public partial class Definitions
{
	internal static JsonSerializerOptions sourceGenOptions = new(Converter.Settings)
	{
		TypeInfoResolver = SourceGenerationContext.Default,
	};

	[SuppressMessage("","IL3050")]
	[SuppressMessage("","IL2026")]
	public static Definitions FromJson(string json, bool useSourceGen)
	{
		if(!useSourceGen)
		{
			return FromJson(json);
		}

		var ret = JsonSerializer
			.Deserialize(
				json,
				typeof(Definitions),
				sourceGenOptions) as Definitions;
		return ret ?? new Definitions();
	}
}

[SuppressMessage("","RCS1060")]
[SuppressMessage("","MA0048")]
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Definitions))]
internal sealed partial class SourceGenerationContext : JsonSerializerContext;

#endif