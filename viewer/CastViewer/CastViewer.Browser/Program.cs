using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using CastViewer;

[assembly: SupportedOSPlatform("browser")]

internal sealed partial class Program
{
	private static Task Main(string[] args) => BuildAvaloniaApp()
		//.WithInterFont()
		.UseReactiveUI()
		.With(new FontManagerOptions
			{
				FontFallbacks = [
					new(){
						FontFamily = new("avares://CastViewer/Assets/NotoSansCJKjp-Regular.otf#Noto Sans CJK JP")
					}
				],
			})
		.StartBrowserAppAsync("out");

	public static AppBuilder BuildAvaloniaApp()
	=> AppBuilder.Configure<App>();
}