using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using LibEveryFileExplorer.Files;
using System.Windows.Forms;
using System.IO;
using NDS.UI;

namespace NDS.NitroSystem.G3D
{
	public class NSBTX : FileFormat<NSBTX.NSBTXIdentifier>, IViewable
	{
		public NSBTX(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new NSBTXHeader(er);
				if (Header.NrBlocks > 0)
				{
					er.BaseStream.Position = Header.BlockOffsets[0];
					TexPlttSet = new TEX0(er);
				}
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new NSBTXViewer(this);
		}

		public NSBTXHeader Header;
		public class NSBTXHeader
		{
			public NSBTXHeader()
			{
				Signature = "BTX0";
				Endianness = 0xFEFF;
				Version = 1;
				HeaderSize = 16;
				NrBlocks = 1;
				BlockOffsets = new uint[NrBlocks];
			}
			public NSBTXHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "BTX0") throw new SignatureNotCorrectException(Signature, "BTX0", er.BaseStream.Position - 4);
				Endianness = er.ReadUInt16();
				Version = er.ReadUInt16();
				FileSize = er.ReadUInt32();
				HeaderSize = er.ReadUInt16();
				NrBlocks = er.ReadUInt16();
				BlockOffsets = er.ReadUInt32s(NrBlocks);
			}
			public String Signature;
			public UInt16 Endianness;
			public UInt16 Version;
			public UInt32 FileSize;
			public UInt16 HeaderSize;
			public UInt16 NrBlocks;
			public UInt32[] BlockOffsets;
		}
		public TEX0 TexPlttSet;
		public class NSBTXIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Textures;
			}

			public override string GetFileDescription()
			{
				return "Nitro System Binary Texture (NSBTX)";
			}

			public override string GetFileFilter()
			{
				return "Nitro System Binary Texture (*.nsbtx)|*.nsbtx";
			}

			public override Bitmap GetIcon()
			{
				return Resource.image_sunset;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'B' && File.Data[1] == 'T' && File.Data[2] == 'X' && File.Data[3] == '0') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
