using DFTvis.WindowsSound;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DFTvis.ViewModels
{
	public class MainWindowViewModel : /*ReactiveObject,*/ INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private SpectrogramTabViewModel spectrogramTabViewModel;
		public SpectrogramTabViewModel SpectrogramTabViewModel
		{
			get => spectrogramTabViewModel;
			set
			{
				spectrogramTabViewModel = value;
				PropertyChanged?.Invoke(this, new(nameof(SpectrogramTabViewModel)));
			}
		}

		public MainWindowViewModel()
		{
			SpectrogramTabViewModel = new();
		}
	}
}
