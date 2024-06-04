using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFTvis.Controls
{
	public class Spectrogram : Control
	{
		public static readonly StyledProperty<Color> MaxColorProperty =
			AvaloniaProperty.Register<Spectrogram, Color>(name: "MaxColor", defaultValue: new Color(255, 255, 0, 255));
		public Color MaxColor
		{
			get { return GetValue(MaxColorProperty); }
			set { SetValue(MaxColorProperty, value); }
		}

		public static readonly StyledProperty<Color> MinColorProperty =
			AvaloniaProperty.Register<Spectrogram, Color>(name: "MinColor", defaultValue: new Color(255, 255, 127, 0));
		public Color MinColor
		{
			get { return GetValue(MinColorProperty); }
			set { SetValue(MinColorProperty, value); }
		}

		public static readonly StyledProperty<double[,]> DataProperty = 
			AvaloniaProperty.Register<Spectrogram, double[,]>(name: "Data");
		public double[,] Data
		{
			get { return GetValue(DataProperty); }
			set { SetValue(DataProperty, value); }
		}
		public void OnDataChange(AvaloniaPropertyChangedEventArgs<double[,]> e)
		{
			GenerateColors();
		}

		IBrush[,] colorBrushes;
		Rect[,] rects;

		public Spectrogram()
		{
			DataProperty.Changed.Subscribe(OnDataChange);
		}

		private void GenerateColors()
		{
			int ylen = Data.GetLength(0);
			int xlen = Data.GetLength(1);
			colorBrushes = new IBrush[ ylen, xlen ];
			double max = MaxReading();

			for (int i = 0; i < ylen; i++)
			{
				for (int j = 0; j < xlen; j++)
				{
					double x = Data[i, j];
					double t = x / max;
					t = Math.Sqrt(t); //sqrt weighting
					SolidColorBrush brush = new SolidColorBrush(ColorLerp(MinColor, MaxColor, t));
					brush.Opacity = 1;
					colorBrushes[i, j] = brush;
				}
			}
		}
		
		public override void Render(DrawingContext context)
		{
			TransformedBounds? tBounds = this.GetTransformedBounds();
			if (generatedBounds != tBounds.Value.Bounds)
				GenerateRectangles();

			if (colorBrushes is not null) {
				for (int i = 0; i < colorBrushes.GetLength(0); i++)
				{
					for (int j = 0; j < colorBrushes.GetLength(1); j++)
					{
						context.DrawRectangle(colorBrushes[i,j], null, rects[i, j]);
					}
				}
			}

			base.Render(context);
		}

		Rect generatedBounds = new Rect();
		private void GenerateRectangles()
		{
			Rect bounds = this.GetTransformedBounds().Value.Bounds;
			generatedBounds = bounds;

			int horizontalDataCount = Data.GetLength(1);
			int verticalDataCount = Data.GetLength(0);
			rects = new Rect[verticalDataCount, horizontalDataCount];

			double sectionWidth = bounds.Width / horizontalDataCount;
			double sectionHeight = bounds.Height / verticalDataCount;
			
			double startingX = bounds.X;
			double startingY = bounds.Y;

			for (int i = 0; i < verticalDataCount; i++)
			{
				for (int j = 0; j < horizontalDataCount; j++)
				{
					double x = startingX + j * sectionWidth;
					double y = startingY + i * sectionHeight;
					Rect rect = new Rect(x, y, sectionWidth, sectionHeight);
					rects[i, j] = rect;
				}
			}
		}

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
	}
}
