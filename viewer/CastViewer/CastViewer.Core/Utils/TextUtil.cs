using System;
using CevioCasts;

namespace CastViewer.Core.Utils;

public static class TextUtil
{
    public static string GetAppCatText(Product app, Category cat)
    {
		var appTxt = app switch
		{
			Product.CeVIO_AI => "AI",
			Product.CeVIO_CS => "CS",
			Product.VoiSona => "VS",
			_ => "?"
		};

		var catTxt = cat switch
		{
			Category.SingerSong => "Song",
			Category.TextVocal => "Talk",
			_ => "?"
		};

		return $"[{appTxt} {catTxt}]";
	}
}