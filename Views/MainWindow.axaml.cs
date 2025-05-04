using Avalonia.Controls;
using DFTvis.ViewModels;
using ScottPlot.Avalonia;
using System;

namespace DFTvis.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Opened += (object? o, EventArgs e) =>
			{
				DataContextCast.Width = Width;
				DataContextCast.Height = Height;
				DataContextCast.DFTPlot = this.Find<AvaPlot>("DFTPlot");
				DataContextCast.GenerateAndPlot();
			};
			Closing += (object? o, WindowClosingEventArgs e) =>
			{
			};
		}

		MainWindowViewModel DataContextCast
		{
			get => (MainWindowViewModel)DataContext;
		}
	}
}
