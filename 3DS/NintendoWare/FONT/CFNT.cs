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
				Header = new CFNTHeader(er);
				FontInfo = new FINF(er);
				//TextureGlyph = new TGLP(er);
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
			public FINF(EndianBinaryReader er)//same as wii font's
			{
				Signature = er.ReadString(System.Text.Encoding.ASCII, 4);
				if (Signature != "FINF") throw new SignatureNotCorrectException(Signature, "FINF", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				FontType = er.ReadByte();
				LineFeed = er.ReadByte();
				AlterCharIndex = er.ReadUInt16();
				DefaultWidth = new CharWidths(er);
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
			public CharWidths DefaultWidth;
			public Byte Encoding;
			public UInt32 TGLPOffset;
			public UInt32 CWDHOffset;
			public UInt32 CMAPOffset;
			public Byte Height;
			public Byte Width;
			public Byte Ascent;
			public Byte Padding;//?
		}

		public class CharWidths
		{
			public CharWidths(EndianBinaryReader er)
			{
				Left = er.ReadByte();
				GlyphWidth = er.ReadByte();
				CharWidth = er.ReadByte();
			}
			public Byte Left;
			public Byte GlyphWidth;
			public Byte CharWidth;
		}

		/*public TGLP TextureGlyph;
		public class TGLP
		{
			public TGLP(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "TGLP") throw new SignatureNotCorrectException(Signature, "TGLP", er.BaseStream.Position - 4);
			}
			public String Signature;
		}*/

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
