using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;

namespace _3DS.NintendoWare.SND
{
	public class CSAR : FileFormat<CSAR.CSARIdentifier>
	{
		public CSAR(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new CSARHeader(er);
				//TODO!
			}
			finally
			{
				er.Close();
			}
		}

		public CSARHeader Header;
		public class CSARHeader
		{
			public CSARHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "CSAR") throw new SignatureNotCorrectException(Signature, "CSAR", er.BaseStream.Position - 4);
				Endianness = er.ReadUInt16();
				HeaderSize = er.ReadUInt16();
				Version = er.ReadUInt32();
				Unknown = er.ReadUInt32();//bigger than the filesize
				NrSections = er.ReadUInt32();
				Sections = new SectionInfo[NrSections];
				for (int i = 0; i < NrSections; i++) Sections[i] = new SectionInfo(er);
			}

			public String Signature;
			public UInt16 Endianness;
			public UInt16 HeaderSize;
			public UInt32 Version;
			public UInt32 Unknown;
			public UInt32 NrSections;
			public SectionInfo[] Sections;
			public class SectionInfo
			{
				public SectionInfo(uint Id) { this.Id = Id; }
				public SectionInfo(EndianBinaryReader er)
				{
					Id = er.ReadUInt32();
					Offset = er.ReadUInt32();
					Size = er.ReadUInt32();
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write(Id);
					er.Write(Offset);
					er.Write(Size);
				}
				public UInt32 Id;//0x4000 = INFO, 0x4001 = SEEK, 0x4002 = DATA
				public UInt32 Offset;
				public UInt32 Size;
			}
		}

		public STRG Strings;
		public class STRG
		{
			public STRG(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "STRG") throw new SignatureNotCorrectException(Signature, "STRG", er.BaseStream.Position - 4);
				long basepos = er.BaseStream.Position;
				StringTableSignature = er.ReadUInt32();
				StringTableOffset = er.ReadUInt32();
				PatriciaTreeSignature = er.ReadUInt32();
				PatriciaTreeOffset = er.ReadUInt32();

				er.BaseStream.Position = basepos + StringTableOffset;
				StringTable = new STRGStringTable(er);

				er.BaseStream.Position = basepos + PatriciaTreeOffset;
				//TODO!
			}
			public String Signature;
			public UInt32 SectionSize;
			public UInt32 StringTableSignature;//0x2400
			public UInt32 StringTableOffset;//Relative to start of section + 8
			public UInt32 PatriciaTreeSignature;//0x2401
			public UInt32 PatriciaTreeOffset;//Relative to start of section + 8

			public STRGStringTable StringTable;
			public class STRGStringTable
			{
				public STRGStringTable(EndianBinaryReader er)
				{
					long basepos = er.BaseStream.Position;
					NrEntries = er.ReadUInt32();
					Entries = new STRGStringTableEntry[NrEntries];
					for (int i = 0; i < NrEntries; i++) Entries[i] = new STRGStringTableEntry(er);
					long curpos = er.BaseStream.Position;
					Strings = new string[NrEntries];
					for (int i = 0; i < NrEntries; i++)
					{
						er.BaseStream.Position = basepos + Entries[i].StringOffset;
						Strings[i] = er.ReadStringNT(Encoding.ASCII);
					}
					er.BaseStream.Position = curpos;
				}
				public UInt32 NrEntries;
				public STRGStringTableEntry[] Entries;
				public class STRGStringTableEntry
				{
					public STRGStringTableEntry(EndianBinaryReader er)
					{
						NTStringSignature = er.ReadUInt32();
						StringOffset = er.ReadUInt32();
					}
					public UInt32 NTStringSignature;//0x1F01
					public UInt32 StringOffset;//Relative to start of this block
					public UInt32 StringLength;
				}
				public String[] Strings;
			}

		}

		public class CSARIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Archives;
			}

			public override string GetFileDescription()
			{
				return "CTR Sound Archive (CSAR)";
			}

			public override string GetFileFilter()
			{
				return "CTR Sound Archive (*.bcsar)|*.bcsar";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 0x10 && File.Data[0] == 'C' && File.Data[1] == 'S' && File.Data[2] == 'A' && File.Data[3] == 'R') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
