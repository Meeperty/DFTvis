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

		internal static Complex UnitCircleExpApprox(double radians)
		{
			if (radians > TAU)
			{
				throw new UnreachableException("nope");
				radians = double.Ieee754Remainder(radians, TAU);
			}
				
			double sinRadians = double.Ieee754Remainder(radians + double.Pi / 2, TAU);

			double cos = ChebyshevApproxCos(radians, 0, TAU);
			double sin = ChebyshevApproxCos(sinRadians, 0, TAU);

			return new Complex(cos, sin);
		}

		private static double ChebyshevApproxCos(double x, double start, double end)
		{
			x = (2 * x - start - end)/(end - start);
			if (x == -1)
				return 1;
			if (x == 0)
				return -1;
			if (x == 1)
				return 1;
			return c0 +
				c1 * x +
				c2 * (2*x*x - 1) +
				c3 * (4*x*x*x - 3*x) +
				c4 * (8*x*x*x*x - 8*x*x + 1) +
				c5 * (16*x*x*x*x*x - 20*x*x*x + 5*x) +
				c6 * (32*x*x*x*x*x*x - 48*x*x*x*x + 18*x*x - 1);
		}

		private static double ChebyshevPolynomial(double x, int order)
		{
			switch (order)
			{
				case 0:
					return 1;
				case 1:
					return x;
				case 2:
					return 2*x*x - 1;
				case 3:
					return 4*x*x*x - 3*x;
				
				default:
					return 2 * x * ChebyshevPolynomial(x, order-1) - ChebyshevPolynomial(x, order-2);
			}
		}

		//1-indexed
		private static double ChebyshevNode(int i, int N)
		{
			return Math.Cos(double.Pi * (2 * i - 1) / (2 * N));
		}

		private static double AdjustmentCoeff(int k)
		{
			if (k == 0) return 1;
			else return 2;
		}

		private static double ApproximationCoeff(int k, int N, double start, double end)
		{
			double[] evals = new double[N];
            for (int i = 0; i < N; i++)
            {
				//the nodes are 1-indexed
				double u = ChebyshevNode(i+1, N);
				double x = (end - start) * 0.5 * u + (start + end) * 0.5;
				evals[i] = ChebyshevPolynomial(u, k) * Math.Cos(x);
            }

			double avg = evals.Sum() / evals.Length;
			return AdjustmentCoeff(k) * avg;
        }


		//Chebyshev approximation of cos(x),
		//for 0 < x < tau and N = 6
		private static double c0 = ApproximationCoeff(0, 7, 0, TAU);
		private static double c1 = ApproximationCoeff(1, 7, 0, TAU);
		private static double c2 = ApproximationCoeff(2, 7, 0, TAU);
		private static double c3 = ApproximationCoeff(3, 7, 0, TAU);
		private static double c4 = ApproximationCoeff(4, 7, 0, TAU);
		private static double c5 = ApproximationCoeff(5, 7, 0, TAU);
		private static double c6 = ApproximationCoeff(6, 7, 0, TAU);
	}
}