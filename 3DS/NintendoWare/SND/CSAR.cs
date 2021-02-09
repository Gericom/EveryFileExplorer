using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;

namespace _3DS.NintendoWare.SND
{
	public class CSAR : FileFormat<CSAR.CSARIdentifier>, IViewable
	{
		public CSAR(byte[] Data)
		{
			EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new CSARHeader(er);
				foreach (var v in Header.Sections)
				{
					er.BaseStream.Position = v.Offset;
					switch (v.Id)
					{
						case 0x2000: Strings = new STRG(er); break;
						case 0x2001: Infos = new INFO(er); break;
					}
				}
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

		public CSARHeader Header;
		public class CSARHeader
		{
			public CSARHeader(EndianBinaryReaderEx er)
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
				public SectionInfo(EndianBinaryReaderEx er)
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
			public STRG(EndianBinaryReaderEx er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "STRG") throw new SignatureNotCorrectException(Signature, "STRG", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				long basepos = er.BaseStream.Position;
				StringTableSignature = er.ReadUInt32();
				StringTableOffset = er.ReadUInt32();
				PatriciaTreeSignature = er.ReadUInt32();
				PatriciaTreeOffset = er.ReadUInt32();

				er.BaseStream.Position = basepos + StringTableOffset;
				StringTable = new STRGStringTable(er);

				er.BaseStream.Position = basepos + PatriciaTreeOffset;
				PatriciaTree = new STRGPatriciaTree(er);
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
				public STRGStringTable(EndianBinaryReaderEx er)
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
					public STRGStringTableEntry(EndianBinaryReaderEx er)
					{
						NTStringSignature = er.ReadUInt32();
						StringOffset = er.ReadUInt32();
						StringLength = er.ReadUInt32();
					}
					public UInt32 NTStringSignature;//0x1F01
					public UInt32 StringOffset;//Relative to start of this block
					public UInt32 StringLength;
				}
				public String[] Strings;
			}
			public STRGPatriciaTree PatriciaTree;
			public class STRGPatriciaTree
			{
				public STRGPatriciaTree(EndianBinaryReaderEx er)
				{
					RootNodeIndex = er.ReadUInt32();
					NrNodes = er.ReadUInt32();
					Nodes = new PatriciaTreeNode[NrNodes];
					for (int i = 0; i < NrNodes; i++) Nodes[i] = new PatriciaTreeNode(er);
				}
				public UInt32 RootNodeIndex;
				public UInt32 NrNodes;
				public PatriciaTreeNode[] Nodes;
				public class PatriciaTreeNode
				{
					public PatriciaTreeNode(EndianBinaryReaderEx er)
					{
						er.ReadObject(this);
						/*IsLeaf = er.ReadUInt16() == 1;
						Bit = er.ReadUInt16();
						Left = er.ReadUInt32();
						Right = er.ReadUInt32();
						StringID = er.ReadUInt32();
						NodeID = er.ReadUInt32();*/
					}
					[BinaryBooleanSize(BooleanSize.U16)]
					public Boolean IsLeaf;//2
					public UInt16 Bit;
					public UInt32 Left;
					public UInt32 Right;
					public UInt32 StringID;
					public UInt32 NodeID;
				}
			}
		}
		public INFO Infos;
		public class INFO
		{
			public INFO(EndianBinaryReaderEx er)
			{
				/*Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "INFO") throw new SignatureNotCorrectException(Signature, "INFO", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				long basepos = er.BaseStream.Position;
				SoundInfoSignature = er.ReadUInt32();
				SoundInfoOffset = er.ReadUInt32();
				SoundGroupInfoSignature = er.ReadUInt32();
				SoundGroupInfoOffset = er.ReadUInt32();
				BankInfoSignature = er.ReadUInt32();
				BankInfoOffset = er.ReadUInt32();
				WaveArchiveInfoSignature = er.ReadUInt32();
				WaveArchiveInfoOffset = er.ReadUInt32();
				GroupInfoSignature = er.ReadUInt32();
				GroupInfoOffset = er.ReadUInt32();
				PlayerInfoSignature = er.ReadUInt32();
				PlayerInfoOffset = er.ReadUInt32();
				FileInfoSignature = er.ReadUInt32();
				FileInfoOffset = er.ReadUInt32();
				SoundArchivePlayerInfoSignature = er.ReadUInt32();
				SoundArchivePlayerInfoOffset = er.ReadUInt32();*/
				long basepos = er.BaseStream.Position + 8;
				er.ReadObject(this);

				er.BaseStream.Position = basepos + SoundInfoOffset;
				SoundInfo = new INFOInfoBlock<INFOSoundInfoEntry>(er);
			}
			[BinaryStringSignature("INFO")]
			[BinaryFixedSize(4)]
			public String Signature;
			public UInt32 SectionSize;
			public UInt32 SoundInfoSignature;
			public UInt32 SoundInfoOffset;
			public UInt32 SoundGroupInfoSignature;
			public UInt32 SoundGroupInfoOffset;
			public UInt32 BankInfoSignature;
			public UInt32 BankInfoOffset;
			public UInt32 WaveArchiveInfoSignature;
			public UInt32 WaveArchiveInfoOffset;
			public UInt32 GroupInfoSignature;
			public UInt32 GroupInfoOffset;
			public UInt32 PlayerInfoSignature;
			public UInt32 PlayerInfoOffset;
			public UInt32 FileInfoSignature;
			public UInt32 FileInfoOffset;
			public UInt32 SoundArchivePlayerInfoSignature;
			public UInt32 SoundArchivePlayerInfoOffset;

