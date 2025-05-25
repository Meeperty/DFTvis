using DFTvis.WindowsSound;
using ReactiveUI;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DFTvis.ViewModels
{
	public class SpectrogramTabViewModel : ViewModelBase
	{
		double[,] spectrogram;

		private WavFile wvh = new(fileName);

		private string text = @"";
		public string Text
		{
			get => text;
			set
			{
				this.RaiseAndSetIfChanged(ref text, value);
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
				this.RaiseAndSetIfChanged(ref fileName, value);
			}
		}

		private bool multithreadFFT = false;
		public bool MultithreadFFT
		{
			get => multithreadFFT;
			set
			{
				this.RaiseAndSetIfChanged(ref multithreadFFT, value);
			}
		}

		private bool multithreadSpectrogram = true;
		public bool MultithreadSpectrogram
		{
			get => multithreadSpectrogram;
			set
			{
				this.RaiseAndSetIfChanged(ref multithreadSpectrogram, value);
			}
		}

		private AvaPlot dftPlot;
		public AvaPlot DFTPlot
		{
			get => dftPlot;
			set
			{
				this.RaiseAndSetIfChanged(ref dftPlot, value);
			}
		}

		public double Width;
		public double Height;

		public int sectionsPerSecond = 32;
		public int frequencyResolution = 44100;
		public int visibleFrequencyFraction = 8;

		public async void GenerateAndPlot()
		{
			wvh = new(FileName);

			Text = $"Generating...";
			DFTPlot.Reset();
			DateTime startSpectrogram = DateTime.Now;
			Task spectroTask;
			if (multithreadSpectrogram)
				spectroTask = Task.Run(GenerateSpectrogramMultithread);
			else
				spectroTask = Task.Run(GenerateSpectrogram);
			await spectroTask;
			DateTime endSpectrogram = DateTime.Now;
			TimeSpan duration = endSpectrogram - startSpectrogram;

			ThreadPool.GetMaxThreads(out int workerThreads, out int ioThreads);

			Text = $"processing time: {duration}" +
				$"\n{wvh.SampleCount} samples, {wvh.Duration} sec of audio" +
				$"\napprox {new TimeSpan(wvh.SampleRate * duration.Ticks / wvh.SampleCount)} per sec"
				+ $"\nthread pool has {workerThreads} workers, {ioThreads} ports";

			LoadPlot();
		}

		private void GenerateSpectrogram()
		{
			int timeSectionSampleLen = wvh.SampleRate / sectionsPerSecond;
			int timeSections = (int)(wvh.SampleCount / (double)timeSectionSampleLen);
			var input = wvh.GetData<double>()[0..(timeSections * timeSectionSampleLen)];

			double avg = input.Average();
			input = input.Select(x => x - avg).ToArray();

			double[,] spectro = new double[frequencyResolution / visibleFrequencyFraction, timeSections];
			for (int i = 0; i < timeSections; i++)
			{
				double[] inputs = input[(i * timeSectionSampleLen)..((i + 1) * timeSectionSampleLen)];
				inputs = inputs.Select((x, n) => x * Fourier.HammingWindow(n, timeSectionSampleLen)).ToArray();
				inputs = Fourier.ZeroPad(inputs, frequencyResolution);

				double[] freqs = Fourier.FFT(inputs, MultithreadFFT);
				for (int j = 0; j < frequencyResolution / visibleFrequencyFraction; j++)
				{
					spectro[j, i] = freqs[j];
				}
			}
			spectrogram = spectro;
		}

		private void GenerateSpectrogramMultithread()
		{
			int timeSectionSampleLen = wvh.SampleRate / sectionsPerSecond;
			int timeSections = (int)(wvh.SampleCount / (double)timeSectionSampleLen);
			var input = wvh.GetData<double>()[0..(timeSections * timeSectionSampleLen)];

			double avg = input.Average();
			input = input.Select(x => x - avg).ToArray();

			double[,] spectro = new double[frequencyResolution / visibleFrequencyFraction, timeSections];
			Task<double[]>[] timeSectionTasks = new Task<double[]>[timeSections];
			Task[] spectrogramCreationTasks = new Task[timeSections];
			for (int i = 0; i < timeSections; i++)
			{
				double[] inputs = input[(i * timeSectionSampleLen)..((i + 1) * timeSectionSampleLen)];

				timeSectionTasks[i] = Task.Factory.StartNew((object? inputObj) =>
				{
					if ((double[]?)inputObj is null)
						throw new UnreachableException("Oh dear");

					double[] inputs = (double[])inputObj;
					inputs = inputs.Select((x, n) => x * Fourier.HammingWindow(n, timeSectionSampleLen)).ToArray();
					inputs = Fourier.ZeroPad(inputs, frequencyResolution);

					double[] freqs = Fourier.FFT(inputs)[0..(frequencyResolution / visibleFrequencyFraction)];
					return freqs;
				}, inputs);

				spectrogramCreationTasks[i] = timeSectionTasks[i].ContinueWith((Task<double[]> sectionTask, object? index) =>
				{
					if ((int?)index is null)
						throw new UnreachableException("oh dear");

					double[] result = sectionTask.Result;

					for (int j = 0; j < frequencyResolution / 8; j++)
					{
						spectro[j, (int)index] = result[j];
					}
				}, i);
			}

			Task.WaitAll(spectrogramCreationTasks);
			spectrogram = spectro;
		}

		public void LoadPlot()
		{
			Plot plot = new();

			DFTPlot.Width = Width * 0.7;
			DFTPlot.Height = Height * 0.6;

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
			DFTPlot.UserInputProcessor.Enable();
			this.RaisePropertyChanged(nameof(DFTPlot));
		}
	}
}
