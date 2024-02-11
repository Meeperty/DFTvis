using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Accord.Math;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using DynamicData.Kernel;
using System.Diagnostics;

namespace DFTvis
{
	internal class Fourier
	{
		const int frequencies = 44100;
		const int samples = frequencies;
		const double sampleSpacing = 1/(double)samples;
		const double PI = Math.PI;

		//Complex[,] dftMatrix = new Complex[0, 0];
		//public bool matrixGenerated = false;

		public Fourier()
		{
			//dftMatrix = new Complex[frequencies, samples];
			////Future TODO: make this faster (probably async)
			//Task fillMatrix = Task.Factory.StartNew(() =>
			//{
			//	for (int i = 0; i < frequencies; i++)
			//	{
			//		for (int j = 0; j < samples; j++)
			//		{
			//			dftMatrix[i, j] = new(
			//				Math.Cos(2 * PI * i * j * sampleSpacing),
			//				Math.Sin(2 * PI * i * j * sampleSpacing)
			//				);
			//		}
			//	}
			//	matrixGenerated = true;
			//});
		}
		
		public double[] DiscreteFourierTransform1Second(int[] input)
		{
			if (input.Length != samples)
				return new double[0];

			double[] output = new double[samples];
			const double radiansStaticPart = 2 * PI * sampleSpacing;
			for (int i = 0; i < frequencies; i++)
			{
				Complex rawOutput = 0;
				Complex matrixElement;
				double radians;
				for (int j = 0; j < samples; j++)
				{
					radians = radiansStaticPart * i * j;
					matrixElement = new Complex(Math.Cos(radians), Math.Sin(radians));
					rawOutput += input[j] * matrixElement;
				}
				output[i] = rawOutput.Magnitude;
			}
			return output;
		}

		public double[] DiscreteFourierTransform(int[] input, int frequencies)
		{
			int sampleCount = input.Length;
			double[] output = new double[frequencies];
			double radiansStaticPart = 2 * PI / sampleCount;
			for (int i = 0; i < frequencies; i++)
			{
				Complex rawOutput = 0;
				Complex matrixElement;
				double radians;
				for (int j = 0; j < sampleCount; j++)
				{
					radians = radiansStaticPart * i * j;
					matrixElement = UnitCircleExp(radians);
					rawOutput += input[j] * matrixElement;
				}
				output[i] = rawOutput.Magnitude;
			}
			return output;
		}

		public double[] DiscreteFourierTransform(int[] input)
		{
			return DiscreteFourierTransform(input, frequencies);
		}

		public double[] DiscreteFourierTransformNormalized(int[] input)
		{
			int len = input.Length;
			return DiscreteFourierTransform(input).Select(x => x / len / samples).ToArray();
		}
		

		public Complex[] ComplexFastFourierTransform(Complex[] input)
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
			Complex[] EvenFourier = ComplexFastFourierTransform(evenInputs);
			Complex[] OddFourier = ComplexFastFourierTransform(oddInputs);
			for (int k = 0; k < halfSize; k++)
			{
				Complex p = EvenFourier[k];
				Complex q = OddFourier[k] * UnitCircleExp(-2 * PI * k / size);
				output[k] = p + q;
				output[k + halfSize] = p - q;
			}

			ret:
			Debug.Write($"{output.Length}#");
			//Debug.WriteLine($"For input {ComplexArrString(input)}, CFFT returning {ComplexArrString(output)}");
			return output;
		}

		public double[] FastFourierTransformNormalized(double[] input)
		{
			return FastFourierTransform(input).Select(x => x / input.Length).ToArray();
		}

		public double[] FastFourierTransform(double[] input)
		{
			return ComplexFastFourierTransform(input.Select(x => new Complex(x, 0)).AsArray()).Select(x => x.Magnitude).ToArray();
		}

		private Complex UnitCircleExp(double radians)
		{
			var o = new Complex(Math.Cos(radians), Math.Sin(radians));
			//Debug.WriteLine($"UnitCircleExp {radians/Math.PI}pi: {o}");
			return o;
		}

		private string ComplexArrString(Complex[] input)
		{
			StringBuilder sb = new();
			sb.Append("[");
			for (int i = 0; i < input.Length; i++)
			{
				if (i == input.Length - 1)
					sb.Append(input[i].ToString());
				else
					sb.Append(input[i].ToString() + ", ");
			}
			sb.Append("]");
			return sb.ToString();
        }
	}
}
