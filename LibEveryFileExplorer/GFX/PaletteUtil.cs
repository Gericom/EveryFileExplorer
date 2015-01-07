using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LibEveryFileExplorer.GFX
{
	public class PaletteUtil
	{
		//Very slow for big images!
		public static uint[] GeneratePalette(uint[] Colors, int NrOutputColors, int NrOutputBits = 8, bool FirstTransparent = false)
		{
			List<Color> UniqueColors = new List<Color>();
			if (NrOutputBits > 8) NrOutputBits = 8;
			if (NrOutputBits < 1) NrOutputBits = 1;
			uint mask = (uint)(~((1 << (8 - NrOutputBits)) - 1)) & 0xFF;
			foreach (uint color in Colors)
			{
				uint a = (color >> 24) & mask;
				if (a < 127) continue;
				uint r = (color >> 16) & mask;
				uint g = (color >> 8) & mask;
				uint b = (color >> 0) & mask;
				UniqueColors.Add(Color.FromArgb(255, (int)r, (int)g, (int)b));
			}
			UniqueColors = UniqueColors.Distinct().ToList();
			while (UniqueColors.Count > NrOutputColors - (FirstTransparent ? 1 : 0))
			{
				float mindiff = float.MaxValue;
				int amin = -1;
				int bmin = -1;
				for (int a = 0; a < UniqueColors.Count; a++)
				{
					for (int c = a + 1; c < UniqueColors.Count; c++)
					{
						//if (a == c) continue;
						float diff = ColorDifference(UniqueColors[a], UniqueColors[c]);
						if (diff < mindiff)
						{
							mindiff = diff;
							amin = a;
							bmin = c;
							//if (diff < 2) goto go;
							//if (diff < 30) goto go;
							if (diff < 700) goto go;
							//if (diff < 650) goto go;
							//if (diff < 900) goto go;//30) goto go;
						}
					}
				}
			go:
				Color e = ColorMean(UniqueColors[amin], UniqueColors[bmin]);
				UniqueColors.RemoveAt(amin);
				if (bmin > amin) bmin--;
				UniqueColors.RemoveAt(bmin);
				UniqueColors.Add(e);
			}
			if (FirstTransparent)// && Colors[0].A != 0)
			{
				UniqueColors.Insert(0, Color.Transparent);
			}
			UniqueColors.AddRange(new Color[NrOutputColors - UniqueColors.Count]);
			List<uint> Result = new List<uint>();
			foreach (Color c in UniqueColors)
			{
				Result.Add((uint)c.ToArgb());
			}
			return Result.ToArray();
		}

		private static int ColorDifference(Color a, Color b)
		{
			int rr = a.R - b.R;
			int gg = a.G - b.G;
			int bb = a.B - b.B;
			return rr * rr + gg * gg + bb * bb;
		}

		private static Color ColorMean(Color a, Color b)
		{
			return Color.FromArgb((a.R + b.R) / 2, (a.G + b.G) / 2, (a.B + b.B) / 2);
		}
	}
}
