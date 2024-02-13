using DFTvis.WindowsSound;
using ScottPlot.Avalonia;
using ScottPlot.Plottable;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

namespace DFTvis.ViewModels
{
	public class MainWindowViewModel : /*ReactiveObject,*/ INotifyPropertyChanged
	{
		Fourier dft;
		double[] fast;
		double[] discrete;
		double[,] spectrogram;
		public event PropertyChangedEventHandler? PropertyChanged;
		int inputCount = 16384 * 2;

		private WavFile wvh = new(fileName);

		private string text = @"hs:hs:hs.ssssss";
		public string Text
		{
			get => text;
			set
			{
				text = value;
				PropertyChanged?.Invoke(this, new(nameof(Text)));
			}
		}

		public string FileName
		{
			get => fileName;
			//set => this.RaiseAndSetIfChanged(ref fileName, value);
			set
			{
				fileName = value;
				PropertyChanged?.Invoke(this, new(nameof(FileName)));
			}
		}
		private static string fileName = @"C:\Users\Us\source\repos\DFTvis\Examples\flute_A41second_PCM_us8_mono.wav";

		private AvaPlot dftPlot;
		public AvaPlot DFTPlot
		{
			get => dftPlot;
			set
			{
				dftPlot = value;
				LoadPlot();
				PropertyChanged?.Invoke(this, new(nameof(DFTPlot)));
			}
		}

		public double Width;
		public double Height;

		public MainWindowViewModel()
		{
			dft = new();
			DateTime start = DateTime.Now;
			wvh = new(FileName);
			Debug.WriteLine(wvh);
			var input = wvh.GetData<double>(1)[0..inputCount];
			fast = dft.FastFourierTransformNormalized(input)[1..(inputCount/2)];
			DateTime end = DateTime.Now;
			TimeSpan duration = end - start;
			Text = duration.ToString();

			Debug.WriteLine(duration);
		}

		private void GenerateSpectrogram()
		{

		}

		private void LoadPlot()
		{
			DFTPlot.Width = Width * 0.7d;
			DFTPlot.Height = Height * 0.6d;
			
			Heatmap hm = DFTPlot.Plot.AddHeatmap(spectrogram);
			SignalPlot sp = DFTPlot.Plot.AddSignal(fast, sampleRate:inputCount/(double)44100, color:Color.IndianRed);
			//DFTPlot.Plot.AddSignal(discrete, sampleRate:inputCount/(double)44100, color:Color.Black);
			double freq = 440;
			DFTPlot.Plot.AddVerticalLine(freq / 2, Color.FromArgb(0x7f_AF_5F_7F));
			DFTPlot.Plot.AddVerticalLine(freq, Color.FromArgb(0x7f_AF_5F_7F));
			DFTPlot.Plot.AddVerticalLine(freq * 2, Color.FromArgb(0x7f_AF_5F_7F));
			DFTPlot.Plot.SetAxisLimitsX(-5, 500);
			DFTPlot.Plot.SetAxisLimitsY(0, 10);
			//DFTPlot.Plot.SaveFig(@"C:\Users\Us\source\repos\DFTvis\Plots\shortA4.png");
		}

	}
}
