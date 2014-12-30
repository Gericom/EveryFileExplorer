using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LibEveryFileExplorer.GFX
{
	public class GFXUtil
	{
		public static Bitmap Resize(Bitmap Original, int Width, int Height)
		{
			if (Original.Width == Width && Original.Height == Height) return Original;
			Bitmap res = new Bitmap(Width, Height);
			using (Graphics g = Graphics.FromImage(res))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				g.DrawImage(Original, 0, 0, Width, Height);
				g.Flush();
			}
			return res;
		}

		public static uint ConvertColorFormat(uint InColor, ColorFormat InputFormat, ColorFormat OutputFormat)
		{
			if (InputFormat == OutputFormat) return InColor;
			//From color format to components:
			uint A, R, G, B;
			uint mask;
			if (InputFormat.ASize == 0) A = 255;
			else
			{
				mask = ~(0xFFFFFFFFu << InputFormat.ASize);
				A = ((((InColor >> InputFormat.AShift) & mask) * 255u) + mask / 2) / mask;
			}
			mask = ~(0xFFFFFFFFu << InputFormat.RSize);
			R = ((((InColor >> InputFormat.RShift) & mask) * 255u) + mask / 2) / mask;
			mask = ~(0xFFFFFFFFu << InputFormat.GSize);
			G = ((((InColor >> InputFormat.GShift) & mask) * 255u) + mask / 2) / mask;
			mask = ~(0xFFFFFFFFu << InputFormat.BSize);
			B = ((((InColor >> InputFormat.BShift) & mask) * 255u) + mask / 2) / mask;
			return ToColorFormat(A, R, G, B, OutputFormat);
		}

		public static uint ToColorFormat(int R, int G, int B, ColorFormat OutputFormat)
		{
			return ToColorFormat(255u, (uint)R, (uint)G, (uint)B, OutputFormat);
		}

		public static uint ToColorFormat(int A, int R, int G, int B, ColorFormat OutputFormat)
		{
			return ToColorFormat((uint)A, (uint)R, (uint)G, (uint)B, OutputFormat);
		}

		public static uint ToColorFormat(uint R, uint G, uint B, ColorFormat OutputFormat)
		{
			return ToColorFormat(255u, R, G, B, OutputFormat);
		}

		public static uint ToColorFormat(uint A, uint R, uint G, uint B, ColorFormat OutputFormat)
		{
			uint result = 0;
			uint mask;
			if (OutputFormat.ASize != 0)
			{
				mask = ~(0xFFFFFFFFu << OutputFormat.ASize);
				result |= ((A * mask + 127u) / 255u) << OutputFormat.AShift;
			}
			mask = ~(0xFFFFFFFFu << OutputFormat.RSize);
			result |= ((R * mask + 127u) / 255u) << OutputFormat.RShift;
			mask = ~(0xFFFFFFFFu << OutputFormat.GSize);
			result |= ((G * mask + 127u) / 255u) << OutputFormat.GShift;
			mask = ~(0xFFFFFFFFu << OutputFormat.BSize);
			result |= ((B * mask + 127u) / 255u) << OutputFormat.BShift;
			return result;
		}
	}
}
