using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using _3DS.GPU;

namespace _3DS.NintendoWare.FONT
{
	public class CFNT : FileFormat<CFNT.CFNTIdentifier>, IViewable
	{
		public CFNT(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new CFNTHeader(er);
				FontInfo = new FINF(er);
				er.BaseStream.Position = FontInfo.TGLPOffset - 8;
				TextureGlyph = new TGLP(er);

				List<CWDH> tmp = new List<CWDH>();
				er.BaseStream.Position = FontInfo.CWDHOffset - 8;
				CWDH Last;
				do
				{
					Last = new CWDH(er);
					tmp.Add(Last);
					if (Last.NextCWDHOffset != 0) er.BaseStream.Position = Last.NextCWDHOffset - 8;
				}
				while (Last.NextCWDHOffset != 0);
				CharWidths = tmp.ToArray();

				List<CMAP> tmp2 = new List<CMAP>();
				er.BaseStream.Position = FontInfo.CMAPOffset - 8;
				CMAP Last2;
				do
				{
					Last2 = new CMAP(er);
					tmp2.Add(Last2);
					if (Last2.NextCMAPOffset != 0) er.BaseStream.Position = Last2.NextCMAPOffset - 8;
				}
				while (Last2.NextCMAPOffset != 0);
				CharMaps = tmp2.ToArray();
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

		public CFNTHeader Header;
		public class CFNTHeader
		{
			public CFNTHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "CFNT") throw new SignatureNotCorrectException(Signature, "CFNT", er.BaseStream.Position - 4);
				Endianness = er.ReadUInt16();
				HeaderSize = er.ReadUInt16();
				Version = er.ReadUInt32();
				FileSize = er.ReadUInt32();
				NrBlocks = er.ReadUInt32();
			}
			public String Signature;
			public UInt16 Endianness;
			public UInt16 HeaderSize;
			public UInt32 Version;
			public UInt32 FileSize;
			public UInt32 NrBlocks;
		}

		public FINF FontInfo;
		public class FINF
		{
			public FINF(EndianBinaryReader er)
			{
				Signature = er.ReadString(System.Text.Encoding.ASCII, 4);
				if (Signature != "FINF") throw new SignatureNotCorrectException(Signature, "FINF", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				FontType = er.ReadByte();
				LineFeed = er.ReadByte();
				AlterCharIndex = er.ReadUInt16();
				DefaultWidth = new CharWidthInfo(er);
				Encoding = er.ReadByte();
				TGLPOffset = er.ReadUInt32();
				CWDHOffset = er.ReadUInt32();
				CMAPOffset = er.ReadUInt32();
				Height = er.ReadByte();
				Width = er.ReadByte();
				Ascent = er.ReadByte();
				Padding = er.ReadByte();
			}
			public String Signature;
			public UInt32 SectionSize;
			public Byte FontType;
			public Byte LineFeed;
			public UInt16 AlterCharIndex;
			public CharWidthInfo DefaultWidth;
			public Byte Encoding;
			public UInt32 TGLPOffset;
			public UInt32 CWDHOffset;
			public UInt32 CMAPOffset;
			public Byte Height;
			public Byte Width;
			public Byte Ascent;
			public Byte Padding;//?
		}

		public TGLP TextureGlyph;
		public class TGLP
		{
			public TGLP(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "TGLP") throw new SignatureNotCorrectException(Signature, "TGLP", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				CellWidth = er.ReadByte();
				CellHeight = er.ReadByte();
				BaselinePos = er.ReadByte();
				MaxCharWidth = er.ReadByte();
				SheetSize = er.ReadUInt32();
				NrSheets = er.ReadUInt16();
				SheetFormat = (Textures.ImageFormat)er.ReadUInt16();
				SheetRow = er.ReadUInt16();
				SheetLine = er.ReadUInt16();
				SheetWidth = er.ReadUInt16();
				SheetHeight = er.ReadUInt16();
				SheetDataOffset = er.ReadUInt32();

				er.BaseStream.Position = SheetDataOffset;
				Sheets = new byte[NrSheets][];
				for (int i = 0; i < NrSheets; i++) Sheets[i] = er.ReadBytes((int)SheetSize);
			}
			public String Signature;
			public UInt32 SectionSize;
			public Byte CellWidth;
			public Byte CellHeight;
			public Byte BaselinePos;
			public Byte MaxCharWidth;
			public UInt32 SheetSize;
			public UInt16 NrSheets;
			public Textures.ImageFormat SheetFormat;
			public UInt16 SheetRow;
			public UInt16 SheetLine;
			public UInt16 SheetWidth;
			public UInt16 SheetHeight;
			public UInt32 SheetDataOffset;

			public Byte[][] Sheets;

			public Bitmap GetSheet(int Index)
			{
				if (Index >= NrSheets) return null;
				return Textures.ToBitmap(Sheets[Index], SheetWidth, SheetHeight, SheetFormat);
			}
		}

		public CWDH[] CharWidths;
		public class CWDH
		{
			public CWDH(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "CWDH") throw new SignatureNotCorrectException(Signature, "CWDH", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				StartIndex = er.ReadUInt16();
				EndIndex = er.ReadUInt16();
				NextCWDHOffset = er.ReadUInt32();

				CharWidths = new CharWidthInfo[EndIndex - StartIndex];
				for (int i = 0; i < EndIndex - StartIndex; i++) CharWidths[i] = new CharWidthInfo(er);
			}
			public String Signature;
			public UInt32 SectionSize;
			public UInt16 StartIndex;
			public UInt16 EndIndex;
			public UInt32 NextCWDHOffset;

			public CharWidthInfo[] CharWidths;
		}

		public CMAP[] CharMaps;
		public class CMAP
		{
			public CMAP(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "CMAP") throw new SignatureNotCorrectException(Signature, "CMAP", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				CodeBegin = er.ReadUInt16();
				CodeEnd = er.ReadUInt16();
				MappingMethod = er.ReadUInt16();
				Reserved = er.ReadUInt16();
				NextCMAPOffset = er.ReadUInt32();
			}
			public String Signature;
			public UInt32 SectionSize;
			public UInt16 CodeBegin;
			public UInt16 CodeEnd;
			public UInt16 MappingMethod;
			public UInt16 Reserved;
			public UInt32 NextCMAPOffset;
		}

		public class CharWidthInfo
		{
			public CharWidthInfo(EndianBinaryReader er)
			{
				Left = er.ReadByte();
				GlyphWidth = er.ReadByte();
				CharWidth = er.ReadByte();
			}
			public Byte Left;
			public Byte GlyphWidth;
			public Byte CharWidth;
		}

		public class CFNTIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Fonts;
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
