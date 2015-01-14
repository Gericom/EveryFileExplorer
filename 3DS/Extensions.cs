using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using LibEveryFileExplorer.IO;

namespace _3DS
{
	public static class Extensions
	{
		public static Color ReadColor8(this EndianBinaryReader er)
		{
			int r = er.ReadByte();
			int g = er.ReadByte();
			int b = er.ReadByte();
			int a = er.ReadByte();
			return Color.FromArgb(a, r, g, b);
		}

		public static Color ReadColor4Singles(this EndianBinaryReader er)
		{
			float R = er.ReadSingle();
			float G = er.ReadSingle();
			float B = er.ReadSingle();
			float A = er.ReadSingle();

			int r = (int)(0.5f + (R * 255f));
			int g = (int)(0.5f + (G * 255f));
			int b = (int)(0.5f + (B * 255f));
			int a = (int)(0.5f + (A * 255f));

			return Color.FromArgb(a, r, g, b);
		}

		public static void WriteColor8(this EndianBinaryWriter er, Color Value)
		{
			er.Write((byte)Value.R);
			er.Write((byte)Value.G);
			er.Write((byte)Value.B);
			er.Write((byte)Value.A);
		}

		public static void WriteColor4Singles(this EndianBinaryWriter er, Color Value)
		{
			er.Write((float)(Value.R / 255f));
			er.Write((float)(Value.G / 255f));
			er.Write((float)(Value.B / 255f));
			er.Write((float)(Value.A / 255f));
		}
	}
}
