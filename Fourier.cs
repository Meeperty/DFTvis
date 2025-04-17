using DynamicData.Kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace DFTvis
{
	internal static partial class Fourier
	{
		const int frequencies = 44100;
		const int samples = frequencies;
		const double TAU = 2 * Math.PI;

		#region DFT
		private static Complex[] CDFT(Complex[] input, int frequencies)
		{
			int size = input.Length;
			Complex[] output = new Complex[frequencies];
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
				output[i] = rawOutput;
			}
			return output;
		}

		private static Complex[] CDFT(IEnumerable<Complex> rawInput, int frequencies)
		{
			return CDFT(rawInput.ToArray(), frequencies);
		}

		public static Complex[] CDFT<T>(IEnumerable<T> rawInput, int frequencies)
		{
			if (Convert.ChangeType(rawInput.ElementAt(0), typeof(double)) == null)
			{
				throw new InvalidCastException("RealDFT<T> was given a T which cannot be converted to double");
			}

			Complex[] input = rawInput
			.Select(x => new Complex((double)Convert.ChangeType(x, typeof(double)), 0))
			.ToArray();

			Complex[] output = CDFT(input, frequencies);

			return output;
		}

		public static Complex[] SquareDFT<T>(IEnumerable<T> input)
		{
			return CDFT<T>(input, input.Count());
		}

		private static Complex[] SquareDFT(IEnumerable<Complex> input)
		{
			return CDFT(input, input.Count());
		}

		public static double[] RealDFT<T>(IEnumerable<T> rawInput, int frequencies)
		{
			if (Convert.ChangeType(rawInput.ElementAt(0), typeof(double)) == null)
			{
				throw new InvalidCastException("RealDFT<T> was given a T which cannot be converted to double");
			}

			Complex[] input = rawInput
				.Select(x => new Complex((double)Convert.ChangeType(x, typeof(double)), 0))
				.ToArray();

			Complex[] output = CDFT(input, frequencies);

			double[] processedOutput = output
				.Select(x => x.Magnitude)
				.ToArray();
			return processedOutput;
		}
		#endregion


		#region FFT
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
			Task<Complex[]>[] cfftTasks = new Task<Complex[]>[radix];
			for (int i = 0; i < radix; i++)
			{
				cfftTasks[i] = Task<Complex[]>.Factory.StartNew((object? input) =>
				{
					Complex[] cast = input as Complex[];
					return CFFT(cast);
				},
				inputSections[i]);
			}
			Task.WaitAll(cfftTasks);
			for (int i = 0; i < radix; i++)
			{
				outputSections[i] = cfftTasks[i].Result;
			}


			for (int k = 0; k < fractionSize; k++)
			{
				//no idea what to call this
				Complex[] parts = new Complex[radix];
				for (int i = 0; i < radix; i++)
				{
					double angle = -TAU * k * i / size;
					//this UnitCircleExp is the twiddle factor
					parts[i] = outputSections[i][k] * UnitCircleExp(angle);
				}

				for (int i = 0; i < radix; i++)
				{
					Complex outputItem = new Complex();
					for (int j = 0; j < radix; j++)
					{
						double angle = -TAU * i * j / radix;
						outputItem += parts[j] * UnitCircleExp(angle);
					}
					output[k + i * (size / radix)] = outputItem;
				}
			}

			ret:
			//Debug.WriteLine($"For input {ComplexArrString(input)}, CFFT {(radix == 0 ? "(CDFT)" : $"radix {radix}")} returning {ComplexArrString(output)}");
			return output;
		}

		public static double[] FFTNormalized<T>(IEnumerable<T> input)
		{
			int count = input.Count();
			return FFT<T>(input).Select(x => x / count).ToArray();
		}

		public static double[] FFT<T>(IEnumerable<T> rawInput)
		{
			if (typeof(T) != typeof(Complex))
			{
				if (Convert.ChangeType(rawInput.ElementAt(0), typeof(double)) == null)
				{
					throw new InvalidCastException("FFT<T> was given a T which cannot be converted to double");
				}
			}

			Complex[] input = rawInput
				.Select(x => new Complex((double)Convert.ChangeType(x, typeof(double)),0))
				.ToArray();

			Complex[] output = CFFT(input);

			double[] processedOut = output
					.Select(x => x.Magnitude)
					.ToArray();

			return processedOut;
		}
		#endregion
	}
}
