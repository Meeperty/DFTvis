using Avalonia.Controls;
using DFTvis.ViewModels;

namespace DFTvis.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Width = 900;
			Height = 600;
		}

		MainWindowViewModel DataContextCast
		{
			get => (MainWindowViewModel)DataContext;
		}
	}
}
