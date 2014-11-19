using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using _3DS.GPU;
using LibEveryFileExplorer.GFX;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using _3DS.UI;

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
			return new CFNTViewer(this);
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
				SheetNrRows = er.ReadUInt16();
				SheetNrLines = er.ReadUInt16();
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
			public UInt16 SheetNrRows;
			public UInt16 SheetNrLines;
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

				CharWidths = new CharWidthInfo[EndIndex - StartIndex + 1];
				for (int i = 0; i < EndIndex - StartIndex + 1; i++) CharWidths[i] = new CharWidthInfo(er);
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
			public enum CMAPMappingMethod : ushort
			{
				Direct = 0,
				Table = 1,
				Scan = 2
			}

			public CMAP(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "CMAP") throw new SignatureNotCorrectException(Signature, "CMAP", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				CodeBegin = er.ReadUInt16();
				CodeEnd = er.ReadUInt16();
				MappingMethod = (CMAPMappingMethod)er.ReadUInt16();
				Reserved = er.ReadUInt16();
				NextCMAPOffset = er.ReadUInt32();

				switch (MappingMethod)
				{
					case CMAPMappingMethod.Direct:
						IndexOffset = er.ReadUInt16();
						break;
					case CMAPMappingMethod.Table:
						IndexTable = er.ReadUInt16s(CodeEnd - CodeBegin + 1);
						break;
					case CMAPMappingMethod.Scan:
						ScanEntries = new Dictionary<ushort, ushort>();
						NrScanEntries = er.ReadUInt16();
						for (int i = 0; i < NrScanEntries; i++) ScanEntries.Add(er.ReadUInt16(), er.ReadUInt16());
						break;
				}
			}
			public String Signature;
			public UInt32 SectionSize;
			public UInt16 CodeBegin;
			public UInt16 CodeEnd;
			public CMAPMappingMethod MappingMethod;
			public UInt16 Reserved;
			public UInt32 NextCMAPOffset;

			//Direct
			public UInt16 IndexOffset;

			//Table
			public UInt16[] IndexTable;

			//Scan
			public UInt16 NrScanEntries;
			public Dictionary<UInt16, UInt16> ScanEntries;

			public UInt16 GetIndexFromCode(UInt16 Code)
			{
				if (Code < CodeBegin || Code > CodeEnd) return 0xFFFF;
				switch (MappingMethod)
				{
					case CMAPMappingMethod.Direct:
						return (UInt16)(Code - CodeBegin + IndexOffset);
					case CMAPMappingMethod.Table:
						return IndexTable[Code - CodeBegin];
					case CMAPMappingMethod.Scan:
						if (!ScanEntries.ContainsKey(Code)) return 0xFFFF;
						return ScanEntries[Code];
				}
				return 0xFFFF;
			}
		}

		private CharWidthInfo GetCharWidthInfoByIndex(UInt16 Index)
		{
			foreach (var v in CharWidths)
			{
				if (Index < v.StartIndex || Index > v.EndIndex) continue;
				return v.CharWidths[Index - v.StartIndex];
			}
			return null;
		}

		private UInt16 GetIndexFromCode(UInt16 Code)
		{
			foreach (var v in CharMaps)
			{
				UInt16 result = v.GetIndexFromCode(Code);
				if (result != 0xFFFF) return result;
			}
			return 0xFFFF;
		}

		public BitmapFont GetBitmapFont()
		{
			BitmapFont f = new BitmapFont();
			f.LineHeight = FontInfo.LineFeed;
			Bitmap[] Chars = new Bitmap[TextureGlyph.SheetNrLines * TextureGlyph.SheetNrRows * TextureGlyph.NrSheets];

			float realcellwidth = TextureGlyph.CellWidth + 1;
			float realcellheight = TextureGlyph.CellHeight + 1;

			int j = 0;
			for (int sheet = 0; sheet < TextureGlyph.NrSheets; sheet++)
			{
				Bitmap SheetBM = TextureGlyph.GetSheet(sheet);
				BitmapData bd = SheetBM.LockBits(new Rectangle(0, 0, SheetBM.Width, SheetBM.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				for (int y = 0; y < TextureGlyph.SheetNrLines; y++)
				{
					for (int x = 0; x < TextureGlyph.SheetNrRows; x++)
					{
						Bitmap b = new Bitmap(TextureGlyph.CellWidth, TextureGlyph.CellHeight);
						BitmapData bd2 = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
						for (int y2 = 0; y2 < TextureGlyph.CellHeight; y2++)
						{
							for (int x2 = 0; x2 < TextureGlyph.CellWidth; x2++)
							{
								Marshal.WriteInt32(bd2.Scan0, y2 * bd2.Stride + x2 * 4, Marshal.ReadInt32(bd.Scan0, (int)(y * realcellheight + y2 + 1) * bd.Stride + (int)(x * realcellwidth + x2 + 1) * 4));
							}
						}
						b.UnlockBits(bd2);
						Chars[j++] = b;
					}
				}
				SheetBM.UnlockBits(bd);
			}

			foreach (var v in CharMaps)
			{
				for (int i = v.CodeBegin; i < v.CodeEnd + 1; i++)
				{
					ushort idx = v.GetIndexFromCode((ushort)i);
					if (idx == 0xFFFF) continue;
					var info = GetCharWidthInfoByIndex(idx);
					f.Characters.Add((char)i, new BitmapFont.Character(Chars[idx], info.Left, info.GlyphWidth, info.CharWidth));
				}
			}
			return f;
		}

		public class CharWidthInfo
		{
			public CharWidthInfo(EndianBinaryReader er)
			{
				Left = er.ReadSByte();
				GlyphWidth = er.ReadByte();
				CharWidth = er.ReadByte();
			}
			public SByte Left;
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