			public class INFOInfoBlock<T> where T : INFOInfoBlockEntry, new()
			{
				public INFOInfoBlock(EndianBinaryReaderEx er)
				{
					long basepos = er.BaseStream.Position;
					NrEntries = er.ReadUInt32();
					ReferenceTableEntries = new ReferenceTableEntry[NrEntries];
					for (int i = 0; i < NrEntries; i++) ReferenceTableEntries[i] = new ReferenceTableEntry(er);
					Entries = new T[NrEntries];
					for (int i = 0; i < NrEntries; i++)
					{
						Entries[i] = new T();
						Entries[i].Read(er);
					}
				}
				public UInt32 NrEntries;
				public ReferenceTableEntry[] ReferenceTableEntries;
				public class ReferenceTableEntry
				{
					public ReferenceTableEntry(EndianBinaryReaderEx er)
					{
						Signature = er.ReadUInt32();
						Offset = er.ReadUInt32();
					}
					public UInt32 Signature;
					public UInt32 Offset;
				}
				public T[] Entries;
			}

			public abstract class INFOInfoBlockEntry
			{
				public abstract void Read(EndianBinaryReaderEx er);
			}
			[BinaryIgnore]
			public INFOInfoBlock<INFOSoundInfoEntry> SoundInfo;
			public class INFOSoundInfoEntry : INFOInfoBlockEntry
			{
				public override void Read(EndianBinaryReaderEx er)
				{
					FileID = er.ReadUInt32();
					PlayerID = er.ReadUInt32();
					Volume = er.ReadByte();
					Padding = er.ReadBytes(3);

					SpecificInfoSignature = er.ReadUInt32();
					SpecificInfoOffset = er.ReadUInt32();
				}
				public UInt32 FileID;
				public UInt32 PlayerID;
				public Byte Volume;
				public Byte[] Padding;//3

				public UInt32 SpecificInfoSignature;
				public UInt32 SpecificInfoOffset;

				public INFOSoundInfoParameterArray Parameters;
				public class INFOSoundInfoParameterArray
				{
					public UInt32 Flags;
				}
			}
		}

		public class CSARIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Sound;
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
				if (File.Data.Length > 50 && File.Data[0] == 'C' && File.Data[1] == 'S' && File.Data[2] == 'A' && File.Data[3] == 'R' && File.Data[0x40] == 'S' && File.Data[0x41] == 'T' && File.Data[0x42] == 'R' && File.Data[0x43] == 'G') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
