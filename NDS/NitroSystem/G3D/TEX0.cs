using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.Files;
using NDS.GPU;
using System.Drawing;

namespace NDS.NitroSystem.G3D
{
	public class TEX0
	{
		public TEX0()
		{
			Signature = "TEX0";
		}
		public TEX0(EndianBinaryReader er)
		{
			long basepos = er.BaseStream.Position;
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "TEX0") throw new SignatureNotCorrectException(Signature, "TEX0", er.BaseStream.Position - 4);
			SectionSize = er.ReadUInt32();
			TexInfo = new texInfo(er);
			Tex4x4Info = new tex4x4Info(er);
			PlttInfo = new plttInfo(er);
			dictTex = new Dictionary<DictTexData>(er);
			for (int i = 0; i < dictTex.numEntry; i++)
			{
				dictTex[i].Value.ReadData(er, TexInfo.ofsTex, Tex4x4Info.ofsTex, Tex4x4Info.ofsTexPlttIdx, basepos);
			}
			dictPltt = new Dictionary<DictPlttData>(er);
			List<UInt32> Offset = new List<uint>();
			for (int i = 0; i < dictPltt.numEntry; i++)
			{
				Offset.Add(dictPltt[i].Value.offset);
			}
			Offset = Offset.Distinct().ToList();
			Offset.Sort();
			for (int i = 0; i < dictPltt.numEntry; i++)
			{
				int idx = Offset.IndexOf(dictPltt[i].Value.offset);
				if (idx == Offset.Count - 1)
				{
					dictPltt[i].Value.ReadData(er, PlttInfo.ofsPlttData, (uint)er.BaseStream.Length - (Offset[idx] + PlttInfo.ofsPlttData + (uint)basepos), basepos);
				}
				else
				{
					dictPltt[i].Value.ReadData(er, PlttInfo.ofsPlttData, Offset[idx + 1] - Offset[idx], basepos);
				}
			}
		}

