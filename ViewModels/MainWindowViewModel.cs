using System.ComponentModel;

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
