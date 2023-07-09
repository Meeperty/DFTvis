using DFTvis.WindowsSound;
using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Timers;
using DynamicData.Kernel;
using System.Linq;
using ScottPlot.Avalonia;
using System.Drawing;
using ScottPlot.Plottable;

namespace DFTvis.ViewModels
{
	public class MainWindowViewModel : /*ReactiveObject,*/ INotifyPropertyChanged
	{
		Fourier dft;
		double[] dftData;
		public event PropertyChangedEventHandler? PropertyChanged;

		public MainWindowViewModel()
		{
			dft = new();
			//updateMatrixGeneratedTimer = new(1000);
			//updateMatrixGeneratedTimer.Elapsed += updateMatrixGenerated;
			//updateMatrixGeneratedTimer.AutoReset = true;
			//updateMatrixGeneratedTimer.Start();
			var data = new int[44100];
			for (int i = 0; i < 44100; i++)
			{
				if (i % 2 == 0)
					data[i] = 1;
				else
					data[i] = -1;
			}
			DateTime start = DateTime.Now;
			//var test = dft.DiscreteFourierTransform(dft.MatrixRow(1).Select(x => (int)x.Real).ToArray());
			//var test = dft.DiscreteFourierTransformNormalized(data);
			var test = dft.FastFourierTransform(new double[] { 5, 3, 2, 1 });
			dftData = dft.DiscreteFourierTransformNormalized(wvh.Data[..44100]);
			DateTime end = DateTime.Now;
			TimeSpan duration = end - start;
		}

		private void LoadPlot()
		{
			//DFTPlot.Plot.Add();
			DFTPlot.Width = Width * 0.6d;
			DFTPlot.Height = Height * 0.5d;

			SignalPlot sp = DFTPlot.Plot.AddSignal(dftData, color:Color.IndianRed);
			double freq = 440;
			//DFTPlot.Plot.AddVerticalLine(freq/2, Color.FromArgb(0x7f_AF_5F_7F));
			//DFTPlot.Plot.AddVerticalLine(freq, Color.FromArgb(0x7f_AF_5F_7F));
			//DFTPlot.Plot.AddVerticalLine(freq*2, Color.FromArgb(0x7f_AF_5F_7F));
			DFTPlot.Plot.SetAxisLimitsX(0, 1000);
			DFTPlot.Plot.SetAxisLimitsY(0, 30);
			DFTPlot.Plot.SaveFig(@"C:\Users\Us\source\repos\DFTvis\Plots\shortA4.png");
		}

		public string Text => wvh.Data.Length.ToString();

		private WavFile wvh = new(fileName);

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
		private static string fileName = @"C:\Users\Us\source\repos\DFTvis\Examples\flute_A4short_PCM_us8.wav";

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

		//public string BPM
		//{
		//	get => bpm.ToString();
		//	set => this.RaiseAndSetIfChanged(ref bpm, float.Parse(value));
		//}
		//public float bpm = 120;

		//public string MatrixGenerated
		//{
		//	get => dft.matrixGenerated.ToString();
		//}
		//private string oldMatrixGenerated;
		//private System.Timers.Timer updateMatrixGeneratedTimer;
		//private void updateMatrixGenerated(object? o, ElapsedEventArgs e)
		//{
		//	if (oldMatrixGenerated != null && oldMatrixGenerated != MatrixGenerated) {
		//		PropertyChanged?.Invoke(this, new(nameof(MatrixGenerated)));
		//	}
		//	oldMatrixGenerated = MatrixGenerated;
		//}
	}
}
