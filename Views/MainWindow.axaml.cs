using Avalonia.Controls;
using DFTvis.ViewModels;
using System;
using System.Threading;
using System.Media;
using System.Runtime.InteropServices;
using System.ComponentModel;
using ScottPlot.Avalonia;

namespace DFTvis.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Opened += (object? o, EventArgs e) => 
			{
				DataContextCast.DFTPlot = this.Find<AvaPlot>("DFTPlot");
				DataContextCast.GenerateSpectrogram();
			};
			Closing += (object? o, WindowClosingEventArgs e) =>
			{
			};
			this.Width = 600;
			this.Height = 400;
		}

		MainWindowViewModel DataContextCast
		{
			get => (MainWindowViewModel)DataContext;
		}
	}
}
