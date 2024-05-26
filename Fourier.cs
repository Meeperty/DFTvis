using DynamicData.Kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DFTvis
{
	internal static class Fourier
	{
		const int frequencies = 44100;
		const int samples = frequencies;
		const double TAU = 2 * Math.PI;

		public static double[] RealDFT(int[] input, int frequencies)
		{
			int size = input.Length;
			double[] output = new double[frequencies];
			double radiansStaticPart = TAU / size;
			for (int i = 0; i < frequencies; i++)
			{
				Complex rawOutput = 0;
				Complex matrixElement;
				double radians;
				for (int j = 0; j < size; j++)
				{
					radians = radiansStaticPart * i * j;
					matrixElement = UnitCircleExp(radians);
					rawOutput += input[j] * matrixElement;
				}
				output[i] = rawOutput.Magnitude;
			}
			return output;
		}

		public static double[] DFT(int[] input)
		{
			return RealDFT(input, frequencies);
		}

		public static Complex[] SquareDFT(Complex[] input)
		{
			int size = input.Length;
			Complex[] output = new Complex[size];
			double radiansStaticPart = -TAU / size;
			for (int i = 0; i < size; i++)
			{
				Complex rawOutput = 0;
				Complex matrixElement;
				double radians;
				for (int j = 0; j < size; j++)
				{
					radians = radiansStaticPart * i * j;
					matrixElement = UnitCircleExp(radians);
					rawOutput += input[j] * matrixElement;
				}
				output[i] = rawOutput;
			}
			return output;
		}

		public static Complex[] CFFTRadix2(Complex[] input)
		{
			int size = input.Length;
			int halfSize = size/2;
			Complex[] output = new Complex[size];
			if (size == 1)
			{
				output[0] = input[0];
				goto ret;
			}
			//TODO: add more base cases

			Complex[] evenInputs = new Complex[halfSize];
			for (int i = 0; i < halfSize; i++)
			{
				evenInputs[i] = input[2 * i];
			}
			Complex[] oddInputs = new Complex[halfSize];
			for (int i = 0; i < halfSize; i++)
			{
				oddInputs[i] = input[2 * (i + 1) - 1];
			}
			Complex[] EvenFourier = CFFTRadix2(evenInputs);
			Complex[] OddFourier = CFFTRadix2(oddInputs);
			for (int k = 0; k < halfSize; k++)
			{
				Complex a = EvenFourier[k];
				Complex b = OddFourier[k] * UnitCircleExp(-TAU * k / size);
				output[k] = a + b;
				output[k + halfSize] = a - b;
				Debug.WriteLineIf(size == 4, $"a is {a}, b is {b}, a+b={a+b}, a-b={a-b}");
			}

			ret:
			//Debug.Write($"{output.Length}#");
			Debug.WriteLine($"For input {ComplexArrString(input)}, CFFTRadix2 returning {ComplexArrString(output)}");
			return output;
		}

		public static Complex[] CFFT(Complex[] input)
		{
			int size = input.Length;
			Complex[] output = new Complex[size];
			int radix = 0;
			if (size == 1)
			{
				output = input;
				goto ret;
			}


			for (int i = 2; i < size; i++)
			{
				if (size % i == 0)
				{
					radix = i;
					//Debug.WriteLine($"CFFT: radix {radix} chosen for size {size}");
					break;
				}
				else
				{
					continue;
				}
			}
			if (radix == 0)
			{
				output = SquareDFT(input);
				goto ret;
			}
			int fractionSize = size / radix;


			Complex[][] inputSections = new Complex[radix][];
			for (int i = 0; i < radix; i++)
			{
				inputSections[i] = new Complex[fractionSize];
			}
			for (int i = 0; i < size; i++)
			{
				inputSections[i % radix][(int)Math.Floor(i / (decimal)radix)] = input[i];
			}
			//inputSections is Complex[radix][fractionSize]
			Complex[][] outputSections = new Complex[radix][];
			for (int i = 0; i < radix; i++)
			{
				outputSections[i] = CFFT(inputSections[i]);
			}

			for (int k = 0; k < fractionSize; k++)
			{
				//no idea what to call this
				Complex[] parts = new Complex[radix];
				for (int i = 0; i < radix; i++)
				{
					//this UnitCircleExp is the twiddle factor
					parts[i] = outputSections[i][k] * UnitCircleExp(-TAU * k * i / size);
				}

				for (int i = 0; i < radix; i++)
				{
					Complex outputItem = new Complex();
					for (int j = 0; j < radix; j++)
					{
						outputItem += parts[j] * UnitCircleExp(-TAU * i * j / radix);
					}
					output[k + i * (size / radix)] = outputItem;
				}
			}

			ret:
			//Debug.WriteLine($"For input {ComplexArrString(input)}, CFFT {(radix == 0 ? "(CDFT)" : $"radix {radix}")} returning {ComplexArrString(output)}");
			return output;
		}

		public static double[] FFTNormalized<T>(T[] input)
		{
			return FFT<T>(input).Select(x => x / input.Length).ToArray();
		}

		public static double[] FFT(double[] input)
		{
			return CFFT(input.Select(x => new Complex(x, 0)).AsArray()).Select(x => x.Magnitude).ToArray();
		}

		public static double[] FFT<T>(T[] rawInput)
		{
			if (Convert.ChangeType(rawInput[0], typeof(double)) == null)
			{
				throw new InvalidCastException("FFT<T> was given a T which cannot be converted to double");
			}
			DateTime startInputProcessing = UtcNowPrecise;
			Complex[] input = rawInput.Select(x => new Complex((double)Convert.ChangeType(x, typeof(double)),0)).ToArray();
			DateTime endInputProcessing = UtcNowPrecise;
			TimeSpan inputProcessing = endInputProcessing - startInputProcessing;

			DateTime startCFFT = UtcNowPrecise;
			Complex[] output = CFFT(input);
			DateTime endCFFT = UtcNowPrecise;
			TimeSpan cfft = endCFFT - startCFFT;

			DateTime startOutputProcessing = UtcNowPrecise;
			double[] processedOut = output
					.Select(x => x.Magnitude)
					.ToArray();
			DateTime endOutputProcessing = UtcNowPrecise;
			TimeSpan outputProcessing = endOutputProcessing - startOutputProcessing;

			return processedOut;
		}

		private static Complex UnitCircleExp(double radians)
		{
			return new Complex(Math.Cos(radians), Math.Sin(radians));
		}

		internal static List<double> ZeroPad(List<double> input, int totalLen)
		{
			List<double> output = new();
			for (int i = 0; i < totalLen; i++)
			{
				if (i < input.Count)
				{
					output.Add(input[i]);
				}
				else
				{
					output.Add(0);
				}
			}
			return output;
		}

		public static string ComplexArrString(Complex[] input)
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

		public static string ComplexMagnitudeArrString(Complex[] input)
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

		public static double BinFrequency(int binIndex, int sampleRate, int transformSize)
		{
			return binIndex * sampleRate / transformSize;
		}

		[DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
		private static extern void GetSystemTimePreciseAsFileTime(out long filetime);
		public static DateTime UtcNowPrecise
		{
			get
			{
				long filetime;
				GetSystemTimePreciseAsFileTime(out filetime);
				return DateTime.FromFileTimeUtc(filetime);
			}
		}
	}
}
