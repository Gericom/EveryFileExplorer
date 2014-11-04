using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.GFX;

namespace _3DS.GPU
{
	public class Textures
	{
		public enum ImageFormat : uint
		{
			RGBA8 = 0,
			RGB8 = 1,
			RGBA5551 = 2,
			RGB565 = 3,
			RGBA4 = 4,
			LA8 = 5,
			HILO8 = 6,
			L8 = 7,
			A8 = 8,
			LA4 = 9,
			L4 = 10,
			A4 = 11,
			ETC1 = 12,
			ETC1A4 = 13
		}

		private static readonly int[] Bpp = { 32, 24, 16, 16, 16, 16, 16, 8, 8, 8, 4, 4, 4, 8 };

		private static readonly int[] TileOrder =
		{
			 0,  1,   4,  5,
			 2,  3,   6,  7,

			 8,  9,  12, 13,  
			10, 11,  14, 15
		};

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

		public static int GetBpp(ImageFormat Format) { return Bpp[(uint)Format]; }

		public static Bitmap ToBitmap(byte[] Data, int Width, int Height, ImageFormat Format, bool ExactSize = false)
		{
			return ToBitmap(Data, 0, Width, Height, Format, ExactSize);
		}

		public static unsafe Bitmap ToBitmap(byte[] Data, int Offset, int Width, int Height, ImageFormat Format, bool ExactSize = false)
		{
			if (Data == null || Data.Length < 1 || Offset < 0 || Offset >= Data.Length || Width < 1 || Height < 1) return null;
			if (ExactSize && ((Width % 8) != 0 || (Height % 8) != 0)) return null;
			int physicalwidth = Width;
			int physicalheight = Height;
			if (!ExactSize)
			{
				Width = 1 << (int)Math.Ceiling(Math.Log(Width, 2));
				Height = 1 << (int)Math.Ceiling(Math.Log(Height, 2));
			}
			Bitmap bitm = new Bitmap(physicalwidth, physicalheight);
			BitmapData d = bitm.LockBits(new Rectangle(0, 0, bitm.Width, bitm.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			uint* res = (uint*)d.Scan0;
			int offs = Offset;//0;
			int stride = d.Stride / 4;
			switch (Format)
			{
				case ImageFormat.RGBA8:
					for (int y = 0; y < Height; y += 8)
					{
						for (int x = 0; x < Width; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								res[(y + y2) * stride + x + x2] = GFXUtil.ToArgb(
									Data[offs + pos * 4],
									Data[offs + pos * 4 + 3],
									Data[offs + pos * 4 + 2],
									Data[offs + pos * 4 + 1]
									);
							}
							offs += 64 * 4;
						}
					}
					break;
				case ImageFormat.RGB8:
					for (int y = 0; y < Height; y += 8)
					{
						for (int x = 0; x < Width; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								res[(y + y2) * stride + x + x2] = GFXUtil.ToArgb(
									Data[offs + pos * 3 + 2],
									Data[offs + pos * 3 + 1],
									Data[offs + pos * 3 + 0]
									);
							}
							offs += 64 * 3;
						}
					}
					break;
				case ImageFormat.RGBA5551:
					for (int y = 0; y < Height; y += 8)
					{
						for (int x = 0; x < Width; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								res[(y + y2) * stride + x + x2] =
									GFXUtil.ARGB1555ToArgb(IOUtil.ReadU16LE(Data, offs + pos * 2));
							}
							offs += 64 * 2;
						}
					}
					break;
				case ImageFormat.RGB565:
					for (int y = 0; y < Height; y += 8)
					{
						for (int x = 0; x < Width; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								res[(y + y2) * stride + x + x2] =
									GFXUtil.RGB565ToArgb(IOUtil.ReadU16LE(Data, offs + pos * 2));
							}
							offs += 64 * 2;
						}
					}
					break;
				case ImageFormat.RGBA4:
					for (int y = 0; y < Height; y += 8)
					{
						for (int x = 0; x < Width; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								res[(y + y2) * stride + x + x2] = GFXUtil.ToArgb(
									(byte)((Data[offs + pos * 2] & 0xF) * 0x11),
									(byte)((Data[offs + pos * 2 + 1] >> 4) * 0x11),
									(byte)((Data[offs + pos * 2 + 1] & 0xF) * 0x11),
									(byte)((Data[offs + pos * 2] >> 4) * 0x11)
									);
							}
							offs += 64 * 2;
						}
					}
					break;
				case ImageFormat.LA8:
					for (int y = 0; y < Height; y += 8)
					{
						for (int x = 0; x < Width; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								res[(y + y2) * stride + x + x2] = GFXUtil.ToArgb(
									Data[offs + pos * 2],
									Data[offs + pos * 2 + 1],
									Data[offs + pos * 2 + 1],
									Data[offs + pos * 2 + 1]
									);
							}
							offs += 64 * 2;
						}
					}
					break;
				case ImageFormat.HILO8:
					for (int y = 0; y < Height; y += 8)
					{
						for (int x = 0; x < Width; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								res[(y + y2) * stride + x + x2] = GFXUtil.ToArgb(
									Data[offs + pos * 2],
									Data[offs + pos * 2 + 1],
									Data[offs + pos * 2 + 1],
									Data[offs + pos * 2 + 1]
									);
							}
							offs += 64 * 2;
						}
					}
					break;
				case ImageFormat.L8:
					for (int y = 0; y < Height; y += 8)
					{
						for (int x = 0; x < Width; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								res[(y + y2) * stride + x + x2] = GFXUtil.ToArgb(
									Data[offs + pos],
									Data[offs + pos],
									Data[offs + pos]
									);
							}
							offs += 64;
						}
					}
					break;
				case ImageFormat.A8:
					for (int y = 0; y < Height; y += 8)
					{
						for (int x = 0; x < Width; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								res[(y + y2) * stride + x + x2] = GFXUtil.ToArgb(
									Data[offs + pos],
									255,
									255,
									255
									);
							}
							offs += 64;
						}
					}
					break;
				case ImageFormat.LA4:
					for (int y = 0; y < Height; y += 8)
					{
						for (int x = 0; x < Width; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								res[(y + y2) * stride + x + x2] = GFXUtil.ToArgb(
									(byte)((Data[offs + pos] & 0xF) * 0x11),
									(byte)((Data[offs + pos] >> 4) * 0x11),
									(byte)((Data[offs + pos] >> 4) * 0x11),
									(byte)((Data[offs + pos] >> 4) * 0x11)
									);
							}
							offs += 64;
						}
					}
					break;
				case ImageFormat.L4:
					for (int y = 0; y < Height; y += 8)
					{
						for (int x = 0; x < Width; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								int shift = (pos & 1) * 4;
								res[(y + y2) * stride + x + x2] = GFXUtil.ToArgb(
									(byte)(((Data[offs + pos / 2] >> shift) & 0xF) * 0x11),
									(byte)(((Data[offs + pos / 2] >> shift) & 0xF) * 0x11),
									(byte)(((Data[offs + pos / 2] >> shift) & 0xF) * 0x11)
									);
							}
							offs += 64 / 2;
						}
					}
					break;
				case ImageFormat.A4:
					for (int y = 0; y < Height; y += 8)
					{
						for (int x = 0; x < Width; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								int shift = (pos & 1) * 4;
								res[(y + y2) * stride + x + x2] = GFXUtil.ToArgb(
									(byte)(((Data[offs + pos / 2] >> shift) & 0xF) * 0x11),
									255,
									255,
									255
									);
							}
							offs += 64 / 2;
						}
					}
					break;
				case ImageFormat.ETC1://Some reference: http://www.khronos.org/registry/gles/extensions/OES/OES_compressed_ETC1_RGB8_texture.txt
				case ImageFormat.ETC1A4:
					{
						for (int y = 0; y < Height; y += 8)
						{
							for (int x = 0; x < Width; x += 8)
							{
								for (int i = 0; i < 8; i += 4)
								{
									for (int j = 0; j < 8; j += 4)
									{
										ulong alpha = 0xFFFFFFFFFFFFFFFF;
										if (Format == ImageFormat.ETC1A4)
										{
											alpha = IOUtil.ReadU64LE(Data, offs);
											offs += 8;
										}
										ulong data = IOUtil.ReadU64LE(Data, offs);
										bool diffbit = ((data >> 33) & 1) == 1;
										bool flipbit = ((data >> 32) & 1) == 1; //0: |||, 1: |-|
										int r1, r2, g1, g2, b1, b2;
										if (diffbit) //'differential' mode
										{
											int r = (int)((data >> 59) & 0x1F);
											int g = (int)((data >> 51) & 0x1F);
											int b = (int)((data >> 43) & 0x1F);
											r1 = (r << 3) | ((r & 0x1C) >> 2);
											g1 = (g << 3) | ((g & 0x1C) >> 2);
											b1 = (b << 3) | ((b & 0x1C) >> 2);
											r += (int)((data >> 56) & 0x7) << 29 >> 29;
											g += (int)((data >> 48) & 0x7) << 29 >> 29;
											b += (int)((data >> 40) & 0x7) << 29 >> 29;
											r2 = (r << 3) | ((r & 0x1C) >> 2);
											g2 = (g << 3) | ((g & 0x1C) >> 2);
											b2 = (b << 3) | ((b & 0x1C) >> 2);
										}
										else //'individual' mode
										{
											r1 = (int)((data >> 60) & 0xF) * 0x11;
											g1 = (int)((data >> 52) & 0xF) * 0x11;
											b1 = (int)((data >> 44) & 0xF) * 0x11;
											r2 = (int)((data >> 56) & 0xF) * 0x11;
											g2 = (int)((data >> 48) & 0xF) * 0x11;
											b2 = (int)((data >> 40) & 0xF) * 0x11;
										}
										int Table1 = (int)((data >> 37) & 0x7);
										int Table2 = (int)((data >> 34) & 0x7);
										for (int y3 = 0; y3 < 4; y3++)
										{
											for (int x3 = 0; x3 < 4; x3++)
											{
												if (x + j + x3 >= physicalwidth) continue;
												if (y + i + y3 >= physicalheight) continue;

												int val = (int)((data >> (x3 * 4 + y3)) & 0x1);
												bool neg = ((data >> (x3 * 4 + y3 + 16)) & 0x1) == 1;
												uint c;
												if ((flipbit && y3 < 2) || (!flipbit && x3 < 2))
												{
													int add = ETC1Modifiers[Table1, val] * (neg ? -1 : 1);
													c = GFXUtil.ToArgb((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r1 + add), (byte)ColorClamp(g1 + add), (byte)ColorClamp(b1 + add));
												}
												else
												{
													int add = ETC1Modifiers[Table2, val] * (neg ? -1 : 1);
													c = GFXUtil.ToArgb((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r2 + add), (byte)ColorClamp(g2 + add), (byte)ColorClamp(b2 + add));
												}
												res[(i + y3) * stride + x + j + x3] = c;
											}
										}
										offs += 8;
									}
								}
							}
							res += stride * 8;
						}
					}
					break;
			}
			bitm.UnlockBits(d);
			return bitm;
		}

		public static unsafe byte[] FromBitmap(Bitmap Picture, ImageFormat Format, bool ExactSize = false)
		{
			if (ExactSize && ((Picture.Width % 8) != 0 || (Picture.Height % 8) != 0)) return null;
			int physicalwidth = Picture.Width;
			int physicalheight = Picture.Height;
			int ConvWidth = Picture.Width;
			int ConvHeight = Picture.Height;
			if (!ExactSize)
			{
				ConvWidth = 1 << (int)Math.Ceiling(Math.Log(Picture.Width, 2));
				ConvHeight = 1 << (int)Math.Ceiling(Math.Log(Picture.Height, 2));
			}
			BitmapData d = Picture.LockBits(new Rectangle(0, 0, Picture.Width, Picture.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			uint* res = (uint*)d.Scan0;
			byte[] result = new byte[ConvWidth * ConvHeight * GetBpp(Format) / 8];
			int offs = 0;
			switch (Format)
			{
				case ImageFormat.RGB565:
					for (int y = 0; y < ConvHeight; y += 8)
					{
						for (int x = 0; x < ConvWidth; x += 8)
						{
							for (int i = 0; i < 64; i++)
							{
								int x2 = i % 8;
								if (x + x2 >= physicalwidth) continue;
								int y2 = i / 8;
								if (y + y2 >= physicalheight) continue;
								int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
								IOUtil.WriteU16LE(result, offs + pos * 2, GFXUtil.ArgbToRGB565(res[(y + y2) * d.Stride / 4 + x + x2]));
							}
							offs += 64 * 2;
						}
					}
					break;
				case ImageFormat.ETC1:
				case ImageFormat.ETC1A4:
					for (int y = 0; y < ConvHeight; y += 8)
					{
						for (int x = 0; x < ConvWidth; x += 8)
						{
							for (int i = 0; i < 8; i += 4)
							{
								for (int j = 0; j < 8; j += 4)
								{
									if (Format == ImageFormat.ETC1A4)
									{
										ulong alpha = 0;
										int iiii = 0;
										for (int xx = 0; xx < 4; xx++)
										{
											for (int yy = 0; yy < 4; yy++)
											{
												uint color = res[((y + i + yy) * (d.Stride / 4)) + x + j + xx];
												uint a = color >> 24;
												a >>= 4;
												alpha |= (ulong)a << (iiii * 4);
												iiii++;
											}
										}
										IOUtil.WriteU64LE(result, offs, alpha);
										offs += 8;
									}
									/*
									ulong data = 0;
									//TODO: Choose the best configuration
									bool diffbit = false;//((data >> 33) & 1) == 1;
									bool flipbit = false;// ((data >> 32) & 1) == 1;
									data |= (flipbit ? 1ul : 0ul) << 32;
									data |= (diffbit ? 1ul : 0ul) << 33;
									//Get the colors for the left side:
									uint[] Left = new uint[8];
									uint avrgRL = 0;
									uint avrgGL = 0;
									uint avrgBL = 0;
									int iii = 0;
									for (int yy = 0; yy < 4; yy++)
									{
										for (int xx = 0; xx < 2; xx++)
										{
											uint color = res[((y + i + yy) * (d.Stride / 4)) + x + j + xx];
											avrgRL += (color >> 16) & 0xFF;
											avrgGL += (color >> 8) & 0xFF;
											avrgBL += (color >> 0) & 0xFF;
											Left[iii++] = color;
										}
									}
									avrgRL /= 8;
									avrgGL /= 8;
									avrgBL /= 8;

									ulong r1 = avrgRL / 0x11;
									ulong g1 = avrgGL / 0x11;
									ulong b1 = avrgBL / 0x11;

									float[] LeftDeltas = new float[8];
									//float LeftMax = int.MinValue;
									//float LeftMin = int.MaxValue;
									float deltamean = 0;
									iii = 0;
									foreach (uint c in Left)
									{
										uint r = (c >> 16) & 0xFF;
										uint g = (c >> 8) & 0xFF;
										uint b = (c >> 0) & 0xF;

										LeftDeltas[iii] = ((int)(r - (r1 * 0x11)) + (int)(g - (g1 * 0x11)) + (int)(b - (b1 * 0x11))) / 3f;
										deltamean += LeftDeltas[iii];
										//if (Math.Abs(LeftDeltas[iii]) > LeftMax) LeftMax = Math.Abs(LeftDeltas[iii]);
										//if (Math.Abs(LeftDeltas[iii]) < LeftMin) LeftMin = Math.Abs(LeftDeltas[iii]);
										iii++;
									}
									deltamean /= 8;
									/*avrgRL = (uint)((int)avrgRL + deltamean);
									if ((int)avrgRL > 255) avrgRL = 255;
									if ((int)avrgRL < 0) avrgRL = 0;
									avrgGL = (uint)((int)avrgGL + deltamean);
									if ((int)avrgGL > 255) avrgGL = 255;
									if ((int)avrgGL < 0) avrgGL = 0;
									avrgBL = (uint)((int)avrgBL + deltamean);
									if ((int)avrgBL > 255) avrgBL = 255;
									if ((int)avrgBL < 0) avrgBL = 0;*/

									/*while (avrgRL > 183 && avrgGL > 183 && avrgBL > 183)
									{
										avrgRL--;
										avrgGL--;
										avrgBL--;
									}/
									r1 = avrgRL / 0x11;
									g1 = avrgGL / 0x11;
									b1 = avrgBL / 0x11;
									data |= r1 << 60;
									data |= g1 << 52;
									data |= b1 << 44;
									//Get the colors for the right side:
									uint[] Right = new uint[8];
									uint avrgRR = 0;
									uint avrgGR = 0;
									uint avrgBR = 0;
									iii = 0;
									for (int yy = 0; yy < 4; yy++)
									{
										for (int xx = 2; xx < 4; xx++)
										{
											uint color = res[((y + i + yy) * (d.Stride / 4)) + x + j + xx];
											avrgRR += (color >> 16) & 0xFF;
											avrgGR += (color >> 8) & 0xFF;
											avrgBR += (color >> 0) & 0xFF;
											Right[iii++] = color;
										}
									}
									avrgRR /= 8;
									avrgGR /= 8;
									avrgBR /= 8;

									ulong r2 = avrgRR / 0x11;
									ulong g2 = avrgGR / 0x11;
									ulong b2 = avrgBR / 0x11;


									float[] RightDeltas = new float[8];
									deltamean = 0;
									iii = 0;
									foreach (uint c in Right)
									{
										uint r = (c >> 16) & 0xFF;
										uint g = (c >> 8) & 0xFF;
										uint b = (c >> 0) & 0xF;
										RightDeltas[iii] = ((int)(r - (r2 * 0x11)) + (int)(g - (g2 * 0x11)) + (int)(b - (b2 * 0x11))) / 3f;
										deltamean += RightDeltas[iii];
										iii++;
									}
									deltamean /= 8;
								/*	avrgRR = (uint)((int)avrgRR + deltamean);
									if ((int)avrgRR > 255) avrgRR = 255;
									if ((int)avrgRR < 0) avrgRR = 0;
									avrgGR = (uint)((int)avrgGR + deltamean);
									if ((int)avrgGR > 255) avrgGR = 255;
									if ((int)avrgGR < 0) avrgGR = 0;
									avrgBR = (uint)((int)avrgBR + deltamean);
									if ((int)avrgBR > 255) avrgBR = 255;
									if ((int)avrgBR < 0) avrgBR = 0;/

									r2 = avrgRR / 0x11;
									g2 = avrgGR / 0x11;
									b2 = avrgBR / 0x11;
									data |= r2 << 56;
									data |= g2 << 48;
									data |= b2 << 40;
									//Calulate Deltas
									LeftDeltas = new float[8];
									float LeftMax = float.MinValue;
									float LeftMin = float.MaxValue;
									iii = 0;
									foreach (uint c in Left)
									{
										uint r = (c >> 16) & 0xFF;
										uint g = (c >> 8) & 0xFF;
										uint b = (c >> 0) & 0xF;

										LeftDeltas[iii] = ((int)(r - (r1 * 0x11)) + (int)(g - (g1 * 0x11)) + (int)(b - (b1 * 0x11))) / 3f;
										if (Math.Abs(LeftDeltas[iii]) > LeftMax) LeftMax = Math.Abs(LeftDeltas[iii]);
										if (Math.Abs(LeftDeltas[iii]) < LeftMin) LeftMin = Math.Abs(LeftDeltas[iii]);
										iii++;
									}
									int tblidxmax = ClosestTable((int)LeftMax);
									int tblidxmin = ClosestTable((int)LeftMin);
									uint Table1 = (uint)tblidxmin;//(uint)((tblidxmax + tblidxmin) / 2);
									RightDeltas = new float[8];
									float RightMax = int.MinValue;
									float RightMin = int.MaxValue;
									iii = 0;
									foreach (uint c in Right)
									{
										uint r = (c >> 16) & 0xFF;
										uint g = (c >> 8) & 0xFF;
										uint b = (c >> 0) & 0xF;
										RightDeltas[iii] = ((int)(r - (r2 * 0x11)) + (int)(g - (g2 * 0x11)) + (int)(b - (b2 * 0x11))) / 3f;
										if (Math.Abs(RightDeltas[iii]) > RightMax) RightMax = Math.Abs(RightDeltas[iii]);
										if (Math.Abs(RightDeltas[iii]) < RightMin) RightMin = Math.Abs(RightDeltas[iii]);
										iii++;
									}
									tblidxmax = ClosestTable((int)RightMax);
									tblidxmin = ClosestTable((int)RightMin);
									uint Table2 = (uint)tblidxmin;//(uint)((tblidxmax + tblidxmin) / 2);
									//Set the tables
									//uint Table1 = 0;
									//uint Table2 = 0;
									data |= (Table1 & 0x7) << 37;
									data |= (Table2 & 0x7) << 34;
									iii = 0;
									for (int yy = 0; yy < 4; yy++)
									{
										for (int xx = 0; xx < 2; xx++)
										{
											if (LeftDeltas[iii] < 0) data |= 1ul << (xx * 4 + yy + 16);
											float tbldiff1 = Math.Abs(LeftDeltas[iii]) - ETC1Modifiers[Table1, 0];
											float tbldiff2 = Math.Abs(LeftDeltas[iii]) - ETC1Modifiers[Table1, 1];
											if (Math.Abs(tbldiff2) < Math.Abs(tbldiff1)) data |= 1ul << (xx * 4 + yy);
											iii++;
										}
									}
									iii = 0;
									for (int yy = 0; yy < 4; yy++)
									{
										for (int xx = 2; xx < 4; xx++)
										{
											if (RightDeltas[iii] < 0) data |= 1ul << (xx * 4 + yy + 16);
											float tbldiff1 = Math.Abs(RightDeltas[iii]) - ETC1Modifiers[Table2, 0];
											float tbldiff2 = Math.Abs(RightDeltas[iii]) - ETC1Modifiers[Table2, 1];
											if (Math.Abs(tbldiff2) < Math.Abs(tbldiff1)) data |= 1ul << (xx * 4 + yy);
											iii++;
										}
									}

									//Write the data
									IOUtil.WriteU64LE(result, offs, data);
									offs += 8;*/
									byte[] Data = new byte[4 * 4 * 3];
									for (int yy = 0; yy < 4; yy++)
									{
										for (int xx = 0; xx < 4; xx++)
										{
											uint color = res[((y + i + yy) * (d.Stride / 4)) + x + j + xx];
											Data[(yy * 4 + xx) * 3] = (byte)((color >> 16) & 0xFF);
											Data[(yy * 4 + xx) * 3 + 1] = (byte)((color >> 8) & 0xFF);
											Data[(yy * 4 + xx) * 3 + 2] = (byte)((color >> 0) & 0xFF);
										}
									}
									IOUtil.WriteU64LE(result, offs, ETC1.etc1_encode_block(Data));
									offs += 8;
								}
							}
						}
					}
					break;
				default:
					throw new NotImplementedException("This format is not implemented yet.");
			}
			return result;
		}

		private static int ClosestTable(int Value)
		{
			int delta = int.MaxValue;
			int curtable = -1;
			for (int i = 0; i < 8; i++)
			{
				if (Math.Abs(Value - ETC1Modifiers[i, 0]) < delta)
				{
					delta = Math.Abs(Value - ETC1Modifiers[i, 0]);
					curtable = i;
				}
				if (Math.Abs(Value - ETC1Modifiers[i, 1]) < delta)
				{
					delta = Math.Abs(Value - ETC1Modifiers[i, 1]);
					curtable = i;
				}
			}
			return curtable;
		}

		private static int ColorClamp(int Color)
		{
			if (Color > 255) Color = 255;
			if (Color < 0) Color = 0;
			return Color;
		}


	}
}
