using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LibEveryFileExplorer.GFX
{
	public class DTX
	{
		public static uint[] DecodeDTX1(ushort Color0, ushort Color1, uint Data)
		{
			uint[] Palette = new uint[4];
			Palette[0] = GFXUtil.ConvertColorFormat(Color0, ColorFormat.RGB565, ColorFormat.ARGB8888);
			Palette[1] = GFXUtil.ConvertColorFormat(Color1, ColorFormat.RGB565, ColorFormat.ARGB8888);
			Color a = Color.FromArgb((int)Palette[0]);
			Color b = Color.FromArgb((int)Palette[1]);
			if (Color0 > Color1)//1/3 and 2/3
			{
				Palette[2] = GFXUtil.ToColorFormat((a.R * 2 + b.R * 1) / 3, (a.G * 2 + b.G * 1) / 3, (a.B * 2 + b.B * 1) / 3, ColorFormat.ARGB8888);
				Palette[3] = GFXUtil.ToColorFormat((a.R * 1 + b.R * 2) / 3, (a.G * 1 + b.G * 2) / 3, (a.B * 1 + b.B * 2) / 3, ColorFormat.ARGB8888);
			}
			else//1/2 and transparent
			{
				Palette[2] = GFXUtil.ToColorFormat((a.R + b.R) / 2, (a.G + b.G) / 2, (a.B + b.B) / 2, ColorFormat.ARGB8888);
				Palette[3] = 0;
			}

			uint[] Result = new uint[4 * 4];

			int q = 30;
			for (int y = 0; y < 4; y++)
			{
				for (int x = 0; x < 4; x++)
				{
					Result[y * 4 + x] = Palette[(Data >> q) & 3];
					q -= 2;
				}
			}
			return Result;
		}

		public static uint[] DecodeDTX5(ushort Color0, ushort Color1, uint Data, ulong AData)
		{
			uint[] Result = DecodeDTX1(Color0, Color1, Data);
			byte[] AlphaPalette = new byte[8];
			AlphaPalette[0] = (byte)(AData & 0xFF);
			AlphaPalette[1] = (byte)((AData >> 8) & 0xFF);
			AData >>= 16;
			if (AlphaPalette[0] > AlphaPalette[1])
			{
				AlphaPalette[2] = (byte)((6 * AlphaPalette[0] + 1 * AlphaPalette[1]) / 7);
				AlphaPalette[3] = (byte)((5 * AlphaPalette[0] + 2 * AlphaPalette[1]) / 7);
				AlphaPalette[4] = (byte)((4 * AlphaPalette[0] + 3 * AlphaPalette[1]) / 7);
				AlphaPalette[5] = (byte)((3 * AlphaPalette[0] + 4 * AlphaPalette[1]) / 7);
				AlphaPalette[6] = (byte)((2 * AlphaPalette[0] + 5 * AlphaPalette[1]) / 7);
				AlphaPalette[7] = (byte)((1 * AlphaPalette[0] + 6 * AlphaPalette[1]) / 7);
			}
			else
			{
				AlphaPalette[2] = (byte)((4 * AlphaPalette[0] + 1 * AlphaPalette[1]) / 5);
				AlphaPalette[3] = (byte)((3 * AlphaPalette[0] + 2 * AlphaPalette[1]) / 5);
				AlphaPalette[4] = (byte)((2 * AlphaPalette[0] + 3 * AlphaPalette[1]) / 5);
				AlphaPalette[5] = (byte)((1 * AlphaPalette[0] + 4 * AlphaPalette[1]) / 5);
				AlphaPalette[6] = 0;
				AlphaPalette[7] = 255;
			}
			int aq = 45;
			for (int i = 0; i < 16; i++)
			{
				Result[i] = (Result[i] & 0xFFFFFF) | ((uint)AlphaPalette[(AData >> aq) & 7] << 24);
				aq -= 3;
			}
			return Result;
		}
	}
}
