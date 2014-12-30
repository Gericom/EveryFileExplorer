using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using LibEveryFileExplorer.GFX;
using LibEveryFileExplorer.IO;

namespace NDS.GPU
{
	public class Textures
	{
		public enum ImageFormat : uint
		{
			NONE = 0,
			A3I5 = 1,
			PLTT4 = 2,
			PLTT16 = 3,
			PLTT256 = 4,
			COMP4x4 = 5,
			A5I3 = 6,
			DIRECT = 7
		}
		public enum CharFormat : uint
		{
			CHAR,
			BMP
		}
		public static Bitmap ToBitmap(byte[] Data, byte[] Palette, int PaletteNr, int Width, int Height, ImageFormat Type, CharFormat CharacterType/*, bool cut = true*/, bool firstTransparent = false)
		{
			return ToBitmap(Data, Palette, new byte[0], PaletteNr, Width, Height, Type, CharacterType, firstTransparent);//, cut);
		}
		public static Bitmap ToBitmap(byte[] Data, byte[] Palette, byte[] Tex4x4, int PaletteNr, int Width, int Height, ImageFormat Type, CharFormat CharacterType, bool firstTransparent = false)//, bool cut = true)
		{
			Bitmap b = null;
			int offset = 0;
			switch (Type)
			{
				case ImageFormat.NONE:
					break;
				case ImageFormat.A3I5:
					{
						Color[] pal = ConvertXBGR1555(Palette);
						b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
						BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
						int x = 0;
						int y = 0;
						foreach (byte bb in Data)
						{
							Color c = Color.FromArgb((((bb >> 5) << 2) + ((bb >> 5) >> 1)) * 8, pal[bb & 0x1F]);
							Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, c.ToArgb());
							if (x >= Width)
							{
								y++;
								x = 0;
							}
						}
						b.UnlockBits(d);
						break;
					}
				case ImageFormat.PLTT4:
					{
						Color[] pal = ConvertXBGR1555(Palette);
						if (firstTransparent)
						{
							pal[0] = Color.FromArgb(0, 0xFF, 0, 0);
						}
						if (CharacterType == CharFormat.CHAR)
						{
							b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);//b = new Bitmap((int)(Data.Length / 16) * 8, 8, PixelFormat.Format32bppArgb);
							BitmapData d = b.LockBits(new Rectangle(0, 0, /*(int)(Data.Length / 32) * 8, 8*/Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							for (int y = 0; y < Height; y += 8)
							{
								for (int x = 0; x < Width; x += 8)
								{
									for (int TileY = 0; TileY < 8; TileY++)
									{
										for (int TileX = 0; TileX < 2; TileX++)
										{
											byte by = Data[offset];
											int y2 = TileY + y;
											int x2 = TileX * 4 + x + 3;//(i * 8 + TileX * 4) + 3;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[(by & 3) + 4 * PaletteNr].ToArgb());
											x2 = TileX * 4 + x + 2;//(i * 8 + TileX * 4) + 2;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[(by >> 2 & 3) + 4 * PaletteNr].ToArgb());
											x2 = TileX * 4 + x + 1;//(i * 8 + TileX * 4) + 1;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[(by >> 4 & 3) + 4 * PaletteNr].ToArgb());
											x2 = TileX * 4 + x + 0;//i * 8 + TileX * 4;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[(by >> 6 & 3) + 4 * PaletteNr].ToArgb());
											offset++;

										}
									}
								}
							}
							b.UnlockBits(d);
						}
						else
						{
							b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
							BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							int x = 0;
							int y = 0;
							foreach (byte bb in Data)
							{
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[(bb & 3) + 4 * PaletteNr].ToArgb());
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[(bb >> 2 & 3) + 4 * PaletteNr].ToArgb());
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[(bb >> 4 & 3) + 4 * PaletteNr].ToArgb());
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[(bb >> 6 & 3) + 4 * PaletteNr].ToArgb());
								if (x >= Width)
								{
									y++;
									x = 0;
								}
							}
							b.UnlockBits(d);
						}
						break;
					}
				case ImageFormat.PLTT16:
					{
						Color[] pal = ConvertXBGR1555(Palette);
						if (firstTransparent)
						{
							pal[0] = Color.FromArgb(0, 0xFF, 0, 0);
						}
						if (CharacterType == CharFormat.CHAR)
						{
							b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
							BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							for (int y = 0; y < Height; y += 8)
							{
								for (int x = 0; x < Width; x += 8)
								{
									for (int TileY = 0; TileY < 8; TileY++)
									{
										for (int TileX = 0; TileX < 4; TileX++)
										{
											byte by = Data[offset];
											int y2 = TileY + y;
											int x2 = TileX * 2 + x + 1; //(i * 8 + TileX * 2) + 1;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[(by / 16) + 16 * PaletteNr].ToArgb());
											x2 = TileX * 2 + x;//i * 8 + TileX * 2;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[(by % 16) + 16 * PaletteNr].ToArgb());
											offset++;
										}
									}
								}
							}
							b.UnlockBits(d);
						}
						else
						{
							b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
							BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							int x = 0;
							int y = 0;
							foreach (byte bb in Data)
							{
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[(bb % 16) + 16 * PaletteNr].ToArgb());
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[(bb / 16) + 16 * PaletteNr].ToArgb());
								if (x >= Width)
								{
									y++;
									x = 0;
								}
							}
							b.UnlockBits(d);
						}
						break;
					}
				case ImageFormat.PLTT256:
					{
						Color[] pal = ConvertXBGR1555(Palette);
						if (firstTransparent)
						{
							pal[0] = Color.FromArgb(0, 0xFF, 0, 0);
						}
						if (CharacterType == CharFormat.CHAR)
						{
							b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);//new Bitmap((int)(Data.Length / 64) * 8, 8, PixelFormat.Format32bppArgb);
							BitmapData d = b.LockBits(new Rectangle(0, 0, /*(int)(Data.Length / 64) * 8, 8*/Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							for (int y = 0; y < Height; y += 8)
							{
								for (int x = 0; x < Width; x += 8)
								{
									for (int TileY = 0; TileY < 8; TileY++)
									{
										for (int TileX = 0; TileX < 8; TileX++)
										{
											byte by = Data[offset];
											int y2 = TileY + y;
											int x2 = TileX + x;//i * 8 + TileX;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[by + 256 * PaletteNr].ToArgb());
											offset++;
										}
									}
								}
							}
							b.UnlockBits(d);
						}
						else
						{
							b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
							BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							int x = 0;
							int y = 0;
							foreach (byte bb in Data)
							{
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[bb + 256 * PaletteNr].ToArgb());
								if (x >= Width)
								{
									y++;
									x = 0;
								}
							}
							b.UnlockBits(d);
						}
						break;
					}
				case ImageFormat.COMP4x4:
					{
						Color[] pal = ConvertXBGR1555(Palette);
						b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
						BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
						int x = 0;
						int y = 0;
						for (int i = 0; i < Data.Length / 4; i++)
						{
							uint texel = IOUtil.ReadU32LE(Data, i * 4);
							ushort data = IOUtil.ReadU16LE(Tex4x4, i * 2);
							int address = data & 0x3fff;
							address = address << 1;
							bool PTY = (data >> 14 & 0x1) == 1;
							bool A = (data >> 15 & 0x1) == 1;
							int shift = 0;
							for (int j = 0; j < 4; j++)
							{
								for (int k = 0; k < 4; k++)
								{
									uint idx = (texel >> (shift * 2) & 0x3);
									Color c = new Color();
									if (!PTY && A)
									{
										c = pal[address + idx];
									}
									else if (!PTY && !A)
									{
										c = (idx != 3) ? pal[address + idx] : Color.Transparent;
									}
									else if (PTY && A)
									{
										switch (idx)
										{
											case 0:
											case 1:
												c = pal[address + idx];
												break;
											case 2:
												c = Color.FromArgb(
													(5 * pal[address + 0].R + 3 * pal[address + 1].R) / 8,
													(5 * pal[address + 0].G + 3 * pal[address + 1].G) / 8,
													(5 * pal[address + 0].B + 3 * pal[address + 1].B) / 8);
												break;
											case 3:
												c = Color.FromArgb(
													(3 * pal[address + 0].R + 5 * pal[address + 1].R) / 8,
													(3 * pal[address + 0].G + 5 * pal[address + 1].G) / 8,
													(3 * pal[address + 0].B + 5 * pal[address + 1].B) / 8);
												break;
										}
									}
									else if (PTY && !A)
									{
										switch (idx)
										{
											case 0:
											case 1:
												c = pal[address + idx];
												break;
											case 2:
												c = Color.FromArgb(
													(pal[address + 0].R + pal[address + 1].R) / 2,
													(pal[address + 0].G + pal[address + 1].G) / 2,
													(pal[address + 0].B + pal[address + 1].B) / 2);
												break;
											case 3:
												c = Color.Transparent;
												break;
										}
									}
									Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, c.ToArgb());
									shift++;
								}
								x -= 4;
								y++;
							}
							y -= 4;
							x += 4;
							if (x >= Width)
							{
								y += 4;
								x = 0;
							}
						}
						b.UnlockBits(d);
						break;
					}
				case ImageFormat.A5I3:
					{
						Color[] pal = ConvertXBGR1555(Palette);
						b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
						BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
						int x = 0;
						int y = 0;
						foreach (byte bb in Data)
						{
							Color c = Color.FromArgb((bb >> 3) * 8, pal[bb & 0x7]);
							Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, c.ToArgb());
							if (x >= Width)
							{
								y++;
								x = 0;
							}
						}
						b.UnlockBits(d);
						break;
					}
				case ImageFormat.DIRECT:
					{
						b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
						BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
						Color[] pal = ConvertABGR1555(Data);
						int x = 0;
						int y = 0;
						for (int i = 0; i < pal.Length; i++)
						{
							/*if (Data[i * 2 + 1] >> 7 == 0)
							{
								pal[i] = Color.FromArgb(0, pal[i]);
							}*/
							Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[i].ToArgb());
							if (x >= Width)
							{
								y++;
								x = 0;
							}
						}
						b.UnlockBits(d);
						break;
					}
			}
			//if (CharacterType == NNSG2dCharacterFmt.NNS_G2D_CHARACTER_FMT_CHAR && cut)
			//{
			//	b = CutImage(b, Width, 1);
			//}
			return b;
		}

		private static Color[] ConvertABGR1555(byte[] Data)
		{
			Color[] data = new Color[Data.Length / 2];
			for (int i = 0; i < Data.Length; i += 2)
			{
				data[i / 2] = Color.FromArgb((int)GFXUtil.ConvertColorFormat(IOUtil.ReadU16LE(Data, i), ColorFormat.ABGR1555, ColorFormat.ARGB8888));
			}
			return data;
		}

		private static Color[] ConvertXBGR1555(byte[] Data)
		{
			Color[] data = new Color[Data.Length / 2];
			for (int i = 0; i < Data.Length; i += 2)
			{
				data[i / 2] = Color.FromArgb((int)GFXUtil.ConvertColorFormat(IOUtil.ReadU16LE(Data, i), ColorFormat.XBGR1555, ColorFormat.ARGB8888));
			}
			return data;
		}
	}
}
