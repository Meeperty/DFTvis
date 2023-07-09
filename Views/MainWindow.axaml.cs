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
				DataContextCast.Width = Width;
				DataContextCast.Height = Height;
				DataContextCast.DFTPlot = this.Find<AvaPlot>("DFTPlot");
			};
			Closing += (object? o, CancelEventArgs e) =>
			{
			};
		}

		MainWindowViewModel DataContextCast
		{
			get => (MainWindowViewModel)DataContext;
		}
	}
}
