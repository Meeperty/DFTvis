using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFTvis.Controls
{
	public class Spectrogram : Control
	{
		public static readonly StyledProperty<Color> MaxColorProperty =
			AvaloniaProperty.Register<Spectrogram, Color>(nameof(MaxColor), defaultValue: new Color(255, 255, 155, 0));
		public Color MaxColor
		{
			get { return GetValue(MaxColorProperty); }
			set { SetValue(MaxColorProperty, value); }
		}


		public static readonly StyledProperty<Color> MinColorProperty =
			AvaloniaProperty.Register<Spectrogram, Color>(nameof(MinColor), defaultValue: new Color(255, 127, 127, 127));
		public Color MinColor
		{
			get { return GetValue(MinColorProperty); }
			set { SetValue(MinColorProperty, value); }
		}


		public static readonly StyledProperty<int> ColorResolutionProperty =
			AvaloniaProperty.Register<Spectrogram, int>(nameof(ColorResolution), defaultValue: 20);
		public int ColorResolution
		{
			get { return GetValue(ColorResolutionProperty); }
			set { SetValue(ColorResolutionProperty, value); }
		}


		public static readonly StyledProperty<double[,]> DataProperty = 
			AvaloniaProperty.Register<Spectrogram, double[,]>(nameof(Data));
		public double[,] Data
		{
			get { return GetValue(DataProperty); }
			set { SetValue(DataProperty, value); }
		}
		public void OnDataChange(AvaloniaPropertyChangedEventArgs<double[,]> e)
		{
			maxReading = MaxReading();
			GenerateRectangles();
			InvalidateArrange();
		}
		private double maxReading = 0;


		List<SpectroRect> rects = new();
		Rect generatedClip = new Rect();

		const int columnsPerSecond = 2;

		public Spectrogram()
		{
			DataProperty.Changed.Subscribe(OnDataChange);
		}


		public override void Render(DrawingContext context)
		{
			TransformedBounds tBounds = this.GetTransformedBounds().Value;
			if (generatedClip != tBounds.Clip)
				GenerateRectangles();
			for (int i = 0; i < rects.Count; i++)
			{
				SpectroRect sr = rects[i];
				Rect rectToDraw = sr.rect;
				IBrush brush = sr.brush;
				context.DrawRectangle(brush, null, rectToDraw);
			}

			base.Render(context);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			double widthPerColumn = 50 / columnsPerSecond; //hardcoded for now
			double width = widthPerColumn * Data.GetLength(0);
			double height = 300;
			Size size = new Size(width, height);

			InvalidateVisual();
			return size;
		}

		//protected override Size MeasureOverride(Size availableSize)
		//{
		//	return availableSize;
		//}

		private void GenerateRectangles()
		{
			var tBounds = this.GetTransformedBounds().Value;
			generatedClip = tBounds.Clip;
			Rect bounds = this.GetTransformedBounds().Value.Clip;

			//sometimes this is true, IDK why
			if (bounds.Width == 0 && bounds.Height == 0)
			{
				return;
			}

			int horizontalDataCount = Data.GetLength(0);
			int verticalDataCount = Data.GetLength(1);

			double sectionWidth = bounds.Width / horizontalDataCount;
			double sectionHeight = bounds.Height / verticalDataCount;
			
			double startingX = bounds.X;
			double startingY = bounds.Y;

			rects = new();
			for (int i = 0; i < horizontalDataCount; i++)
			{
				for (int j = 0; j < verticalDataCount; j++)
				{
					double x = startingX + i * sectionWidth;
					double y = startingY + bounds.Height - j * sectionHeight;
					Color color = ColorOfDataPoint(Data[i,j]);

					int repeatLen = 1; //includes starting rect
					for (; j < verticalDataCount; j++)
					{
						if (ColorOfDataPoint(Data[i,j]) == color)
							repeatLen++;
						else
							break;
					}

					Rect rect = new Rect(x, y - sectionHeight * repeatLen, sectionWidth, sectionHeight * repeatLen);
					rects.Add(new SpectroRect(rect, color));
				}
			}
		}



		//private void ResizeRectangles()
		//{
		//	Rect newBounds = this.GetTransformedBounds().Value.Clip;

		//	int horizontalDataCount = Data.GetLength(1);
		//	int verticalDataCount = Data.GetLength(0);

		//	double oldSectionWidth = generatedClip.Width;
		//	double oldSectionHeight = generatedClip.Height;

		//	double newSectionHeight = newBounds.Width;
		//	double newSectionWidth = newBounds.Height;

		//	for (int i = 0; i < verticalDataCount; i++)
		//	{
		//		for (int j = 0; j < horizontalDataCount; j++)
		//		{
		//			Rect oldRect = rects[i,j];
		//			double newX = oldRect.X - (j * oldSectionWidth);
		//		}
		//	}
		//}

		private double MaxReading()
		{
			double max = double.NegativeInfinity;
			foreach (double value in Data)
			{
				if (value > max)
				{
					max = value;
				}
			}
			return max;
		}


		private Color ColorLerp(Color a, Color b, double t)
		{
			byte alpha = (byte)(a.A * (1 - t) + b.A * t);
			byte red = (byte)(a.R * (1 - t) + b.R * t);
			byte green = (byte)(a.G * (1 - t) + b.G * t);
			byte blue = (byte)(a.B * (1 - t) + b.B * t);
			return new Color(alpha, red, green, blue);
		}


		private Color ColorOfDataPoint(double dataPoint)
		{
			double colorFraction = /*Math.Pow(*/dataPoint / maxReading/*, 2)*/; //sqr weighting to stop high values from taking all the colors
			colorFraction = Math.Floor(colorFraction * ColorResolution) / ColorResolution;
			return ColorLerp(MinColor, MaxColor, colorFraction);
		}
	}

	internal struct SpectroRect
	{
		public Avalonia.Rect rect;
		public Avalonia.Media.Color color;
		public Avalonia.Media.IBrush brush;

		internal SpectroRect(Rect rect, Color color)
		{
			this.rect = rect;
			this.color = color;
			brush = new SolidColorBrush(color);
		}
	}
}
