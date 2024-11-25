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
using CastViewer.Core.Utils;
using Avalonia.Collections;
using Avalonia.VisualTree;

namespace CastViewer.ViewModels;

[ViewModel]
public class MainViewModel : ViewModelBase
{
	public string WindowTitle { get; set; } = "C v0.0.0";
	public IEnumerable<Cast> RawCastList { get; private set; }

	private readonly ImmutableList<DisplayCast>? loadedList;

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

	// ------------- mode tab --------------- //
	public TabItem? SelectedModeTab { get; set; }
	public int SelectedModeTabIndex { get; set; }

	// ------------- plot tab --------------- //
	public TabItem? SelectedPlotTab { get; set; }
	public Pile<AvaPlot> TempoPlotPile { get; } = Pile.Factory.Create<AvaPlot>();
	public Pile<AvaPlot> RangePlotPile { get; } = Pile.Factory.Create<AvaPlot>();
	public Pile<AvaPlot> TempoRangePlotPile { get; } = Pile.Factory.Create<AvaPlot>();
	public Pile<AvaPlot> EmotionsPlotPile { get; } = Pile.Factory.Create<AvaPlot>();

	public int SelectedTempoSortIndex { get; set; }
	public int SelectedRangeSortIndex { get; set; }

	// ------------- table tab --------------- //
	public TabItem? SelectedTableTab { get; set; }
	public ObservableCollection<EmotionTableColumn>? EmotionTableCastList { get; set; } = [];
	public ObservableCollection<SpSymbolTableColumn>? SpSymbolTableCastList { get; private set; } = [];

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
		WindowTitle = $"CastViewer ver.{DataVersion}";

