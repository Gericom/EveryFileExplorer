using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.GameData;
using System.IO;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.Collections;
using System.Windows.Forms;
using LibEveryFileExplorer;
using System.ComponentModel;
using LibEveryFileExplorer.ComponentModel;
using System.Drawing;
using LibEveryFileExplorer.GFX;

namespace MarioKart.MKDS.NKM
{
	public class STAG
	{
		public STAG(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "STAG") throw new SignatureNotCorrectException(Signature, "STAG", er.BaseStream.Position - 4);
			Unknown1 = er.ReadUInt16();
			NrLaps = er.ReadInt16();
			Unknown2 = er.ReadByte();
			FogEnabled = er.ReadByte() == 1;
			FogTableGenMode = er.ReadByte();
			FogSlope = er.ReadByte();
			UnknownData1 = er.ReadBytes(0x8);
			FogDensity = er.ReadFx32();
			FogColor = Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(er.ReadUInt16()));
			FogAlpha = er.ReadUInt16();
			KclColor1 = Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(er.ReadUInt16()));
			KclColor2 = Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(er.ReadUInt16()));
			KclColor3 = Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(er.ReadUInt16()));
			KclColor4 = Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(er.ReadUInt16()));
			FrustumFar = er.ReadFx32();
			UnknownData2 = er.ReadBytes(0x4);
		}

		public String Signature;
		public UInt16 Unknown1;
		public Int16 NrLaps;
		public Byte Unknown2;
		public Boolean FogEnabled;
		public Byte FogTableGenMode;
		public Byte FogSlope;
		public byte[] UnknownData1;
		public Single FogDensity;
		public Color FogColor;
		public UInt16 FogAlpha;
		public Color KclColor1;
		public Color KclColor2;
		public Color KclColor3;
		public Color KclColor4;
		public Single FrustumFar;
		public byte[] UnknownData2;
	}
}
