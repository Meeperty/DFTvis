using DynamicData.Kernel;
using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace DFTvis
{
	internal class Fourier
	{
		const int frequencies = 44100;
		const int samples = frequencies;
		const double sampleSpacing = 1/(double)samples;
		const double PI = Math.PI;

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
			//Debug.Write($"{output.Length}#");
			//Debug.WriteLine($"For input {ComplexArrString(input)}, CFFT returning {ComplexArrString(output)}");
			return output;
		}

		public double[] FastFourierTransformNormalized<T>(T[] input)
		{
			return FastFourierTransform(input).Select(x => x / input.Length).ToArray();
		}

		public double[] FastFourierTransform(double[] input)
		{
			return ComplexFastFourierTransform(input.Select(x => new Complex(x, 0)).AsArray()).Select(x => x.Magnitude).ToArray();
		}

		public double[] FastFourierTransform<T>(T[] rawInput)
		{
			if (Convert.ChangeType(rawInput[0], typeof(double)) == null)
			{
				throw new InvalidCastException("FastFourierTransform<T> was given a T which cannot be converted to double");
			}
			double[] input = rawInput.Select(x => (double)Convert.ChangeType(x, typeof(double))).ToArray();
			return ComplexFastFourierTransform(input.Select(x => new Complex(x, 0)).ToArray())
					.Select(x => x.Magnitude)
					.ToArray();
		}

		//private Dictionary<double, Complex> unitCircleExpLUT = new();

		private Complex UnitCircleExp(double radians)
		{
			//if (unitCircleExpLUT.ContainsKey(radians)) { return unitCircleExpLUT[radians]; }

			var o = new Complex(Math.Cos(radians), Math.Sin(radians));
			//unitCircleExpLUT[radians] = o;
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