		public void Write(EndianBinaryWriter er)
		{
			long offpos = er.BaseStream.Position;
			//header.Write(er, 0);
			er.Write(Signature, Encoding.ASCII, false);
			er.Write((uint)0);

			List<byte> TexData = new List<byte>();
			List<byte> Tex4x4Data = new List<byte>();
			List<byte> Tex4x4PlttIdxData = new List<byte>();
			foreach (DictTexData d in dictTex.entry.data)
			{
				if (d.Fmt != Textures.ImageFormat.COMP4x4) TexData.AddRange(d.Data);
				else
				{
					Tex4x4Data.AddRange(d.Data);
					Tex4x4PlttIdxData.AddRange(d.Data4x4);
				}
			}
			List<byte> PaletteData = new List<byte>();
			foreach (DictPlttData d in dictPltt.entry.data)
			{
				PaletteData.AddRange(d.Data);
			}

			TexInfo.ofsDict = 60;
			TexInfo.sizeTex = (uint)TexData.Count;
			TexInfo.ofsTex = (UInt32)(60 + 8 + (dictTex.numEntry + 1) * 4 + 4 + dictTex.numEntry * 8 + dictTex.numEntry * 16 + 8 + (dictPltt.numEntry + 1) * 4 + 4 + dictPltt.numEntry * 4 + dictPltt.numEntry * 16);
			Tex4x4Info.ofsDict = 60;
			Tex4x4Info.sizeTex = (uint)Tex4x4Data.Count;
			Tex4x4Info.ofsTex = (UInt32)(60 + 8 + (dictTex.numEntry + 1) * 4 + 4 + dictTex.numEntry * 8 + dictTex.numEntry * 16 + 8 + (dictPltt.numEntry + 1) * 4 + 4 + dictPltt.numEntry * 4 + dictPltt.numEntry * 16 + TexData.Count);
			Tex4x4Info.ofsTexPlttIdx = (UInt32)(60 + 8 + (dictTex.numEntry + 1) * 4 + 4 + dictTex.numEntry * 8 + dictTex.numEntry * 16 + 8 + (dictPltt.numEntry + 1) * 4 + 4 + dictPltt.numEntry * 4 + dictPltt.numEntry * 16 + TexData.Count + Tex4x4Data.Count);
			PlttInfo.ofsDict = (ushort)(60 + 8 + (dictTex.numEntry + 1) * 4 + 4 + dictTex.numEntry * 8 + dictTex.numEntry * 16);
			PlttInfo.sizePltt = (uint)PaletteData.Count;
			PlttInfo.ofsPlttData = (UInt32)(60 + 8 + (dictTex.numEntry + 1) * 4 + 4 + dictTex.numEntry * 8 + dictTex.numEntry * 16 + 8 + (dictPltt.numEntry + 1) * 4 + 4 + dictPltt.numEntry * 4 + dictPltt.numEntry * 16 + TexData.Count + Tex4x4Data.Count + Tex4x4PlttIdxData.Count);
			TexInfo.Write(er);
			Tex4x4Info.Write(er);
			PlttInfo.Write(er);
			uint offset = 0;
			uint offset4x4 = 0;
			for (int i = 0; i < dictTex.numEntry; i++)
			{
				if (dictTex[i].Value.Fmt != Textures.ImageFormat.COMP4x4)
				{
					dictTex[i].Value.Offset = offset;
					offset += (uint)dictTex[i].Value.Data.Length;
				}
				else
				{
					dictTex[i].Value.Offset = offset4x4;
					offset4x4 += (uint)dictTex[i].Value.Data.Length;
				}
			}
			dictTex.Write(er);
			uint offsetPltt = 0;
			for (int i = 0; i < dictPltt.numEntry; i++)
			{
				dictPltt[i].Value.offset = offsetPltt;
				offsetPltt += (uint)dictPltt[i].Value.Data.Length;
			}
			dictPltt.Write(er);
			er.Write(TexData.ToArray(), 0, TexData.Count);
			er.Write(Tex4x4Data.ToArray(), 0, Tex4x4Data.Count);
			er.Write(Tex4x4PlttIdxData.ToArray(), 0, Tex4x4PlttIdxData.Count);
			er.Write(PaletteData.ToArray(), 0, PaletteData.Count);
			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = offpos + 4;
			er.Write((UInt32)(curpos - offpos));
			er.BaseStream.Position = curpos;
		}
		public String Signature;
		public UInt32 SectionSize;
		public texInfo TexInfo;
		public class texInfo
		{
			public texInfo(EndianBinaryReader er)
			{
				vramKey = er.ReadUInt32();
				sizeTex = (UInt32)(er.ReadUInt16() << 3);
				ofsDict = er.ReadUInt16();
				flag = er.ReadUInt16();
				er.ReadBytes(2);//PADDING(2 bytes);
				ofsTex = er.ReadUInt32();
			}
			public texInfo() { }
			public void Write(EndianBinaryWriter er)
			{
				er.Write(vramKey);
				er.Write((UInt16)(sizeTex >> 3));
				er.Write(ofsDict);
				er.Write(flag);
				er.Write((UInt16)0);//PADDING(2 bytes);
				er.Write(ofsTex);
			}
			public UInt32 vramKey;
			public UInt32 sizeTex;
			public UInt16 ofsDict;
			public UInt16 flag;

			public UInt32 ofsTex;
		}
		public tex4x4Info Tex4x4Info;
		public class tex4x4Info
		{
			public tex4x4Info(EndianBinaryReader er)
			{
				vramKey = er.ReadUInt32();
				sizeTex = (UInt32)(er.ReadUInt16() << 3);
				ofsDict = er.ReadUInt16();
				flag = er.ReadUInt16();
				er.ReadBytes(2);//PADDING(2 bytes);
				ofsTex = er.ReadUInt32();
				ofsTexPlttIdx = er.ReadUInt32();
			}
			public tex4x4Info() { }
			public void Write(EndianBinaryWriter er)
			{
				er.Write(vramKey);
				er.Write((UInt16)(sizeTex >> 3));
				er.Write(ofsDict);
				er.Write(flag);
				er.Write((UInt16)0);//PADDING(2 bytes);
				er.Write(ofsTex);
				er.Write(ofsTexPlttIdx);
			}
			public UInt32 vramKey;
			public UInt32 sizeTex;
			public UInt16 ofsDict;
			public UInt16 flag;

			public UInt32 ofsTex;
			public UInt32 ofsTexPlttIdx;
		}
		public plttInfo PlttInfo;
		public class plttInfo
		{
			public plttInfo(EndianBinaryReader er)
			{
				vramKey = er.ReadUInt32();
				sizePltt = (UInt32)(er.ReadUInt16() << 3);
				flag = er.ReadUInt16();
				ofsDict = er.ReadUInt16();
				er.ReadBytes(2);//PADDING(2 bytes);
				ofsPlttData = er.ReadUInt32();
			}
			public plttInfo() { }
			public void Write(EndianBinaryWriter er)
			{
				er.Write(vramKey);
				er.Write((UInt16)(sizePltt >> 3));
				er.Write(flag);
				er.Write(ofsDict);
				er.Write((UInt16)0);//PADDING(2 bytes);
				er.Write(ofsPlttData);
			}
			public UInt32 vramKey;
			public UInt32 sizePltt;
			public UInt16 flag;
			public UInt16 ofsDict;

