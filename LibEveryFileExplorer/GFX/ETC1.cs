using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LibEveryFileExplorer.GFX
{
	public class ETC1
	{
		private static readonly int[,] ETC1Modifiers = 
		{	
			{ 2, 8 },
			{ 5, 17 },
			{ 9, 29 },
			{ 13, 42 },
			{ 18, 60 },
			{ 24, 80 },
			{ 33, 106 },
			{ 47, 183 }
		};

		private static int GenModifier(out Color BaseColor, Color[] Pixels)
		{
			Color Max = Color.White;
			//Color LessMax;
			//Color MoreMin;
			Color Min = Color.Black;
			int MinY = int.MaxValue;
			int MaxY = int.MinValue;
			for (int i = 0; i < 8; i++)
			{
				if (Pixels[i].A == 0) continue;
				int Y = (Pixels[i].R + Pixels[i].G + Pixels[i].B) / 3;
				if (Y > MaxY)
				{
					MaxY = Y;
					Max = Pixels[i];
				}
				if (Y < MinY)
				{
					MinY = Y;
					Min = Pixels[i];
				}
			}
			/*LessMax = Max;
			MoreMin = Min;
			int MoreMinY = int.MaxValue;
			int LessMaxY = int.MinValue;
			for (int i = 0; i < 8; i++)
			{
				if (Pixels[i].A == 0) continue;
				int Y = (Pixels[i].R + Pixels[i].G + Pixels[i].B) / 3;
				if (Y > MinY && Y < MaxY && Y > LessMaxY)
				{
					LessMaxY = Y;
					LessMax = Pixels[i];
				}
				if (Y > MinY && Y < MaxY && Y < MoreMinY)
				{
					MoreMinY = Y;
					MoreMin = Pixels[i];
				}
			}*/

			int DiffMean = ((Max.R - Min.R) + (Max.G - Min.G) + (Max.B - Min.B)) / 3;
			//int DiffMeanSmall = ((LessMax.R - MoreMin.R) + (LessMax.G - MoreMin.G) + (LessMax.B - MoreMin.B)) / 3;

			int ModDiff = int.MaxValue;
			int Modifier = -1;
			int Mode = -1;

			//int ModDiffSmall = int.MaxValue;
			//int ModifierSmall = -1;

			for (int i = 0; i < 8; i++)
			{
				int SS = ETC1Modifiers[i, 0] * 2;
				int SB = ETC1Modifiers[i, 0] + ETC1Modifiers[i, 1];
				int BB = ETC1Modifiers[i, 1] * 2;
				if (SS > 255) SS = 255;
				if (SB > 255) SB = 255;
				if (BB > 255) BB = 255;
				if (System.Math.Abs(DiffMean - SS) < ModDiff)
				{
					ModDiff = System.Math.Abs(DiffMean - SS);
					Modifier = i;
					Mode = 0;
				}
				if (System.Math.Abs(DiffMean - SB) < ModDiff)
				{
					ModDiff = System.Math.Abs(DiffMean - SB);
					Modifier = i;
					Mode = 1;
				}
				if (System.Math.Abs(DiffMean - BB) < ModDiff)
				{
					ModDiff = System.Math.Abs(DiffMean - BB);
					Modifier = i;
					Mode = 2;
				}

				/*if (System.Math.Abs(DiffMeanSmall - SS) < ModDiffSmall)
				{
					ModDiffSmall = System.Math.Abs(DiffMeanSmall - SS);
					ModifierSmall = i;
				}
				if (System.Math.Abs(DiffMeanSmall - SB) < ModDiffSmall)
				{
					ModDiffSmall = System.Math.Abs(DiffMeanSmall - SB);
					ModifierSmall = i;
				}
				if (System.Math.Abs(DiffMeanSmall - BB) < ModDiffSmall)
				{
					ModDiffSmall = System.Math.Abs(DiffMeanSmall - BB);
					ModifierSmall = i;
				}*/
			}

			//Modifier = (Modifier + ModifierSmall) / 2;
			//if (Mode == 0 && Modifier != 0) Modifier--;

			if (Mode == 1)
			{
				float div1 = (float)ETC1Modifiers[Modifier, 0] / (float)ETC1Modifiers[Modifier, 1];
				float div2 = 1f - div1;
				BaseColor = Color.FromArgb((int)(Min.R * div1 + Max.R * div2), (int)(Min.G * div1 + Max.G * div2), (int)(Min.B * div1 + Max.B * div2));
			}
			else
			{
				/*int R = 0;
				int G = 0;
				int B = 0;
				int total = 0;
				for (int i = 0; i < 8; i++)
				{
					if (Pixels[i].A == 0) continue;
					total++;
					R += Pixels[i].R;
					G += Pixels[i].G;
					B += Pixels[i].B;
				}
				if (total == 0) BaseColor = Color.White;
				else BaseColor = Color.FromArgb(R / total, G / total, B / total); //*/
				BaseColor = Color.FromArgb((Min.R + Max.R) / 2, (Min.G + Max.G) / 2, (Min.B + Max.B) / 2);
			}
			return Modifier;
		}

		private static ulong GenHorizontal(Color[] Colors)
		{
			ulong data = 0;
			SetFlipMode(ref data, false);
			//Left
			Color[] Left = GetLeftColors(Colors);
			Color basec1;
			int mod = GenModifier(out basec1, Left);
			SetTable1(ref data, mod);
			GenPixDiff(ref data, Left, basec1, mod, 0, 2, 0, 4);
			//Right
			Color[] Right = GetRightColors(Colors);
			Color basec2;
			mod = GenModifier(out basec2, Right);
			SetTable2(ref data, mod);
			GenPixDiff(ref data, Right, basec2, mod, 2, 4, 0, 4);
			SetBaseColors(ref data, basec1, basec2);
			return data;
		}

		private static ulong GenVertical(Color[] Colors)
		{
			ulong data = 0;
			SetFlipMode(ref data, true);
			//Top
			Color[] Top = GetTopColors(Colors);
			Color basec1;
			int mod = GenModifier(out basec1, Top);
			SetTable1(ref data, mod);
			GenPixDiff(ref data, Top, basec1, mod, 0, 4, 0, 2);
			//Bottom
			Color[] Bottom = GetBottomColors(Colors);
			Color basec2;
			mod = GenModifier(out basec2, Bottom);
			SetTable2(ref data, mod);
			GenPixDiff(ref data, Bottom, basec2, mod, 0, 4, 2, 4);
			SetBaseColors(ref data, basec1, basec2);
			return data;
		}

		private static int GetScore(Color[] Original, Color[] Encode)
		{
			int Diff = 0;
			for (int i = 0; i < 4 * 4; i++)
			{
				Diff += System.Math.Abs(Encode[i].R - Original[i].R);
				Diff += System.Math.Abs(Encode[i].G - Original[i].G);
				Diff += System.Math.Abs(Encode[i].B - Original[i].B);
			}
			return Diff;
		}

		public static ulong GenETC1(Color[] Colors)
		{
			ulong Horizontal = GenHorizontal(Colors);
			ulong Vertical = GenVertical(Colors);
			int HorizontalScore = GetScore(Colors, DecodeETC1(Horizontal));
			int VerticalScore = GetScore(Colors, DecodeETC1(Vertical));
			return (HorizontalScore < VerticalScore) ? Horizontal : Vertical;
		}

		private static void GenPixDiff(ref ulong Data, Color[] Pixels, Color BaseColor, int Modifier, int XOffs, int XEnd, int YOffs, int YEnd)
		{
			int BaseMean = (BaseColor.R + BaseColor.G + BaseColor.B) / 3;
			int i = 0;
			for (int yy = YOffs; yy < YEnd; yy++)
			{
				for (int xx = XOffs; xx < XEnd; xx++)
				{
					int Diff = ((Pixels[i].R + Pixels[i].G + Pixels[i].B) / 3) - BaseMean;

					if (Diff < 0) Data |= 1ul << (xx * 4 + yy + 16);
					int tbldiff1 = System.Math.Abs(Diff) - ETC1Modifiers[Modifier, 0];
					int tbldiff2 = System.Math.Abs(Diff) - ETC1Modifiers[Modifier, 1];

					if (System.Math.Abs(tbldiff2) < System.Math.Abs(tbldiff1)) Data |= 1ul << (xx * 4 + yy);
					i++;
				}
			}
		}

		private static Color[] GetLeftColors(Color[] Pixels)
		{
			Color[] Left = new Color[4 * 2];
			for (int y = 0; y < 4; y++)
			{
				for (int x = 0; x < 2; x++)
				{
					Left[y * 2 + x] = Pixels[y * 4 + x];
				}
			}
			return Left;
		}

		private static Color[] GetRightColors(Color[] Pixels)
		{
			Color[] Right = new Color[4 * 2];
			for (int y = 0; y < 4; y++)
			{
				for (int x = 2; x < 4; x++)
				{
					Right[y * 2 + x - 2] = Pixels[y * 4 + x];
				}
			}
			return Right;
		}

		private static Color[] GetTopColors(Color[] Pixels)
		{
			Color[] Top = new Color[4 * 2];
			for (int y = 0; y < 2; y++)
			{
				for (int x = 0; x < 4; x++)
				{
					Top[y * 4 + x] = Pixels[y * 4 + x];
				}
			}
			return Top;
		}

		private static Color[] GetBottomColors(Color[] Pixels)
		{
			Color[] Bottom = new Color[4 * 2];
			for (int y = 2; y < 4; y++)
			{
				for (int x = 0; x < 4; x++)
				{
					Bottom[(y - 2) * 4 + x] = Pixels[y * 4 + x];
				}
			}
			return Bottom;
		}

		private static void SetFlipMode(ref ulong Data, bool Mode)
		{
			Data &= ~(1ul << 32);
			Data |= (Mode ? 1ul : 0ul) << 32;
		}

		private static void SetDiffMode(ref ulong Data, bool Mode)
		{
			Data &= ~(1ul << 33);
			Data |= (Mode ? 1ul : 0ul) << 33;
		}

		private static void SetTable1(ref ulong Data, int Table)
		{
			Data &= ~(7ul << 37);
			Data |= (ulong)(Table & 0x7) << 37;
		}

		private static void SetTable2(ref ulong Data, int Table)
		{
			Data &= ~(7ul << 34);
			Data |= (ulong)(Table & 0x7) << 34;
		}

		private static void SetBaseColors(ref ulong Data, Color Color1, Color Color2)
		{
			int R1 = Color1.R;
			int G1 = Color1.G;
			int B1 = Color1.B;
			int R2 = Color2.R;
			int G2 = Color2.G;
			int B2 = Color2.B;
			//First look if differencial is possible.
			int RDiff = (R2 - R1) / 8;
			int GDiff = (G2 - G1) / 8;
			int BDiff = (B2 - B1) / 8;
			if (RDiff > -4 && RDiff < 3 && GDiff > -4 && GDiff < 3 && BDiff > -4 && BDiff < 3)
			{
				SetDiffMode(ref Data, true);
				R1 /= 8;
				G1 /= 8;
				B1 /= 8;
				Data |= (ulong)R1 << 59;
				Data |= (ulong)G1 << 51;
				Data |= (ulong)B1 << 43;
				Data |= (ulong)(RDiff & 0x7) << 56;
				Data |= (ulong)(GDiff & 0x7) << 48;
				Data |= (ulong)(BDiff & 0x7) << 40;
			}
			else
			{
				Data |= (ulong)(R1 / 0x11) << 60;
				Data |= (ulong)(G1 / 0x11) << 52;
				Data |= (ulong)(B1 / 0x11) << 44;

				Data |= (ulong)(R2 / 0x11) << 56;
				Data |= (ulong)(G2 / 0x11) << 48;
				Data |= (ulong)(B2 / 0x11) << 40;
			}
		}

		public static Color[] DecodeETC1(ulong Data, ulong Alpha = ~0ul)
		{
			Color[] Result = new Color[4 * 4];
			bool diffbit = ((Data >> 33) & 1) == 1;
			bool flipbit = ((Data >> 32) & 1) == 1; //0: |||, 1: |-|
			int r1, r2, g1, g2, b1, b2;
			if (diffbit) //'differential' mode
			{
				int r = (int)((Data >> 59) & 0x1F);
				int g = (int)((Data >> 51) & 0x1F);
				int b = (int)((Data >> 43) & 0x1F);
				r1 = (r << 3) | ((r & 0x1C) >> 2);
				g1 = (g << 3) | ((g & 0x1C) >> 2);
				b1 = (b << 3) | ((b & 0x1C) >> 2);
				r += (int)((Data >> 56) & 0x7) << 29 >> 29;
				g += (int)((Data >> 48) & 0x7) << 29 >> 29;
				b += (int)((Data >> 40) & 0x7) << 29 >> 29;
				r2 = (r << 3) | ((r & 0x1C) >> 2);
				g2 = (g << 3) | ((g & 0x1C) >> 2);
				b2 = (b << 3) | ((b & 0x1C) >> 2);
			}
			else //'individual' mode
			{
				r1 = (int)((Data >> 60) & 0xF) * 0x11;
				g1 = (int)((Data >> 52) & 0xF) * 0x11;
				b1 = (int)((Data >> 44) & 0xF) * 0x11;
				r2 = (int)((Data >> 56) & 0xF) * 0x11;
				g2 = (int)((Data >> 48) & 0xF) * 0x11;
				b2 = (int)((Data >> 40) & 0xF) * 0x11;
			}
			int Table1 = (int)((Data >> 37) & 0x7);
			int Table2 = (int)((Data >> 34) & 0x7);
			for (int y3 = 0; y3 < 4; y3++)
			{
				for (int x3 = 0; x3 < 4; x3++)
				{
					//if (x + j + x3 >= physicalwidth) continue;
					//if (y + i + y3 >= physicalheight) continue;

					int val = (int)((Data >> (x3 * 4 + y3)) & 0x1);
					bool neg = ((Data >> (x3 * 4 + y3 + 16)) & 0x1) == 1;
					uint c;
					if ((flipbit && y3 < 2) || (!flipbit && x3 < 2))
					{
						int add = ETC1Modifiers[Table1, val] * (neg ? -1 : 1);
						c = GFXUtil.ToColorFormat((byte)(((Alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r1 + add), (byte)ColorClamp(g1 + add), (byte)ColorClamp(b1 + add), ColorFormat.ARGB8888);
					}
					else
					{
						int add = ETC1Modifiers[Table2, val] * (neg ? -1 : 1);
						c = GFXUtil.ToColorFormat((byte)(((Alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r2 + add), (byte)ColorClamp(g2 + add), (byte)ColorClamp(b2 + add), ColorFormat.ARGB8888);
					}
					Result[y3 * 4 + x3] = Color.FromArgb((int)c);
					//res[(i + y3) * stride + x + j + x3] = c;
				}
			}
			return Result;
		}

		private static int ColorClamp(int Color)
		{
			if (Color > 255) Color = 255;
			if (Color < 0) Color = 0;
			return Color;
		}
	}
}
