using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;

namespace NDS.NitroSystem.SND
{
	public class SDAT : FileFormat<SDAT.SDATIdentifier>, IViewable
	{
		public SDAT(byte[] Data)
		{
			EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new SDATHeader(er);
				if (Header.SYMBOffset != 0 && Header.SYMBLength != 0)
				{
					er.BaseStream.Position = Header.SYMBOffset;
					SymbolBlock = new SYMB(er);
				}
				er.BaseStream.Position = Header.INFOOffset;
				InfoBlock = new INFO(er);
				er.BaseStream.Position = Header.FATOffset;
				FileAllocationTable = new FAT(er);
				er.BaseStream.Position = Header.FILEOffset;
				File = new FILE(er);
			}
			finally
			{
				er.Close();
			}
		}

		public System.Windows.Forms.Form GetDialog()
		{
			return new System.Windows.Forms.Form();
		}

		public SDATHeader Header;
		public class SDATHeader
		{
			public SDATHeader(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
				/*Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "SDAT") throw new SignatureNotCorrectException(Signature, "SDAT", er.BaseStream.Position - 4);
				Endianness = er.ReadUInt16();
				Version = er.ReadUInt16();
				FileSize = er.ReadUInt32();
				HeaderSize = er.ReadUInt16();
				NrBlocks = er.ReadUInt16();
				SYMBOffset = er.ReadUInt32();
				SYMBLength = er.ReadUInt32();
				INFOOffset = er.ReadUInt32();
				INFOLength = er.ReadUInt32();
				FATOffset = er.ReadUInt32();
				FATLength = er.ReadUInt32();
				FILEOffset = er.ReadUInt32();
				FILELength = er.ReadUInt32();
				Padding = er.ReadBytes(16);*/
			}
			[BinaryStringSignature("SDAT")]
			[BinaryFixedSize(4)]
			public String Signature;
			[BinaryBOM(0xFFFE)]
			public UInt16 Endianness;
			public UInt16 Version;
			public UInt32 FileSize;
			public UInt16 HeaderSize;
			public UInt16 NrBlocks;
			public UInt32 SYMBOffset;
			public UInt32 SYMBLength;
			public UInt32 INFOOffset;
			public UInt32 INFOLength;
			public UInt32 FATOffset;
			public UInt32 FATLength;
			public UInt32 FILEOffset;
			public UInt32 FILELength;
			[BinaryFixedSize(16)]
			public byte[] Padding;//16
		}

		public SYMB SymbolBlock;
		public class SYMB
		{
			public SYMB(EndianBinaryReader er)
			{
				long baseoffset = er.BaseStream.Position;
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "SYMB") throw new SignatureNotCorrectException(Signature, "SYMB", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				RecordOffsets = er.ReadUInt32s(8);
				Padding = er.ReadBytes(24);
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = baseoffset + RecordOffsets[0];
				SEQRecord = new SymbolRecord(er);
				er.BaseStream.Position = baseoffset + RecordOffsets[1];
				SEQARCRecord = new ArchiveSymbolRecord(er);
				er.BaseStream.Position = baseoffset + RecordOffsets[2];
				BANKRecord = new SymbolRecord(er);
				er.BaseStream.Position = baseoffset + RecordOffsets[3];
				WAVEARCRecord = new SymbolRecord(er);
				er.BaseStream.Position = baseoffset + RecordOffsets[4];
				PLAYERRecord = new SymbolRecord(er);
				er.BaseStream.Position = baseoffset + RecordOffsets[5];
				GROUPRecord = new SymbolRecord(er);
				er.BaseStream.Position = baseoffset + RecordOffsets[6];
				PLAYER2Record = new SymbolRecord(er);
				er.BaseStream.Position = baseoffset + RecordOffsets[7];
				STRMRecord = new SymbolRecord(er);
				er.BaseStream.Position = curpos;
				SEQRecord.ReadNames(er, baseoffset);
				SEQARCRecord.ReadNames(er, baseoffset);
				BANKRecord.ReadNames(er, baseoffset);
				WAVEARCRecord.ReadNames(er, baseoffset);
				PLAYERRecord.ReadNames(er, baseoffset);
				GROUPRecord.ReadNames(er, baseoffset);
				PLAYER2Record.ReadNames(er, baseoffset);
				STRMRecord.ReadNames(er, baseoffset);
				er.BaseStream.Position = baseoffset + SectionSize;
			}
			public String Signature;
			public UInt32 SectionSize;
			public UInt32[] RecordOffsets;
			public Byte[] Padding;
			public SymbolRecord SEQRecord;
			public ArchiveSymbolRecord SEQARCRecord;
			public SymbolRecord BANKRecord;
			public SymbolRecord WAVEARCRecord;
			public SymbolRecord PLAYERRecord;
			public SymbolRecord GROUPRecord;
			public SymbolRecord PLAYER2Record;
			public SymbolRecord STRMRecord;
			public class SymbolRecord
			{
				public SymbolRecord(EndianBinaryReader er)
				{
					NrEntries = er.ReadUInt32();
					EntryOffsets = er.ReadUInt32s((int)NrEntries);
				}
				public UInt32 NrEntries;
				public UInt32[] EntryOffsets;

