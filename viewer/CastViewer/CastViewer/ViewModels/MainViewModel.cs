using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using CastViewer.Core.Model;
using CevioCasts;
using Epoxy;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform;
using System.IO;
using Avalonia.PropertyGrid.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace CastViewer.ViewModels;

[ViewModel]
public class MainViewModel : ViewModelBase
{
	private ImmutableList<DisplayCast>? loadedList;

	public ObservableCollection<Core.Model.DisplayCast> CastList { get; set; }

	public DisplayCast? SelectedCast { get; set; }

	public int SelectedCastIndex { get; set; }
	public Task<DisplayCast> AsyncSelectedCast => GetCastDataAsync();

	public bool IsLoading { get; set; }
	public bool IsPgEnabled { get; set; }
	public string DataVersion { get; } = "x.x.x";

	#region filterbuttons

	public bool IsShowTalkVoice { get; set; } = true;
	public bool IsShowSongVoice { get; set; } = true;
	public bool IsShowCS { get; set; } = true;
	public bool IsShowAI { get; set; } = true;
	public bool IsShowVS { get; set; } = true;

	public Command? CastFilterEvent { get; }


	#endregion filterbuttons

	public Command? CastDataChanged { get; }

	public Pile<PropertyGrid> PgPile { get; }
		= Pile.Factory.Create<PropertyGrid>();

	public ImmutableList<Cast> TestList { get; } =
	[
		new()
		{
			Id = "thisisid1",
			Cname = "thisiscname1",
			Names =
			[
				new() { Lang = Lang.Japanese, Display = "日本語名前1", },
				new() { Lang = Lang.English, Display = "English name 1", },
			],
			Category = Category.SingerSong,
			Product = Product.CeVIO_AI,
			Gender = Gender.Male,
			Lang = Lang.Japanese,
			Versions = ["2.0.0", "1.0.1"],
			HasEmotions = true,
			Emotions =
			[
				new()
				{
					Id = "id1",
					Names =
					[
						new() { Lang = Lang.Japanese, Display = "日本語感情名前1", },
						new() { Lang = Lang.English, Display = "English emo name 1", },
					]
				},
				new()
				{
					Id = "id2",
					Names =
					[
						new() { Lang = Lang.Japanese, Display = "日本語感情名前2", },
						new() { Lang = Lang.English, Display = "English emo name 2", },
					]
				},
			],
			SpSymbols =
			[
				new()
				{
					Id = "sp-id1",
					Symbols = ["@", "＠"],
					Names =
					[
						new() { Lang = Lang.Japanese, Display = "日本語 sp 名前1", },
						new() { Lang = Lang.English, Display = "English sp name 1", },
					],
				},
				new()
				{
					Id = "sp-id2",
					Symbols = ["$", "＄"],
					Names =
					[
						new() { Lang = Lang.Japanese, Display = "日本語 sp 名前2", },
						new() { Lang = Lang.English, Display = "English sp name 2", },
					],
				}
			],
		},
		new()
		{
			Id = "thisisid2",
			Cname = "thisiscname2",
			Names =
			[
				new() { Lang = Lang.Japanese, Display = "日本語名前2", },
				new() { Lang = Lang.English, Display = "English name 2", },
			],
			Category = Category.TextVocal,
			Product = Product.VoiSona,
			Gender = Gender.Female,
			Lang = Lang.English,
			Versions = ["2.0.0", "1.1.0", "1.0.1 ported from CeVIO AI"],
			HasEmotions = true,
			Emotions =
			[
				new()
				{
					Id = "id1",
					Names =
					[
						new() { Lang = Lang.Japanese, Display = "日本語感情名前1", },
						new() { Lang = Lang.English, Display = "English emo name 1", },
					]
				},
				new()
				{
					Id = "id2",
					Names =
					[
						new() { Lang = Lang.Japanese, Display = "日本語感情名前2", },
						new() { Lang = Lang.English, Display = "English emo name 2", },
					]
				},
				new()
				{
					Id = "id3",
					Names =
					[
						new() { Lang = Lang.Japanese, Display = "日本語感情名前3", },
						new() { Lang = Lang.English, Display = "English emo name 3", },
					]
				},
				new()
				{
					Id = "id4",
					Names =
					[
						new() { Lang = Lang.Japanese, Display = "日本語感情名前4", },
						new() { Lang = Lang.English, Display = "English emo name 4", },
					]
				},
			],
			EmotionOrder =
			[
				new() { Version = "2.0.0", Order = ["id2", "id1", "id3", "id4"], },
				new() { Version = "1.1.0", Order = ["id1", "id2", "id3", "id4"], }
			],
		},
	];

	public MainViewModel()
	{
		var list = TestList
			.Select<Cast, DisplayCast>(v => new(v))
			.ToImmutableList();
		CastList = new(list);

		using var fs = new StreamReader(AssetLoader.Open(new Uri("avares://CastViewer/Assets/data.json")));
		string jsonString = fs.ReadToEnd();
		var definitions = Definitions.FromJson(jsonString, true);
		loadedList = definitions
			.Casts
			.Select<Cast, DisplayCast>(v => new(v))
			.ToImmutableList();
		CastList = new(loadedList);
		DataVersion = definitions.Version;

		CastFilterEvent = Command.Factory.Create(()=>{
			var filterd = loadedList
				.Where(v =>
					(IsShowSongVoice || v.Category != Category.SingerSong)
						&& (IsShowTalkVoice || v.Category != Category.TextVocal)
						&& (IsShowCS || v.Product != Product.CeVIO_CS)
						&& (IsShowAI || v.Product != Product.CeVIO_AI)
						&& (IsShowVS || v.Product != Product.VoiSona)
				)
				;
			CastList.Clear();
			CastList = new(filterd);
			SelectedCastIndex = 0;
			return default;
		});

		IsPgEnabled = true;
	}

	private Task<DisplayCast> GetCastDataAsync()
	{
		IsLoading = true;

		return Task.Run(() => {
			var cast = CastList[SelectedCastIndex];
			if(cast is null)
			{
				IsLoading = false;
				return new DisplayCast(null);
			}

			IsLoading = false;
			return cast;
		});
	}

	/*
	[PropertyChanged(nameof(SelectedCast))]
	[SuppressMessage("","IDE0051")]
	private async ValueTask SelectedCastChangedAsync(DisplayCast value)
	{
		if(PgPile is null){return;}

		IsLoading = true;
		try
		{
			await PgPile.RentAsync(async pg =>
			{
				await Task.Delay(300);
				pg.DataContext = value;
				await Task.Delay(500);
			});
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}

		IsLoading = false;

		//return default;
	}
	*/
}