			public UInt32 ofsPlttData;
		}
		public Dictionary<DictTexData> dictTex;
		public class DictTexData : DictionaryData
		{
			private int[] DataLength = { 0, 8, 2, 4, 8, 2, 8, 16 };
			public override ushort GetDataSize()
			{
				return 8;
			}
			public override void Read(EndianBinaryReader er)
			{
				texImageParam = er.ReadUInt32();
				Offset = (texImageParam & 0xFFFF) << 3;
				S = (UInt16)(8 << (int)((texImageParam & 0x700000) >> 20));
				T = (UInt16)(8 << (int)((texImageParam & 0x3800000) >> 23));
				Fmt = (Textures.ImageFormat)((texImageParam & 0x1C000000) >> 26);
				TransparentColor = ((texImageParam >> 29) & 1) == 1;
				extraParam = er.ReadUInt32();
			}
			public override void Write(EndianBinaryWriter er)
			{
				int newS = 0;
				int newT = 0;
				do newS++;
				while (8 << newS != S);
				do newT++;
				while (8 << newT != T);
				texImageParam = (UInt32)(((TransparentColor ? 1 : 0) << 29) | ((byte)Fmt << 26) | ((newT & 0x7) << 23) | ((newS & 0x7) << 20) | ((Offset >> 3) & 0xFFFF));
				er.Write(texImageParam);
				extraParam = (UInt32)((1 << 31) | ((T & 0x3FF) << 11) | ((S & 0x3FF) << 0));
				er.Write(extraParam);
			}
			public void ReadData(EndianBinaryReader er, uint BaseOffsetTex, uint BaseOffsetTex4x4, uint BaseOffsetTex4x4Info, long TexplttSetOffset)
			{
				long curpos = er.BaseStream.Position;
				if (Fmt == Textures.ImageFormat.COMP4x4)
				{
					er.BaseStream.Position = Offset + BaseOffsetTex4x4 + TexplttSetOffset;//er.GetMarker("TexplttSet");
					Data = er.ReadBytes((S * T * DataLength[(int)Fmt]) / 8);
					er.BaseStream.Position = Offset / 2 + BaseOffsetTex4x4Info + TexplttSetOffset;//er.GetMarker("TexplttSet");
					Data4x4 = er.ReadBytes((S * T * DataLength[(int)Fmt]) / 8 / 2);
				}
				else
				{
					er.BaseStream.Position = Offset + BaseOffsetTex + TexplttSetOffset;//er.GetMarker("TexplttSet");
					Data = er.ReadBytes((S * T * DataLength[(int)Fmt]) / 8);
				}
				er.BaseStream.Position = curpos;
			}
			public UInt32 texImageParam;
			public UInt32 Offset;
			public UInt16 S;
			public UInt16 T;
			public Textures.ImageFormat Fmt;
			public bool TransparentColor;

			public UInt32 extraParam;

			public byte[] Data;
			public byte[] Data4x4;

			public Bitmap ToBitmap(DictPlttData Palette)
			{
				return Textures.ToBitmap(Data, (Palette != null ? Palette.Data : null), Data4x4, 0, S, T, Fmt, Textures.CharFormat.BMP, TransparentColor);
			}
		}
		public Dictionary<DictPlttData> dictPltt;
		public class DictPlttData : DictionaryData
		{
			public override ushort GetDataSize()
			{
				return 4;
			}
			public override void Read(EndianBinaryReader er)
			{
				offset = (uint)(er.ReadUInt16() << 3);
				flag = er.ReadUInt16();
			}
			public override void Write(EndianBinaryWriter er)
			{
				er.Write((ushort)(offset >> 3));
				er.Write(flag);
			}
			public void ReadData(EndianBinaryReader er, uint BaseOffsetPltt, uint Length, long TexplttSetOffset)
			{
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = offset + BaseOffsetPltt + TexplttSetOffset;//er.GetMarker("TexplttSet");
				Data = er.ReadBytes((int)Length);
				er.BaseStream.Position = curpos;
			}
			public UInt32 offset;
			public UInt16 flag;

			public byte[] Data;
		}
	}
}
