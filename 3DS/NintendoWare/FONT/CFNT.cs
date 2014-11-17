using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace _3DS.NintendoWare.FONT
{
	public class CFNT : FileFormat<CFNT.CFNTIdentifier>, IViewable
	{
		public CFNT(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				header = new CFNTHeader(er);
				fontInfo = new FontInfo(er);
				tglplHeader = new TGLPLHeader(er);
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new Form();
		}

		CFNTHeader header;
		public class CFNTHeader
		{
			public CFNTHeader(EndianBinaryReader er)
			{
				signature = er.ReadString(Encoding.ASCII, 4);
				if (signature != "CFNT") throw new SignatureNotCorrectException(signature, "CFNT", er.BaseStream.Position - 4);

				littleEndian = er.ReadUInt16() == 65279;
				version = er.ReadUInt32();//20 on internal sysmenu
				er.ReadBytes(2);
				fileSize = er.ReadUInt32();
				sectionCount = er.ReadUInt32();

				Console.WriteLine("little Endian? " + littleEndian);
				Console.WriteLine("version " + version);
				Console.WriteLine("fileSize " + fileSize);
				Console.WriteLine("sectionCount " + sectionCount);


			}

			public String signature;
			public bool littleEndian;
			public UInt32 version;
			public UInt32 fileSize;// at 0xC
			public UInt32 sectionCount;
		}

		FontInfo fontInfo;
		public class FontInfo
		{
			public FontInfo(EndianBinaryReader er)//same as wii font's
			{
				signature = er.ReadString(Encoding.ASCII, 4);
				if (signature != "FINF") throw new SignatureNotCorrectException(signature, "FINF", er.BaseStream.Position - 4);

				sectionSize = er.ReadUInt32();
				fontType = er.ReadByte();
				leading = er.ReadByte();
				er.ReadUInt16();
				leftMargin = er.ReadByte();
				er.ReadByte();
				er.ReadByte();
				er.ReadByte();
				tglplDataOffset = er.ReadUInt32();
				cwdhDataOffset = er.ReadUInt32();//above + tlgp size
				cmapDataOffset = er.ReadUInt32();//above + cwdh size
				height = er.ReadByte();
				width = er.ReadByte();
				ascender = er.ReadByte();
				descender = er.ReadByte();

				Console.WriteLine("tglplDataOffset " + tglplDataOffset);
				Console.WriteLine("cwdhDataOffset " + cwdhDataOffset);
				Console.WriteLine("cmapDataOffset " + cmapDataOffset);

			}

			public String signature;
			public UInt32 sectionSize;
			public Byte fontType;
			public Byte leading;//space between lines (unsure)
			public Byte leftMargin;
			public UInt32 tglplDataOffset;
			public UInt32 cwdhDataOffset;
			public UInt32 cmapDataOffset;
			public Byte height;
			public Byte width;
			public Byte ascender;
			public Byte descender;

		}

		TGLPLHeader tglplHeader;
		public class TGLPLHeader
		{
			public TGLPLHeader(EndianBinaryReader er)
			{
				signature = er.ReadString(Encoding.ASCII, 5);
				if (signature != "TGLPL") throw new SignatureNotCorrectException(signature, "TGLPL", er.BaseStream.Position - 5);
				/*
				sectionLength = er.ReadUInt32();

				fontWidthMinus1 = er.ReadByte();
				fontHeightMinus1 = er.ReadByte();

				charWidthMinus1 = er.ReadByte();
				charHeightMinus1 = er.ReadByte();

				imageLength = er.ReadUInt32();

				imageAmount = er.ReadUInt16();

				*/

			}
			public String signature;

		}

		public class CFNTIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return "CFNT Files";
			}

			public override string GetFileDescription()
			{
				return "CTR Font (CFNT)";
			}

			public override string GetFileFilter()
			{
				return "CTR Font (*.bcfnt)|*.bcfnt";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 0x10 && File.Data[0] == 'C' && File.Data[1] == 'F' && File.Data[2] == 'N' && File.Data[3] == 'T') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}

	}
}