		CastFilterEvent = Command.Factory.Create(async ()=>{
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
			await RedrawAsync();
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
		var targets = CastList
			//.Where(c => c.IsShowTempo)
			.Where(c => c is { Category: Category.SingerSong, VocalTempo: not null })
			.OrderBy(c => c.VocalTempo!.High)
			.ThenByDescending(c => c.VocalTempo!.Low)
			.ThenBy(c => c.Names![1])
			;
		Tick[] ticks = [];
		if(CastList is not null)
		{
			ticks = targets
				.Select((c, i) => new Tick(i, c.Names![1]))
				.ToArray()
				;
		}

		ScottPlot.Bar[] bars = [];
		if(CastList is not null)
		{
			bars = targets
				.Select((c,i) => new Bar()
					{
						Label = $"{c.VocalTempo?.Low} - {c.VocalTempo?.High}",
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



		SetPlot(
			avaPlot,
			bars,
			//valueTicks: rangeTicks,
			title: "Recommended vocal tempo (bpm)",
			castTicks: ticks,
			isHorizontal: true);

		return default;
	}

	private ValueTask ShowRangeTabAsync(AvaPlot avaPlot)
	{
		var targets = CastList
			//.Where(c => c.IsShowTempo)
			.Where(c => c is { Category: Category.SingerSong, VocalRange: not null })
			.OrderBy(c => VocalRangeUtil.GetNoteNumberFromName(c.VocalRange?.High ?? "C4"))
			.ThenBy(c => VocalRangeUtil.GetNoteNumberFromName(c.VocalRange?.Low ?? "C4"))
			.ThenBy(c => c.Names![1])
			;

		Tick[] castTicks = targets
			.Select((c, i) => new Tick(i, c.Names![1]))
			.ToArray()
			;

		Tick[] rangeTicks = VocalRangeUtil
			.GetAllNoteNameTicks()
			.Select(t => new Tick(t.Value, t.Key))
			.ToArray();

		ScottPlot.Bar[] bars =
		[
			new() { Position = 1, Value = 5, ValueBase = 3, FillColor = Colors.Red },
			new() { Position = 2, Value = 7, ValueBase = 0, FillColor = Colors.Blue },
			new() { Position = 4, Value = 3, ValueBase = 2, FillColor = Colors.Green },
		];
		bars = targets
			.Select((c,i) => new Bar()
				{
					Label = $"{c.VocalRange?.Low} - {c.VocalRange?.High}",
					Position = i,
					ValueBase = VocalRangeUtil.GetNoteNumberFromName(c.VocalRange?.Low ?? "C4"),
					Value = VocalRangeUtil.GetNoteNumberFromName(c.VocalRange?.High ?? "C4"),
					FillColor = GetColor(c.Product),
					BorderLineWidth = 0,
					//BorderColor = Colors.Gray,
				})
			.ToArray()
			;

		SetPlot(
			avaPlot,
			bars,
			title: "Recommended vocal range",
			castTicks: castTicks,
			valueTicks: rangeTicks,
			isHorizontal: true);

		return default;
	}

	private ValueTask ShowTempoRangeTabAsync(AvaPlot avaPlot)
	{
		var targets = CastList
			//.Where(c => c.IsShowTempo)
			.Where(c => c is { Category: Category.SingerSong, VocalRange: not null, VocalTempo: not null })
			.OrderBy(c => c.Names![0])
			;

		Tick[] rangeTicks = VocalRangeUtil
			.GetAllNoteNameTicks()
			.Select(t => new Tick(t.Value, t.Key))
			.ToArray();

		CoordinateRect[] rects = targets
			.Select((c, _) => new CoordinateRect(
				VocalRangeUtil.GetNoteNumberFromName(c.VocalRange?.Low ?? "C4"),
				VocalRangeUtil.GetNoteNumberFromName(c.VocalRange?.High ?? "C4"),
				c.VocalTempo?.Low ?? 50,
				c.VocalTempo?.High ?? 300
			))
			.Distinct()
			.ToArray();

		SetPlot(
			avaPlot,
			rects,
			title: "Recommended vocal tempo & range area",
			//castTicks: castTicks,
			valueTicks: rangeTicks,
			isHorizontal: true,
			isShowLegend: false);

		var texts = targets
			.Select(c => (
				cast: c.Names![1],
				rect: (
					left: VocalRangeUtil.GetNoteNumberFromName(c.VocalRange?.Low ?? "C4"),
					//VocalRangeUtil.GetNoteNumberFromName(c.VocalRange.High),
					//c.VocalTempo.Low,
					top: c.VocalTempo?.High ?? 300
					)))
			.GroupBy(v => v.rect)
			.Select(v => (
				casts: string.Join(", ", v.Select(t => t.cast).Distinct()),
				rect: v.Key
			))
			;
		foreach (var text in texts)
		{
			var textPlot = avaPlot.Plot.Add.Text(text.casts, text.rect.left, text.rect.top);
			textPlot.LabelFontColor = Colors.Yellow;
		}

		return default;
	}

	private async ValueTask ShowEmotionsTabAsync(AvaPlot avaPlot)
	{
		var targets = CastList
			.Where(c => c.Emotions is not null && c.Emotions.Count != 0)
			.OrderByDescending(c => c.Emotions.Count)
			.ThenBy(c => c.Names![0])
			;
		Tick[] castTicks = targets
			.Select((c, i) => new Tick(
				i,
				$"{c.Names![1]} - {TextUtil.GetAppCatText(c.Product, c.Category)}"))
			.ToArray()
			;
		Bar[] bars = targets
			.Select((c,i) => new Bar()
				{
					Label = $"{c.Emotions?.Count ?? 0}",
					Position = i,
					Value = c.Emotions?.Count ?? 0,
					FillColor = GetColor(c.Product),
					BorderLineWidth = 0,
					//BorderColor = Colors.Gray,
				})
			.ToArray()
			;
		SetPlot(
			avaPlot,
			bars,
			title: "Voice Lib emotion counts",
			//castTicks: ,
			valueTicks: castTicks,
			isHorizontal: false,
			isShowLegend: true);
		avaPlot.Plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericFixedInterval();
		avaPlot.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
		avaPlot.Plot.Axes.Margins(bottom: 0);
		avaPlot.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
		avaPlot.Plot.Axes.Bottom.TickLabelStyle.OffsetY = 10;

		// determine the width of the largest tick label
		float largestLabelWidth = 0;
		using SkiaSharp.SKPaint paint = new();
		foreach (Tick tick in castTicks)
		{
			var size = avaPlot.Plot.Axes.Bottom.TickLabelStyle.Measure(tick.Label, paint).Size;
			largestLabelWidth = Math.Max(largestLabelWidth, size.Width);
		}

		// ensure axis panels do not get smaller than the largest label
		avaPlot.Plot.Axes.Bottom.MinimumSize = largestLabelWidth;
		avaPlot.Plot.Axes.Right.MinimumSize = largestLabelWidth;

		avaPlot.Plot.Axes.Left.MajorTickStyle.Length = 5;
		await Task.CompletedTask;
	}

	private static void SetPlot<T>(
		AvaPlot avaPlot,
		IEnumerable<T> plots,
		string title = "plot name",
		IEnumerable<Tick>? castTicks = null,
		IEnumerable<Tick>? valueTicks = null,
		bool isHorizontal = false,
		bool isShowLegend = true
	)
	{
		avaPlot.Reset();
		avaPlot.Plot.Clear();

		//title
		avaPlot.Plot.Axes.Title.Label.Text = title;

		//color
		avaPlot.Plot.Axes.Color(Color.FromHex("#a0acb5"));
		avaPlot.Plot.Grid.MajorLineColor = Color.FromHex("#0e3d54");
		avaPlot.Plot.FigureBackground.Color = Color.FromHex("#07263b");
		avaPlot.Plot.DataBackground.Color = Color.FromHex("#0b3049");

		//bar plot
		if (typeof(T) == typeof(ScottPlot.Bar))
		{
			if (plots is IEnumerable<Bar> _bars)
			{
				var barPlot = avaPlot.Plot.Add.Bars(_bars);
				barPlot.Horizontal = isHorizontal;
				barPlot.ValueLabelStyle.ForeColor = Color.FromHex("#a0acb5");
			}
		}

		if (plots is IEnumerable<CoordinateRect> rects)
		{
			var pallet = new ScottPlot.Palettes.Category20();
			int i = 0;
			foreach (var r in rects)
			{
				var rp = avaPlot.Plot.Add.Rectangle(r);
				rp.FillStyle.Color = pallet.Colors[i].WithOpacity(0.01);
				rp.LineColor = pallet.Colors[i];
				i++;
				if (i >= 20) { i = 0; }
			}
		}

		if (castTicks is not null)
		{
			avaPlot.Plot.Axes.Left.TickGenerator = new NumericManual([.. castTicks]);
			avaPlot.Plot.Axes.Left.MajorTickStyle.Length = 0;
			avaPlot.Plot.Axes.Left.Label.FontName = "Noto Sans CJK";
		}

		if (valueTicks is not null)
		{
			avaPlot.Plot.Axes.Bottom.TickGenerator = new NumericManual([.. valueTicks]);
			avaPlot.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
		}

		// build the legend manually
		if (isShowLegend)
		{
			var legend = avaPlot.Plot.Legend;
			legend.IsVisible = true;
			legend.Alignment = Alignment.LowerRight;
			legend.ManualItems = [
				new(){LabelText = nameof(Product.CeVIO_AI), FillColor = Colors.LightSalmon},
				new(){LabelText = nameof(Product.VoiSona), FillColor = Colors.LightBlue},
				new(){LabelText = nameof(Product.CeVIO_CS), FillColor = Colors.Gray},
			];
		}

		avaPlot.Refresh();

	}

	static Color GetColor(Product product)
	{
		return product switch
		{
			Product.CeVIO_AI => ScottPlot.Colors.LightSalmon,
			Product.VoiSona => ScottPlot.Colors.LightBlue,
			_ => Colors.Gray,
		};
	}

	private ValueTask ShowEmotionTableAsync(DataGrid grid)
	{
		var casts = CastList
			.Where(c => c.HasEmotions)
			.ToList()
			.Select(c => new EmotionTableColumn(
				castName: c.Names?[0] ?? "NO NAME",
				category: c.Category,
				product: c.Product,
				emotions: [.. c.Emotions ?? []]
			))
			;
		EmotionTableCastList = [.. casts];
		grid.ItemsSource = new DataGridCollectionView(EmotionTableCastList)
		{
			GroupDescriptions =
            {
             new DataGridPathGroupDescription("Category")
            }
		};
		return default;
	}

	private ValueTask ShowSpSymbolTableAsync(DataGrid grid)
	{
		var casts = CastList
			.Where(c => c.SpSymbols?.Count != 0)
			.ToList()
			.Select(c => new SpSymbolTableColumn(
				Cat: c.Category,
				CastName: c.Names?[0] ?? "NO NAME",
				Product: c.Product,
				SpSymbols: [.. c.SpSymbols]
			))
			;
		SpSymbolTableCastList = [.. casts];
		grid.ItemsSource = new DataGridCollectionView(SpSymbolTableCastList)
		{
			GroupDescriptions =
            {
             new DataGridPathGroupDescription("Product")
            }
		};
		return default;
	}

	private async ValueTask RedrawAsync()
	{
		if (SelectedModeTab is null)
		{
			SelectedModeTabIndex = 0;
		}
		switch (SelectedModeTab?.Name ?? "Plot")
		{
			case "Plot":
			{
				if (SelectedPlotTab is null) break;
				await SelectedPlotTabChangedAsync(SelectedPlotTab);
				break;
			}
			case "Table":
			{
				if (SelectedTableTab is null) break;
				await SelectedTableTabChangedAsync(SelectedTableTab);
				break;
			}
			default:
				break;
		}
	}

	[PropertyChanged(nameof(SelectedPlotTab))]
	[SuppressMessage("","IDE0051")]
	private async ValueTask SelectedPlotTabChangedAsync(TabItem value)
	{
		if(value.Content is not Visual tabVisual) return;

		var plot = tabVisual.FindDescendantOfType<AvaPlot>();
		if(plot is null) return;

		switch(value.Name){
			case "TempoPlotTab":
			{
				await ShowTempoTabAsync(plot);
				break;
			}
			case "RangePlotTab":
			{
				await ShowRangeTabAsync(plot);
				break;
			}
			case "TempoRangePlotTab":
			{
				await ShowTempoRangeTabAsync(plot);
				break;
			}
			case "EmotionsPlotTab":
			{
				await ShowEmotionsTabAsync(plot);
				break;
			}
			default:
				break;
		}

		//return default;
	}

	[PropertyChanged(nameof(SelectedTableTab))]
	[SuppressMessage("","IDE0051")]
	private async ValueTask SelectedTableTabChangedAsync(TabItem value)
	{
		if(value.Content is not Visual tabVisual) return;

		var grid = tabVisual.FindDescendantOfType<DataGrid>();
		if(grid is null) return;

		switch(value.Name)
		{
			case "EmotionsTab":
			{
				await ShowEmotionTableAsync(grid);
				break;
			}

			case "SpSymbolTab":
			{
				await ShowSpSymbolTableAsync(grid);
				break;
			}

			default:
				break;
		}

		//return default;
	}
}
