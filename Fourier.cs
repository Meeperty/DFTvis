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
		
		public double[] DiscreteFourierTransform(int[] input)
		{
			if (input.Length != samples)
				return new double[0];

			double[] output = new double[samples];
			const double radiansStaticPart = 2 * PI * sampleSpacing;
			for (int i = 0; i < frequencies; i++)
			{
				//Complex[] row = MatrixRow(i);
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

		public double[] DiscreteFourierTransformNormalized(int[] input)
		{
			return DiscreteFourierTransform(input).Select(x => x / samples).ToArray();
		}
		

		public Complex[] ComplexFastFourierTransform(Complex[] input, int size, int stride = 1)
		{
			Complex[] output = new Complex[size];
			if (size == 1)
			{
				output[0] = input[0];
				Debug.WriteLine($"For input {ComplexArrString(input)}, CFFT returning {ComplexArrString(output)}");
				return output;
			}
			//TODO: add more base cases

			Complex[] EvenFourier = ComplexFastFourierTransform(input, size/2, 2*stride);
			Complex[] OddFourier = ComplexFastFourierTransform(input[stride..], size/2, 2*stride);
			for (int i = 0; i < size; i++)
			{
				if (i % 2 == 0) output[i] = EvenFourier[i/2];
				else output[i] = OddFourier[i/2];
			}
			for (int k = 0; k < size / 2; k++)
			{
				Complex p = output[k];
				Complex q = UnitCircleExp(-2 * PI * k / size) * output[k + size/2];
				output[k] = p + q;
				output[k + size/2] = p - q;
			}
			Debug.WriteLine($"For input {ComplexArrString(input)}, CFFT returning {ComplexArrString(output)}");
			return output;
		}

		public double[] FastFourierTransformNormalized(double[] input)
		{
			return FastFourierTransform(input).Select(x => x / input.Length).ToArray();
		}

		public double[] FastFourierTransform(double[] input)
		{
			return ComplexFastFourierTransform(input.Select(x => new Complex(x, 0)).AsArray(), input.Length).Select(x => x.Magnitude).ToArray();
		}

		private Complex UnitCircleExp(double radians)
		{
			return new Complex(Math.Cos(radians), Math.Sin(radians));
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
