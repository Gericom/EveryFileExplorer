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
		public STAG()
		{
			Signature = "STAG";
			Unknown1 = 0;
			NrLaps = 3;
			Unknown2 = 0;
			FogEnabled = false;
			FogTableGenMode = 0;
			FogSlope = 0;
			UnknownData1 = new byte[8];
			FogDensity = 0;
			FogColor = Color.White;
			FogAlpha = 0;
			KclColor1 = Color.White;
			KclColor2 = Color.White;
			KclColor3 = Color.White;
			KclColor4 = Color.White;
			FrustumFar = 0;
			UnknownData2 = new byte[4];
		}
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

		public void Write(EndianBinaryWriter er)
		{
			er.Write(Signature, Encoding.ASCII, false);
			er.Write(Unknown1);
			er.Write(NrLaps);
			er.Write(Unknown2);
			er.Write((Byte)(FogEnabled ? 1 : 0));
			er.Write(FogTableGenMode);
			er.Write(FogSlope);
			er.Write(UnknownData1, 0, 8);
			er.WriteFx32(FogDensity);
			er.Write((UInt16)(GFXUtil.ArgbToXBGR1555((uint)FogColor.ToArgb()) | 0x8000));
			er.Write(FogAlpha);
			er.Write(GFXUtil.ArgbToXBGR1555((uint)KclColor1.ToArgb()));
			er.Write(GFXUtil.ArgbToXBGR1555((uint)KclColor2.ToArgb()));
			er.Write(GFXUtil.ArgbToXBGR1555((uint)KclColor3.ToArgb()));
			er.Write(GFXUtil.ArgbToXBGR1555((uint)KclColor4.ToArgb()));
			er.WriteFx32(FrustumFar);
			er.Write(UnknownData2, 0, 4);
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
