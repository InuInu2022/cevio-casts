using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CastViewer.ViewModels;
using CastViewer.Views;
using HotAvalonia;

namespace CastViewer;

public partial class App : Application
{
    public override void Initialize()
    {
        switch (RuntimeInformation.OSArchitecture)
        {
            case Architecture.Wasm:
                break;

			default:
				{
					this.EnableHotReload();
					break;
				}
		}

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
			singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}