using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LibEveryFileExplorer.GFX
{
	public class GFXUtil
	{
		/// <summary>
		/// Converts the given color to the default 32bpp color format used in C#. (System.Drawing.Imaging.PixelFormat.Format32bppArgb)
		/// </summary>
		/// <param name="Color">The XBGR1555 color to convert.</param>
		/// <returns></returns>
		public static uint XBGR1555ToArgb(ushort Color)
		{
			return ToArgb(
				255,
				(byte)((Color & 0x1F) * 8),
				(byte)(((Color >> 5) & 0x1F) * 8),
				(byte)(((Color >> 10) & 0x1F) * 8)
				);
		}

		/// <summary>
		/// Converts the given color to the default 32bpp color format used in C#. (System.Drawing.Imaging.PixelFormat.Format32bppArgb)
		/// </summary>
		/// <param name="Color">The ABGR1555 color to convert.</param>
		/// <returns></returns>
		public static uint ABGR1555ToArgb(ushort Color)
		{
			return ToArgb(
				(byte)((Color >> 15) * 255),
				(byte)((Color & 0x1F) * 8),
				(byte)(((Color >> 5) & 0x1F) * 8),
				(byte)(((Color >> 10) & 0x1F) * 8)
				);
		}

		/// <summary>
		/// Converts the given color to the default 32bpp color format used in C#. (System.Drawing.Imaging.PixelFormat.Format32bppArgb)
		/// </summary>
		/// <param name="Color">The RGBA5551 color to convert.</param>
		/// <returns></returns>
		public static uint ARGB1555ToArgb(ushort Color)
		{
			return ToArgb(
				(byte)((Color >> 15) * 255),
				(byte)(((Color >> 10) & 0x1F) * 8),
				(byte)(((Color >> 5) & 0x1F) * 8),
				(byte)((Color & 0x1F) * 8)
				);
		}

		/// <summary>
		/// Converts the given color to the default 32bpp color format used in C#. (System.Drawing.Imaging.PixelFormat.Format32bppArgb)
		/// </summary>
		/// <param name="Color">The RGB565 color to convert.</param>
		/// <returns></returns>
		public static uint RGB565ToArgb(ushort Color)
		{
			return ToArgb(
				255,
				(byte)(((Color >> 11) & 0x1F) * 8),
				(byte)(((Color >> 5) & 0x3F) * 4),
				(byte)((Color & 0x1F) * 8)
				);
		}

		/// <summary>
		/// Converts the given color to RGB565.
		/// </summary>
		/// <param name="Color">The color to convert in the default 32bpp color format used in C#. (System.Drawing.Imaging.PixelFormat.Format32bppArgb)</param>
		/// <returns></returns>
		public static ushort ArgbToRGB565(uint Color)
		{
			uint A = (Color >> 24) & 0xFF;
			uint R = (Color >> 16) & 0xFF;
			uint G = (Color >> 8) & 0xFF;
			uint B = (Color >> 0) & 0xFF;
			if (A < 128) { R = 0; G = 0; B = 0; }
			return (ushort)((((R >> 3) << 11) | ((G >> 2) << 5) | ((B >> 3) << 0)) & 0xFFFF);
		}

		/// <summary>
		/// Converts the given color to the default 32bpp color format used in C# with an alpha value of 255. (System.Drawing.Imaging.PixelFormat.Format32bppArgb)
		/// </summary>
		/// <param name="R">Red</param>
		/// <param name="G">Green</param>
		/// <param name="B">Blue</param>
		/// <returns></returns>
		public static uint ToArgb(byte R, byte G, byte B)
		{
			return ToArgb(255, R, G, B);
		}

		/// <summary>
		/// Converts the given color to the default 32bpp color format used in C#. (System.Drawing.Imaging.PixelFormat.Format32bppArgb)
		/// </summary>
		/// <param name="A">Alpha</param>
		/// <param name="R">Red</param>
		/// <param name="G">Green</param>
		/// <param name="B">Blue</param>
		/// <returns></returns>
		public static uint ToArgb(byte A, byte R, byte G, byte B)
		{
			return (uint)(A << 24 | R << 16 | G << 8 | B);
		}

		public static uint SetArgbAlpha(uint Argb, byte A)
		{
			return (uint)((uint)A << 24 | (Argb & 0xFFFFFF));
		}

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
	}
}
