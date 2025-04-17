using ScottPlot.Interactivity.UserActionResponses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace DFTvis
{
	internal static partial class Fourier
	{
		public static double HannWindow(int index, int windowSize)
		{
			return Math.Pow(Math.Cos(double.Pi * index / windowSize), 2);
		}

		public static double HammingWindow(int index, int windowSize)
		{
			double a = 25d/46d;
			return a - (1 - a) * Math.Cos(double.Tau * index / windowSize);
		}

		public static double BinFrequency(int binIndex, int sampleRate, int transformSize)
		{
			return binIndex * sampleRate / transformSize;
		}

		internal static double[] ZeroPad(IEnumerable<double> input, int totalLen)
		{
			double[] output = new double[totalLen];
			for (int i = 0; i < totalLen; i++)
			{
				if (i < input.Count())
				{
					output[i] = input.ElementAt(i);
				}
				else
				{
					output[i] = 0;
				}
			}
			return output;
		}

		internal static string ComplexArrString(Complex[] input)
		{
			StringBuilder sb = new();
			sb.Append("[");
			for (int i = 0; i < input.Length; i++)
			{
				if (i == input.Length - 1)
					sb.Append(input[i].ToString("G4"));
				else
					sb.Append(input[i].ToString("G4") + ", ");
			}
			sb.Append("]");
			return sb.ToString();
		}

		internal static string ComplexMagnitudeArrString(Complex[] input)
		{
			StringBuilder sb = new();
			sb.Append("[");
			for (int i = 0; i < input.Length; i++)
			{
				if (i == input.Length - 1)
					sb.Append(input[i].Magnitude.ToString("G4"));
				else
					sb.Append(input[i].Magnitude.ToString("G4") + ", ");
			}
			sb.Append("]");
			return sb.ToString();
		}

		[DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
		private static extern void GetSystemTimePreciseAsFileTime(out long filetime);
		internal static DateTime UtcNowPrecise
		{
			get
			{
				long filetime;
				GetSystemTimePreciseAsFileTime(out filetime);
				return DateTime.FromFileTimeUtc(filetime);
			}
		}

		internal static Complex UnitCircleExp(double radians)
		{
			return new Complex(Math.Cos(radians), Math.Sin(radians));
		}

	}
}