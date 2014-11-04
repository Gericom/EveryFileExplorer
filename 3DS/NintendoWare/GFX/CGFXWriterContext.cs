using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Drawing;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.Collections;

namespace _3DS.NintendoWare.GFX
{
	public class CGFXWriterContext
	{
		private Dictionary<long, byte[]> IMAGBlockEntries = new Dictionary<long, byte[]>();
		private Dictionary<long, string> StringTableEntries = new Dictionary<long, string>();

		public void WriteDataReference(byte[] Data, EndianBinaryWriter er)
		{
			IMAGBlockEntries.Add(er.BaseStream.Position, Data);
			er.Write((uint)0);
		}

		public void WriteStringReference(String s, EndianBinaryWriter er)
		{
			StringTableEntries.Add(er.BaseStream.Position, s);
			er.Write((uint)0);
		}

		public void WriteStringTable(EndianBinaryWriter er)
		{
			Dictionary<String, long> strings = new Dictionary<string,long>();
			foreach (var v in StringTableEntries)
			{
				if (strings.ContainsKey(v.Value))
				{
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = v.Key;
					er.Write((uint)(strings[v.Value] - v.Key));
					er.BaseStream.Position = curpos;
				}
				else
				{
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = v.Key;
					er.Write((uint)(curpos - v.Key));
					er.BaseStream.Position = curpos;
					strings.Add(v.Value, curpos);
					er.Write(v.Value, Encoding.ASCII, true);
				}
			}
		}

		public bool DoWriteIMAGBlock()
		{
			return IMAGBlockEntries.Count > 0;
		}

		public int GetIMAGBlockSize()
		{
			int size = 0;//8;
			foreach (var v in IMAGBlockEntries)
			{
				size += v.Value.Length;
				while ((size % 128) != 0) size++;
			}
			while ((size % 8) != 0) size++;
			return size + 8;
		}

		public void WriteIMAGBlock(EndianBinaryWriter er)
		{
			//long basepos = er.BaseStream.Position;
			er.Write("IMAG", Encoding.ASCII, false);
			er.Write((uint)GetIMAGBlockSize());
			foreach (var v in IMAGBlockEntries)
			{
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = v.Key;
				er.Write((uint)(curpos - v.Key));
				er.BaseStream.Position = curpos;
				er.Write(v.Value, 0, v.Value.Length);
				while ((er.BaseStream.Position % 128) != 0) er.Write((byte)0);
			}
			while ((er.BaseStream.Position % 64) != 0) er.Write((byte)0);
		}

		public static uint CalcHash(byte[] Data)
		{
			uint num = 0;
			byte[] buffer = new MD5CryptoServiceProvider().ComputeHash(Data);
			for (int i = 0; i < buffer.Length; i++)
			{
				num ^= (uint)(buffer[i] << ((i % 4) * 8));
			}
			if (num == 0) return 1;
			return num;
		}

		public static void WriteFloatColorRGB(byte[] Data, int Offset, Vector4 Value)
		{
			IOUtil.WriteSingleLE(Data, Offset, Value.X);
			IOUtil.WriteSingleLE(Data, Offset + 4, Value.Y);
			IOUtil.WriteSingleLE(Data, Offset + 8, Value.Z);
		}

		public static void WriteFloatColorRGBA(byte[] Data, int Offset, Vector4 Value)
		{
			IOUtil.WriteSingleLE(Data, Offset, Value.X);
			IOUtil.WriteSingleLE(Data, Offset + 4, Value.Y);
			IOUtil.WriteSingleLE(Data, Offset + 8, Value.Z);
			IOUtil.WriteSingleLE(Data, Offset + 12, Value.W);
		}

	}
}
