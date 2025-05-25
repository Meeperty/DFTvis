using ScottPlot;
using System;

namespace DFTvis
{
	public class InterpolatedColormap : ScottPlot.IColormap
	{
		Func<double, double> interpolator;
		IColormap sourceMap;

		public InterpolatedColormap(Func<double, double> interpolator, IColormap sourceMap)
		{
			this.interpolator = interpolator;
			this.sourceMap = sourceMap;
		}

		public string Name => $"Interpolated {sourceMap.GetType()}";

		public Color GetColor(double position)
		{
			return sourceMap.GetColor(interpolator.Invoke(position));
		}
	}
}