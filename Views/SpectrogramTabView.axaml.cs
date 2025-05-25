using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DFTvis.ViewModels;
using ScottPlot.Avalonia;

namespace DFTvis;

public partial class SpectrogramTabView : UserControl
{
	public SpectrogramTabView()
	{
		InitializeComponent();
		Loaded += (object? o, RoutedEventArgs e) =>
		{
			DataContextCast.DFTPlot = this.Find<AvaPlot>("DFTPlot");
			DataContextCast.Width = this.Bounds.Width;
			DataContextCast.Height = this.Bounds.Height;
		};
		SizeChanged += (object? o, SizeChangedEventArgs e) =>
		{
			DataContextCast.Width = e.NewSize.Width;
			DataContextCast.Height = e.NewSize.Height;
		};
	}

	public async void ChooseFileButton(object sender, RoutedEventArgs args)
	{
		TopLevel? window = TopLevel.GetTopLevel(this);
		if (window is null)
			throw new System.Exception($"Failed to get window in {nameof(SpectrogramTabView)}");

		FilePickerFileType type = new FilePickerFileType("Wav Files")
		{
			Patterns = ["*.wav"]
		};

		IStorageFolder? examplesFolder = await window.StorageProvider.TryGetFolderFromPathAsync(@"C:\Users\Us\source\repos\DFTvis\Examples\");

		var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			AllowMultiple = false,
			FileTypeFilter = [type],
			SuggestedStartLocation = examplesFolder,
			Title = "Select a file"
		});

		if (files is not null && files.Count > 0)
		{
			var file = files[0];

			DataContextCast.FileName = file.Path.AbsolutePath;
		}
	}

	SpectrogramTabViewModel DataContextCast
	{
		get => (SpectrogramTabViewModel)DataContext;
	}
}