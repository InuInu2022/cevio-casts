using System.Collections.ObjectModel;
using System.Globalization;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
namespace CastViewer.Core.Utils;

public static class VocalRangeUtil
{
	private static ReadOnlyCollection<KeyValuePair<string, int>>? rangeTicks;
	public static int GetNoteNumberFromName(string name)
	{
		var isParsed = Note.TryParse(name, out var note);
		if(!isParsed) return -1;

		return note.NoteNumber;
	}

	public static ReadOnlyCollection<KeyValuePair<string, int>>
	GetAllNoteNameTicks()
	{
		if(rangeTicks is null)
		{
			var list = Enumerable
				.Range(SevenBitNumber.MinValue, SevenBitNumber.MaxValue)
				.Select(v => new KeyValuePair<string, int>(
					NoteUtilities
						.GetNoteName((SevenBitNumber)v)
						.ToString()
						.Replace("Sharp","#")
					+ NoteUtilities
						.GetNoteOctave((SevenBitNumber)v)
						.ToString(CultureInfo.InvariantCulture)
					,
					v
				))
				.ToList()
				.AsReadOnly();
			rangeTicks = list;
		}
		return rangeTicks;
	}
}
