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
using ScottPlot.Avalonia;
using Avalonia.Controls;
using ScottPlot;
using ScottPlot.TickGenerators;
using System.Collections.Generic;

namespace CastViewer.ViewModels;

[ViewModel]
public class MainViewModel : ViewModelBase
{
	public IEnumerable<Cast> RawCastList { get; private set; }

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


	// ------------- plot tab --------------- //
	public TabItem? SelectedPlotTab { get; set; }
	public Pile<AvaPlot> TempoPlotPile { get; } = Pile.Factory.Create<AvaPlot>();
	public Pile<AvaPlot> RangePlotPile { get; } = Pile.Factory.Create<AvaPlot>();

	public MainViewModel()
	{
		var list = TestList
			.Select<Cast, DisplayCast>(v => new(v))
			.ToImmutableList();
		CastList = new(list);

		using var fs = new StreamReader(AssetLoader.Open(new Uri("avares://CastViewer/Assets/data.json")));
		string jsonString = fs.ReadToEnd();
		var definitions = Definitions.FromJson(jsonString, true);
		RawCastList = definitions.Casts;
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

	private ValueTask ShowTempoTabAsync(AvaPlot avaPlot)
	{
		var targets = RawCastList
			//.Where(c => c.IsShowTempo)
			.Where(c => c.Category == Category.SingerSong)
			.Where(c => c.VocalTempo is not null)
			.OrderBy(c => c.VocalTempo!.High)
			.ThenByDescending(c => c.VocalTempo!.Low)
			;
		Tick[] ticks =
		{
			new(1, "Apple"),
			new(2, "Orange"),
			new(3, "Pear"),
			new(4, "Banana"),
		};
		if(CastList is not null)
		{
			ticks = targets
				.Select((c, i) => new Tick(i, c.Names[1].Display))
				.ToArray()
				;
		}
		avaPlot.Plot.Axes.Left.TickGenerator = new NumericManual(ticks);
		avaPlot.Plot.Axes.Left.MajorTickStyle.Length = 0;
		avaPlot.Plot.Axes.Left.Label.FontName = "Noto Sans CJK";
		avaPlot.Plot.Axes.Title.Label.Text = "Recommended vocal tempo";

		avaPlot.Plot.Axes.Color(Color.FromHex("#a0acb5"));
		avaPlot.Plot.Grid.MajorLineColor = Color.FromHex("#0e3d54");
		avaPlot.Plot.FigureBackground.Color = Color.FromHex("#07263b");
		avaPlot.Plot.DataBackground.Color = Color.FromHex("#0b3049");
		//avaPlot.Plot.Font.Automatic();
		//avaPlot.Plot.Add.Palette = new ScottPlot.Palettes.Penumbra();

		var fontName = Fonts.Detect("日本語あいうえお");

		ScottPlot.Bar[] bars =
		{
			new() { Position = 1, Value = 5, ValueBase = 3, FillColor = Colors.Red },
			new() { Position = 2, Value = 7, ValueBase = 0, FillColor = Colors.Blue },
			new() { Position = 4, Value = 3, ValueBase = 2, FillColor = Colors.Green },
		};
		if(CastList is not null)
		{
			bars = targets
				.Select((c,i) => new Bar()
				{
					Label = $"{c.VocalTempo.Low} - {c.VocalTempo.High}",
					Position = i,
					ValueBase = c.VocalTempo?.Low ?? 0,
					Value = c.VocalTempo?.High ?? 0,
					FillColor = GetColor(c.Product),
					BorderLineWidth = 0,
					//BorderColor = Colors.Gray,
				})
				.ToArray()
				;

		}
		var barPlot = avaPlot.Plot.Add.Bars(bars);
		barPlot.Horizontal = true;
		barPlot.ValueLabelStyle.ForeColor = Color.FromHex("#a0acb5");

		// build the legend manually
		var legend = avaPlot.Plot.Legend;
		legend.IsVisible = true;
		legend.Alignment = Alignment.LowerRight;
		legend.ManualItems = [
			new(){LabelText = Product.CeVIO_AI.ToString(), FillColor = Colors.LightSalmon},
			new(){LabelText = Product.VoiSona.ToString(), FillColor = Colors.LightBlue},
			new(){LabelText = Product.CeVIO_CS.ToString(), FillColor = Colors.Gray},
		];

		return default;

		static Color GetColor(Product product)
		{
			return product switch
			{
				Product.CeVIO_AI => ScottPlot.Colors.LightSalmon,
				Product.VoiSona => ScottPlot.Colors.LightBlue,
				_ => Colors.Gray,
			};
		}
	}

	[PropertyChanged(nameof(SelectedPlotTab))]
	[SuppressMessage("","IDE0051")]
	private async ValueTask SelectedPlotTabChangedAsync(TabItem value)
	{
		if(value is null) return;

		switch(value.Name){
			case "TempoPlotTab":
				if(value.Content is AvaPlot plot)
				{
					await ShowTempoTabAsync(plot);
				}
				break;
			default:
				break;
		}

		//return default;
	}
}
