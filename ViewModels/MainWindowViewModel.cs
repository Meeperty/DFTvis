using DFTvis.WindowsSound;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Panels;
using ScottPlot.Plottables;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace DFTvis.ViewModels
{
	public class MainWindowViewModel : /*ReactiveObject,*/ INotifyPropertyChanged
	{
		double[] fast;
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

		private static string fileName = @"C:\Users\Us\source\repos\DFTvis\Examples\flute_A4_PCM_us8.wav";
		//private static string fileName = @"C:\Users\Us\source\repos\DFTvis\Examples\CDC_Voice_PCM_us8.wav";
		//private static string fileName = @"C:\Users\Us\Documents\Source Unpack 2.4\portal\sound\vo\aperture_ai\03_part1_entry-1.wav";
		//private static string fileName = @"C:\Users\Us\source\repos\DFTvis\Examples\666 sin 30 sec.wav";
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

		private AvaPlot dftPlot;
		public AvaPlot DFTPlot
		{
			get => dftPlot;
			set
			{
				dftPlot = value;
				//LoadPlot();
				PropertyChanged?.Invoke(this, new(nameof(DFTPlot)));
			}
		}

		public double Width;
		public double Height;

		public MainWindowViewModel()
		{
			wvh = new(FileName);
			Debug.WriteLine(wvh);

			DateTime startSpectrogram = DateTime.Now;
			GenerateSpectrogram();
			DateTime endSpectrogram = DateTime.Now;
			TimeSpan duration = endSpectrogram - startSpectrogram;
			Text = $"processing time: {duration}\n{wvh.SampleCount} samples, {wvh.Duration} sec\napprox {new TimeSpan(wvh.SampleRate * duration.Ticks / wvh.SampleCount)} per sec";

			Debug.WriteLine(duration);
		}

		public void GenerateAndPlot()
		{
			wvh = new(FileName);

			DateTime startSpectrogram = DateTime.Now;
			GenerateSpectrogram();
			DateTime endSpectrogram = DateTime.Now;
			TimeSpan duration = endSpectrogram - startSpectrogram;
			Text = $"processing time: {duration}\n{wvh.SampleCount} samples, {wvh.Duration} sec\napprox {new TimeSpan(wvh.SampleRate * duration.Ticks / wvh.SampleCount)} per sec";

			LoadPlot();
		}

		private void GenerateSpectrogram()
		{
			int timeSectionSampleLen = 44100 / 7;
			//int timeSectionSampleLen = wvh.SampleCount;
			int timeSections = (int)(wvh.SampleCount / (double)timeSectionSampleLen);
			var input = wvh.GetData<double>()[0..(timeSections * timeSectionSampleLen)];
			double avg = input.Average();
			input = input.Select(x => x - avg).ToArray();
			double[,] spectro = new double[44100 / 8, timeSections];
			for (int i = 0; i < timeSections; i++)
			{
				double[] inputs = input[(i * timeSectionSampleLen)..((i + 1) * timeSectionSampleLen)];
				inputs = Fourier.ZeroPad(inputs.ToList(), 44100 /*wvh.SampleCount*/).ToArray();
				double[] freqs = Fourier.FFT(inputs);
				//fast = freqs;
				for (int j = 0; j < 44100 / 8; j++)
				{
					spectro[j, i] = freqs[j];
				}
			}
			spectrogram = spectro;
		}

		public void LoadPlot()
		{
			DFTPlot.Width = Width * 0.7d;
			DFTPlot.Height = Height * 0.6d;

			Plot plot = new();

			//DFTPlot.Plot.Add.Signal(fast, color:ScottPlot.Color.FromARGB(0xFF000000));
			Heatmap hm = plot.Add.Heatmap(spectrogram);
			hm.Colormap = new InterpolatedColormap(Math.Cbrt, new ScottPlot.Colormaps.Plasma());
			hm.FlipVertically = true;

			hm.CellAlignment = Alignment.MiddleLeft;
			//hm.Smooth = true;
			plot.Add.ColorBar(hm);
			//DFTPlot.Plot.SetAxisLimitsX(0, 136);
			//DFTPlot.Plot.SetAxisLimitsY(0, 22050);
			//DFTPlot.Plot.Margins(0, 0);
			//SignalPlot sp = DFTPlot.Plot.AddSignal(fast, sampleRate:1/*inputCount/(double)44100*/, color:Color.IndianRed);
			//DFTPlot.Plot.SaveFig(@"C:\Users\Us\source\repos\DFTvis\Plots\shortA4.png");

			DFTPlot.Reset(plot);
		}
	}
}
