using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.GFX
{
	//Based on: https://raw.githubusercontent.com/gp-b2g/frameworks_base/master/opengl/libs/ETC1/etc1.cpp
	public class ETC1
	{	
		private static readonly int[] kModifierTable = {
		/* 0 */2, 8, -2, -8,
		/* 1 */5, 17, -5, -17,
		/* 2 */9, 29, -9, -29,
		/* 3 */13, 42, -13, -42,
		/* 4 */18, 60, -18, -60,
		/* 5 */24, 80, -24, -80,
		/* 6 */33, 106, -33, -106,
		/* 7 */47, 183, -47, -183 };

		private static readonly int[] kLookup = { 0, 1, 2, 3, -4, -3, -2, -1 };

		private static byte clamp(int x)
		{
			return (byte)(x >= 0 ? (x < 255 ? x : 255) : 0);
		}

		private static int convert4To8(int b)
		{
			int c = b & 0xf;
			return (c << 4) | c;
		}

		private static int convert5To8(int b)
		{
			int c = b & 0x1f;
			return (c << 3) | (c >> 2);
		}

		private static int convert6To8(int b)
		{
			int c = b & 0x3f;
			return (c << 2) | (c >> 4);
		}

		private static int divideBy255(int d)
		{
			return (d + 128 + (d >> 8)) >> 8;
		}

		private static int convert8To4(int b)
		{
			int c = b & 0xff;
			return divideBy255(b * 15);
		}

		private static int convert8To5(int b)
		{
			int c = b & 0xff;
			return divideBy255(b * 31);
		}

		private static int convertDiff(int b, int diff)
		{
			return convert5To8((0x1f & b) + kLookup[0x7 & diff]);
		}

		private struct etc_compressed
		{
			public uint high;
			public uint low;
			public uint score; // Lower is more accurate
		}

		private static void take_best(ref etc_compressed a, etc_compressed b)
		{
			if (a.score > b.score) a = b;
		}

		private static void etc_average_colors_subblock(byte[] pIn, uint inMask, byte[] pColors, int pColorsOffset, bool flipped, bool second)
		{
			int r = 0;
			int g = 0;
			int b = 0;

			if (flipped)
			{
				int by = 0;
				if (second)
				{
					by = 2;
				}
				for (int y = 0; y < 2; y++)
				{
					int yy = by + y;
					for (int x = 0; x < 4; x++)
					{
						int i = x + 4 * yy;
						if ((inMask & (1 << i)) != 0)
						{
							r += pIn[i * 3];
							g += pIn[i * 3 + 1];
							b += pIn[i * 3 + 2];
						}
					}
				}
			}
			else
			{
				int bx = 0;
				if (second)
				{
					bx = 2;
				}
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 2; x++)
					{
						int xx = bx + x;
						int i = xx + 4 * y;
						if ((inMask & (1 << i)) != 0)
						{
							r += pIn[i * 3];
							g += pIn[i * 3 + 1];
							b += pIn[i * 3 + 2];
						}
					}
				}
			}
			pColors[0 + pColorsOffset] = (byte)((r + 4) >> 3);
			pColors[1 + pColorsOffset] = (byte)((g + 4) >> 3);
			pColors[2 + pColorsOffset] = (byte)((b + 4) >> 3);
		}

		private static int square(int x)
		{
			return x * x;
		}

		private static uint chooseModifier(byte[] pBaseColors, int pBaseColorsOffset, byte[] pIn, int pInOffset, ref uint pLow, int bitIndex, int[] pModifierTable, int pModifierTableOffset)
		{
			uint bestScore = ~0u;
			int bestIndex = 0;
			int pixelR = pIn[0 + pInOffset];
			int pixelG = pIn[1 + pInOffset];
			int pixelB = pIn[2 + pInOffset];
			int r = pBaseColors[0 + pBaseColorsOffset];
			int g = pBaseColors[1 + pBaseColorsOffset];
			int b = pBaseColors[2 + pBaseColorsOffset];
			for (int i = 0; i < 4; i++)
			{
				int modifier = pModifierTable[i + pModifierTableOffset];
				int decodedG = clamp(g + modifier);
				uint score = (uint)(6 * square(decodedG - pixelG));
				if (score >= bestScore)
				{
					continue;
				}
				int decodedR = clamp(r + modifier);
				score += (uint)(3 * square(decodedR - pixelR));
				if (score >= bestScore)
				{
					continue;
				}
				int decodedB = clamp(b + modifier);
				score += (uint)square(decodedB - pixelB);
				if (score < bestScore)
				{
					bestScore = score;
					bestIndex = i;
				}
			}
			uint lowMask = (uint)((((bestIndex >> 1) << 16) | (bestIndex & 1)) << bitIndex);
			pLow |= lowMask;
			return bestScore;
		}

		private static void etc_encode_subblock_helper(byte[] pIn, uint inMask, ref etc_compressed pCompressed, bool flipped, bool second, byte[] pBaseColors, int pBaseColorsOffset, int[] pModifierTable, int pModifierTableOffset)
		{
			int score = (int)pCompressed.score;
			if (flipped)
			{
				int by = 0;
				if (second)
				{
					by = 2;
				}
				for (int y = 0; y < 2; y++)
				{
					int yy = by + y;
					for (int x = 0; x < 4; x++)
					{
						int i = x + 4 * yy;
						if ((inMask & (1 << i)) != 0)
						{
							score += (int)chooseModifier(pBaseColors, pBaseColorsOffset, pIn, i * 3, ref pCompressed.low, yy + x * 4, pModifierTable, pModifierTableOffset);
						}
					}
				}
			}
			else
			{
				int bx = 0;
				if (second)
				{
					bx = 2;
				}
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 2; x++)
					{
						int xx = bx + x;
						int i = xx + 4 * y;
						if ((inMask & (1 << i)) != 0)
						{
							score += (int)chooseModifier(pBaseColors, pBaseColorsOffset, pIn, i * 3, ref pCompressed.low, y + xx * 4, pModifierTable, pModifierTableOffset);
						}
					}
				}
			}
			pCompressed.score = (uint)score;
		}

		private static bool inRange4bitSigned(int color)
		{
			return color >= -4 && color <= 3;
		}

		private static void etc_encodeBaseColors(byte[] pBaseColors, byte[] pColors, ref etc_compressed pCompressed)
		{
			int r1 = 0, g1 = 0, b1 = 0, r2 = 0, g2 = 0, b2 = 0; // 8 bit base colors for sub-blocks
			bool differential;
			{
				int r51 = convert8To5(pColors[0]);
				int g51 = convert8To5(pColors[1]);
				int b51 = convert8To5(pColors[2]);
				int r52 = convert8To5(pColors[3]);
				int g52 = convert8To5(pColors[4]);
				int b52 = convert8To5(pColors[5]);

				r1 = convert5To8(r51);
				g1 = convert5To8(g51);
				b1 = convert5To8(b51);

				int dr = r52 - r51;
				int dg = g52 - g51;
				int db = b52 - b51;

				differential = inRange4bitSigned(dr) && inRange4bitSigned(dg) && inRange4bitSigned(db);
				if (differential)
				{
					r2 = convert5To8(r51 + dr);
					g2 = convert5To8(g51 + dg);
					b2 = convert5To8(b51 + db);
					pCompressed.high |= (uint)((r51 << 27) | ((7 & dr) << 24) | (g51 << 19) | ((7 & dg) << 16) | (b51 << 11) | ((7 & db) << 8) | 2);
				}
			}

			if (!differential)
			{
				int r41 = convert8To4(pColors[0]);
				int g41 = convert8To4(pColors[1]);
				int b41 = convert8To4(pColors[2]);
				int r42 = convert8To4(pColors[3]);
				int g42 = convert8To4(pColors[4]);
				int b42 = convert8To4(pColors[5]);
				r1 = convert4To8(r41);
				g1 = convert4To8(g41);
				b1 = convert4To8(b41);
				r2 = convert4To8(r42);
				g2 = convert4To8(g42);
				b2 = convert4To8(b42);
				pCompressed.high |= (uint)((r41 << 28) | (r42 << 24) | (g41 << 20) | (g42 << 16) | (b41 << 12) | (b42 << 8));
			}
			pBaseColors[0] = (byte)r1;
			pBaseColors[1] = (byte)g1;
			pBaseColors[2] = (byte)b1;
			pBaseColors[3] = (byte)r2;
			pBaseColors[4] = (byte)g2;
			pBaseColors[5] = (byte)b2;
		}

		private static void etc_encode_block_helper(byte[] pIn, uint inMask, byte[] pColors, ref etc_compressed pCompressed, bool flipped)
		{
			pCompressed.score = ~0u;
			pCompressed.high = (flipped ? 1u : 0u);
			pCompressed.low = 0;

			byte[] pBaseColors = new byte[6];

			etc_encodeBaseColors(pBaseColors, pColors, ref pCompressed);

			uint originalHigh = pCompressed.high;

			int[] pModifierTable = kModifierTable;
			for (int i = 0; i < 8; i++)
			{
				etc_compressed temp;
				temp.score = 0;
				temp.high = originalHigh | (uint)(i << 5);
				temp.low = 0;
				etc_encode_subblock_helper(pIn, inMask, ref temp, flipped, false, pBaseColors, 0, pModifierTable, i * 4);
				take_best(ref pCompressed, temp);
			}
			pModifierTable = kModifierTable;
			etc_compressed firstHalf = pCompressed;
			for (int i = 0; i < 8; i++)
			{
				etc_compressed temp;
				temp.score = firstHalf.score;
				temp.high = firstHalf.high | (uint)(i << 2);
				temp.low = firstHalf.low;
				etc_encode_subblock_helper(pIn, inMask, ref temp, flipped, true, pBaseColors, 3, pModifierTable, i * 4);
				if (i == 0)
				{
					pCompressed = temp;
				}
				else
				{
					take_best(ref pCompressed, temp);
				}
			}
		}

		// Input is a 4 x 4 square of 3-byte pixels in form R, G, B
		// inmask is a 16-bit mask where bit (1 << (x + y * 4)) tells whether the corresponding (x,y)
		// pixel is valid or not. Invalid pixel color values are ignored when compressing.
		// Output is an ETC1 compressed version of the data.

		public static ulong etc1_encode_block(byte[] pIn, uint inMask = 0xFFFF)//, byte[] pOut)
		{
			byte[] colors = new byte[6];
			byte[] flippedColors = new byte[6];
			etc_average_colors_subblock(pIn, inMask, colors, 0, false, false);
			etc_average_colors_subblock(pIn, inMask, colors, 3, false, true);
			etc_average_colors_subblock(pIn, inMask, flippedColors, 0, true, false);
			etc_average_colors_subblock(pIn, inMask, flippedColors, 3, true, true);

			etc_compressed a = new etc_compressed(), b = new etc_compressed();
			etc_encode_block_helper(pIn, inMask, colors, ref a, false);
			etc_encode_block_helper(pIn, inMask, flippedColors, ref b, true);
			take_best(ref a, b);
			return ((ulong)a.high << 32) | (ulong)a.low;
		}
	}
}
