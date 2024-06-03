using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.PropertyGrid.Controls;
using CastViewer.ViewModels;

namespace CastViewer.Views;

public partial class MainView : UserControl
{
	public MainView()
	{
		InitializeComponent();
	}

	private void Listbox_SelectionChanged(
		object? sender,
		SelectionChangedEventArgs e)
	{
		if (sender is not ListBox listBox || listBox.SelectedItem is null)
		{
			return;
		}

		if (DataContext is not MainViewModel vm)
		{
			return;
		}

		//vm.IsLoading = true;
	}

	private void PropertyGrid_Loaded(
		object? sender,
		EventArgs e
	)
	{
		if (sender is not PropertyGrid pg || !pg.IsLoaded)
		{
			return;
		}

		if (DataContext is not MainViewModel vm)
		{
			return;
		}

		vm.IsLoading = false;
	}
}