				public String[] Entries;

				public void ReadNames(EndianBinaryReader er, long BaseOffset)
				{
					long curpos = er.BaseStream.Position;
					Entries = new string[NrEntries];
					for (int i = 0; i < NrEntries; i++)
					{
						if (EntryOffsets[i] != 0)
						{
							er.BaseStream.Position = BaseOffset + EntryOffsets[i];
							Entries[i] = er.ReadStringNT(Encoding.ASCII);
						}
						else Entries[i] = "Entry " + i;
					}
					er.BaseStream.Position = curpos;
				}
			}
			public class ArchiveSymbolRecord
			{
				public ArchiveSymbolRecord(EndianBinaryReader er)
				{
					NrEntries = er.ReadUInt32();
					Entries = new ArchiveSymbolRecordEntry[NrEntries];
					for (int i = 0; i < NrEntries; i++)
					{
						Entries[i] = new ArchiveSymbolRecordEntry(er);
					}
				}
				public UInt32 NrEntries;
				public ArchiveSymbolRecordEntry[] Entries;
				public class ArchiveSymbolRecordEntry
				{
					public ArchiveSymbolRecordEntry(EndianBinaryReader er)
					{
						ArchiveNameOffset = er.ReadUInt32();
						ArchiveSubRecordOffset = er.ReadUInt32();
					}
					public UInt32 ArchiveNameOffset;
					public UInt32 ArchiveSubRecordOffset;

					public String ArchiveName;
					public SymbolRecord ArchiveSubRecord;
				}

				public void ReadNames(EndianBinaryReader er, long BaseOffset)
				{
					long curpos = er.BaseStream.Position;
					for (int i = 0; i < NrEntries; i++)
					{
						if (Entries[i].ArchiveNameOffset != 0)
						{
							er.BaseStream.Position = BaseOffset + Entries[i].ArchiveNameOffset;
							Entries[i].ArchiveName = er.ReadStringNT(Encoding.ASCII);
						}
						else Entries[i].ArchiveName = "Entry_" + i;
						er.BaseStream.Position = BaseOffset + Entries[i].ArchiveSubRecordOffset;
						Entries[i].ArchiveSubRecord = new SymbolRecord(er);
						Entries[i].ArchiveSubRecord.ReadNames(er, BaseOffset);
					}
					er.BaseStream.Position = curpos;
				}
			}
		}

		public INFO InfoBlock;
		public class INFO
		{
			public INFO(EndianBinaryReader er)
			{
				long baseoffset = er.BaseStream.Position;
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "INFO") throw new SignatureNotCorrectException(Signature, "INFO", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				RecordOffsets = er.ReadUInt32s(8);
				Padding = er.ReadBytes(24);
				//TODO
				er.BaseStream.Position = baseoffset + SectionSize;
			}
			public String Signature;
			public UInt32 SectionSize;
			public UInt32[] RecordOffsets;
			public Byte[] Padding;
		}

		public FAT FileAllocationTable;
		public class FAT
		{
			public FAT(EndianBinaryReader er)
			{
				long baseoffset = er.BaseStream.Position;
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "FAT ") throw new SignatureNotCorrectException(Signature, "FAT ", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				NrEntries = er.ReadUInt32();
				Entries = new FATEntry[NrEntries];
				for (int i = 0; i < NrEntries; i++)
				{
					Entries[i] = new FATEntry(er);
				}
				er.BaseStream.Position = baseoffset + SectionSize;
			}
			public String Signature;
			public UInt32 SectionSize;
			public UInt32 NrEntries;
			public FATEntry[] Entries;
			public class FATEntry
			{
				public FATEntry(EndianBinaryReader er)
				{
					Offset = er.ReadUInt32();
					Length = er.ReadUInt32();
					Padding = er.ReadBytes(8);
				}
				public UInt32 Offset;
				public UInt32 Length;
				public Byte[] Padding;//8
			}
		}

		public FILE File;
		public class FILE
		{
			public FILE(EndianBinaryReader er)
			{
				long baseoffset = er.BaseStream.Position;
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "FILE") throw new SignatureNotCorrectException(Signature, "FILE", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				NrFiles = er.ReadUInt32();
				while ((er.BaseStream.Position % 32) != 0) er.ReadByte();
				FileData = er.ReadBytes((int)(baseoffset + SectionSize - er.BaseStream.Position));
				er.BaseStream.Position = baseoffset + SectionSize;
			}
			public String Signature;
			public UInt32 SectionSize;
			public UInt32 NrFiles;
			public Byte[] FileData;
		}

		public class SDATIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Sound;
			}

			public override string GetFileDescription()
			{
				return "Nitro Sound Data (SDAT)";
			}

			public override string GetFileFilter()
			{
				return "Nitro Sound Data (*.sdat)|*.sdat";
			}

			public override Bitmap GetIcon()
			{
				return Resource.disc_music;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'D' && File.Data[2] == 'A' && File.Data[3] == 'T') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
