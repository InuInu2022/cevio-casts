using System.Linq;
using System.Security.AccessControl;
using System.Diagnostics;
using Xunit.Abstractions;
using CevioCasts;
using FluentCeVIOWrapper.Common;

namespace test2;

public class UnitTest1 : IDisposable
{
	private readonly ITestOutputHelper output;
	private readonly string jsonString;
	private readonly Dictionary<string, Process?> process = new();

	public UnitTest1(ITestOutputHelper output)
	{
		this.output = output;
		var path = Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"../../../../data/data.json"
		);
		jsonString = File.ReadAllText(path);

		//StartProcessAsync("CeVIO_CS").AsTask().Wait();
		//StartProcessAsync("CeVIO_AI").AsTask().Wait();
		//Thread.Sleep(1000);
	}

	void IDisposable.Dispose(){
		GC.SuppressFinalize(this);
		foreach(var p in process)
		{
			p.Value?.Kill();
		}
	}

	[Fact]
	public void Test1()
	{
		output.WriteLine("OK!");
	}

	[Fact]
	public async Task ReadAsync()
	{
		var jsonString =
			await File.ReadAllTextAsync("../data/data.json");
		Assert.NotNull(jsonString);
		var defs = Definitions.FromJson(jsonString);
		Assert.NotNull(defs);
	}

	[Fact]
	public void SumCasts()
	{
		var defs = Definitions.FromJson(jsonString);
		int total = defs.Casts.Length;
		int ai_total = defs.Casts.Count(c => c.Product == CevioCasts.Product.CeVIO_AI);
		int cs_total = defs.Casts.Count(c => c.Product == CevioCasts.Product.CeVIO_CS);
		int vs_total = defs.Casts.Count(c => c.Product == CevioCasts.Product.VoiSona);
		int talk_total = defs.Casts.Count(c => c.Category == Category.TextVocal);
		int song_total = defs.Casts.Count(c => c.Category == Category.SingerSong);
		var cs_song = defs.Casts.Count(c => c.Product == CevioCasts.Product.CeVIO_CS && c.Category == Category.SingerSong);
		var cs_talk = defs.Casts.Count(c => c.Product == CevioCasts.Product.CeVIO_CS && c.Category == Category.TextVocal);
		var ai_song = defs.Casts.Count(c => c.Product == CevioCasts.Product.CeVIO_AI && c.Category == Category.SingerSong);
		var ai_talk = defs.Casts.Count(c => c.Product == CevioCasts.Product.CeVIO_AI && c.Category == Category.TextVocal);
		var vs_song = defs.Casts.Count(c => c.Product == CevioCasts.Product.VoiSona && c.Category == Category.SingerSong);
		var vs_talk = defs.Casts.Count(c => c.Product == CevioCasts.Product.VoiSona && c.Category == Category.TextVocal);

		output.WriteLine($"total: {total}");
		output.WriteLine($" ai: {ai_total}");
		output.WriteLine($" cs: {cs_total}");
		output.WriteLine($" vs: {vs_total}");
		output.WriteLine($" talk: {talk_total}");
		output.WriteLine($" song: {song_total}");

		output.WriteLine("|Product|Talk|Song|Total|");
		output.WriteLine("|---|---:|---:|---:|");
		output.WriteLine($"|CeVIO CS|{cs_talk}|{cs_song}|{cs_total}|");
		output.WriteLine($"|CeVIO AI|{ai_talk}|{ai_song}|{ai_total}|");
		output.WriteLine($"|VoiSona|{vs_talk}|{vs_song}|{vs_total}|");
		output.WriteLine($"|Sum|{talk_total}|{song_total}|{total}|");
	}

	[Fact]
	public void SumTempo()
	{
		var defs = Definitions.FromJson(jsonString);
		var list = defs
			.Casts
			.ToList()
			.Where(v => v.VocalTempo is not null)
			.OrderBy(v => v.VocalTempo.High)
			.ThenBy(v => v.VocalTempo.Low)
			;
		foreach(var v in list)
		{
			output.WriteLine($"|{v.Product} |{v.Names[0].Display} |{v.VocalTempo.Low} |{v.VocalTempo.High} |");
		}
	}

	[Fact]
	public void ChechCName()
	{
		var defs = Definitions.FromJson(jsonString);
		foreach(var cast in defs.Casts)
		{
			if(cast.Product == CevioCasts.Product.CeVIO_AI)
			{
				Assert.Equal(cast.Id, cast.Cname);
			}
		}
	}

	[Fact]
	public void CheckSpSymbol()
	{
		var defs = Definitions.FromJson(jsonString);
		foreach(var cast in defs.Casts)
		{
			if(cast?.SpSymbols is null){continue;}
			if(cast!.SpSymbols.Length == 0){continue;}
			var result =
				cast
					.SpSymbols
					.Any(a => a
						.Names
						.Any(a2 => a2.Display != ""));
			Assert.True(result);
		}
	}

	[Fact]
	public void CheckRange()
	{
		var defs = Definitions.FromJson(jsonString);
		foreach (var cast in defs.Casts)
		{
			if(cast.VocalRange is null){ continue;}

			var range = cast.VocalRange;
			Assert.NotEmpty(range.High);
			Assert.NotEmpty(range.Low);

			var low = GetNumber(range.Low);
			var high = GetNumber(range.High);
			Assert.True(
				low <= high,
				$"[{cast.Names[0].Display}] low:{low}, high:{high}, range:{range.Low} - {range.High} "
			);
			if(low == high)
			{
				var lowOct = GetOctave(range.Low);
				var highOct = GetOctave(range.High);
				Assert.True(
					lowOct < highOct,
					$"[{cast.Names[0].Display}] low:{lowOct}, high:{highOct}, range:{range.Low} - {range.High} "
				);
			}
		}

		static int GetOctave(string name)
		{
			var oct = name[0];
			return oct switch
			{
				'C' => 0,
				'D' => 1,
				'E' => 2,
				'F' => 3,
				'G' => 4,
				'A' => 5,
				'B' => 6,
				_ => throw new ArgumentException("Invalid note name"),
			};
		}

		static int GetNumber(string name)
		{
			var num = name[^1];
			return Convert.ToInt32(num);
		}
	}

	[Fact]
	public void CheckTempo()
	{
		var defs = Definitions.FromJson(jsonString);
		foreach (var cast in defs.Casts)
		{
			if (cast.VocalTempo is null) { continue; }
			var tempo = cast.VocalTempo;

			Assert.True(0 < tempo.Low);
			Assert.True(0 < tempo.High);
			Assert.True(tempo.Low < tempo.High);
		}
	}

	[Fact]
	public void CheckCopyPasteError()
	{
		var defs = Definitions
			.FromJson(jsonString);
		foreach (var cast in defs.Casts)
		{
			var isTarget = cast.Cname.EndsWith(".tsnvoice");
			if(!isTarget){continue;}

			Assert.True(cast.Product == CevioCasts.Product.VoiSona);
		}
	}

	[Theory]
	[InlineData("CeVIO_AI")]
	[InlineData("CeVIO_CS")]
	public async Task CastTestAsync(string product)
	{
		output.WriteLine("1");

		output.WriteLine("3");
		var defs = Definitions.FromJson(jsonString);
		output.WriteLine("4");

		var talkCasts = defs
			.Casts
			.Where(cast =>
				cast.Category == Category.TextVocal
					&& cast.Product.ToString() == product)
			;
		Assert.True(talkCasts.Any());

		output.WriteLine("5");
		//var src = new CancellationTokenSource();
		//src.CancelAfter(2000);
		//StartProcessAsync(product,src.Token).AsTask().Wait();
		//await Task.Delay(500);
		output.WriteLine("6");

		var p = (FluentCeVIOWrapper.Common.Product)Enum.Parse(typeof(FluentCeVIOWrapper.Common.Product), product);

		var fcw = await FluentCeVIO
			.FactoryAsync(
				$"{product}_Pipe",
				p);
		output.WriteLine("6-2");

		Assert.NotNull(fcw);

		string[] cnames ;
		try
		{
			cnames = await fcw.GetAvailableCastsAsync();
		}
		catch (Exception e)
		{
			Assert.True(false);
			output.WriteLine($"Error:{e.Message}");
			throw new Xunit.Sdk.XunitException($"Error:{e.Message}");
		}

		output.WriteLine("7");

		foreach(var c in talkCasts)
		{
			var jname = c.Names
				.First(n => n.Lang == Lang.Japanese)
				.Display;
			output.WriteLine($"j name:{jname}");
			//check name is exists
			Assert.Contains(jname, cnames);
		}

		//var _ = await fcw.CloseAsync();

		//process[product]?.Kill();
		output.WriteLine($"{product} check finished");
		await Task.Delay(5000);
	}

	async ValueTask StartProcessAsync(string TTS){
		var ps = new ProcessStartInfo()
		{
			FileName = Path.Combine(
				AppDomain.CurrentDomain.BaseDirectory,
				@"..\..\..\server\FluentCeVIOWrapper.Server.exe"
			),
			Arguments = $"-cevio {TTS} -pipeName {TTS}_Pipe",
			CreateNoWindow = false, //show console if debug,
			//ErrorDialog = true,
			UseShellExecute = true,//false,
			//RedirectStandardOutput = true,
		};

		ps.WorkingDirectory = Path.GetDirectoryName(ps.FileName);
		try
		{
			process?.Add(TTS, Process.Start(ps));
		}
		catch (Exception e)
		{
			output.WriteLine($"Error {e}");
			throw;
		}

		output.WriteLine("awaked.");
		await Task.Delay(500);
		//Thread.Sleep(2000);
	}
}