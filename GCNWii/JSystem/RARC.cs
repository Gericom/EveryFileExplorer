using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using GCNWii.UI;
using LibEveryFileExplorer.IO;

namespace GCNWii.JSystem
{
	public class RARC : FileFormat<RARC.RARCIdentifier>, IViewable
	{
		public RARC(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.BigEndian);
			try
			{
				Header = new RARCHeader(er);
				ArchiveInfo = new InfoBlock(er);
				er.BaseStream.Position = ArchiveInfo.FolderTableOffset + Header.HeaderSize;
				FolderTable = new FolderTableEntry[ArchiveInfo.NrFolderTableEntries];
				for (int i = 0; i < ArchiveInfo.NrFolderTableEntries; i++) FolderTable[i] = new FolderTableEntry(er);

				er.BaseStream.Position = ArchiveInfo.FileTableOffset + Header.HeaderSize;
				FileTable = new FileTableEntry[ArchiveInfo.NrFileTableEntries];
				for (int i = 0; i < ArchiveInfo.NrFileTableEntries; i++) FileTable[i] = new FileTableEntry(er);

				for (int i = 0; i < ArchiveInfo.NrFolderTableEntries; i++)
				{
					er.BaseStream.Position = ArchiveInfo.StringTableOffset + Header.HeaderSize + FolderTable[i].NameOffset;
					FolderTable[i].Name = er.ReadStringNT(Encoding.ASCII);
				}

				for (int i = 0; i < ArchiveInfo.NrFileTableEntries; i++)
				{
					er.BaseStream.Position = ArchiveInfo.StringTableOffset + Header.HeaderSize + FileTable[i].NameOffset;
					FileTable[i].Name = er.ReadStringNT(Encoding.ASCII);
				}

				er.BaseStream.Position = Header.HeaderSize + Header.FileDataOffset;
				this.Data = er.ReadBytes((int)Header.FileDataLength1);
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new RARCViewer(this);
		}

		public RARCHeader Header;
		public class RARCHeader
		{
			public RARCHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "RARC") throw new SignatureNotCorrectException(Signature, "RARC", er.BaseStream.Position - 4);
				FileSize = er.ReadUInt32();
				HeaderSize = er.ReadUInt32();
				FileDataOffset = er.ReadUInt32();
				FileDataLength1 = er.ReadUInt32();
				FileDataLength2 = er.ReadUInt32();
				Unknown3 = er.ReadUInt32();
				Unknown4 = er.ReadUInt32();
			}
			public String Signature;
			public UInt32 FileSize;
			public UInt32 HeaderSize;
			public UInt32 FileDataOffset;//relative to the end of the header
			public UInt32 FileDataLength1;
			public UInt32 FileDataLength2;
			public UInt32 Unknown3;//Padding?
			public UInt32 Unknown4;//Padding?
		}

		public InfoBlock ArchiveInfo;
		public class InfoBlock
		{
			public InfoBlock(EndianBinaryReader er)
			{
				NrFolderTableEntries = er.ReadUInt32();
				FolderTableOffset = er.ReadUInt32();
				NrFileTableEntries = er.ReadUInt32();
				FileTableOffset = er.ReadUInt32();
				StringTableLength = er.ReadUInt32();
				StringTableOffset = er.ReadUInt32();
				NrFileTableEntries2 = er.ReadUInt16();
				Unknown1 = er.ReadUInt16();
				Unknown2 = er.ReadUInt32();
			}
			public UInt32 NrFolderTableEntries;
			public UInt32 FolderTableOffset;//relative to the start of this block
			public UInt32 NrFileTableEntries;
			public UInt32 FileTableOffset;//relative to the start of this block
			public UInt32 StringTableLength;
			public UInt32 StringTableOffset;//relative to the start of this block
			public UInt16 NrFileTableEntries2;
			public UInt16 Unknown1;
			public UInt32 Unknown2;
		}

		public FolderTableEntry[] FolderTable;
		public class FolderTableEntry
		{
			public FolderTableEntry(EndianBinaryReader er)
			{
				Identifier = er.ReadString(Encoding.ASCII, 4);
				NameOffset = er.ReadUInt32();
				Unknown = er.ReadUInt16();
				NrFileEntries = er.ReadUInt16();
				FirstFileEntryIndex = er.ReadUInt32();
			}
			public String Identifier;//4
			public UInt32 NameOffset;//relative to the start of the string table
			public UInt16 Unknown;//Seems to be a hash of the name
			public UInt16 NrFileEntries;
			public UInt32 FirstFileEntryIndex;

			public String Name;

			public override string ToString()
			{
				if (Name != null) return Name;
				return Identifier;
			}
		}

		public FileTableEntry[] FileTable;
		public class FileTableEntry
		{
			public FileTableEntry(EndianBinaryReader er)
			{
				FileID = er.ReadUInt16();
				Unknown1 = er.ReadUInt16();
				Unknown2 = er.ReadUInt16();
				NameOffset = er.ReadUInt16();
				DataOffset = er.ReadUInt32();
				DataSize = er.ReadUInt32();
				Unknown3 = er.ReadUInt32();
			}
			public UInt16 FileID;
			public UInt16 Unknown1;//Seems to be a hash of the name
			public UInt16 Unknown2;
			public UInt16 NameOffset;//relative to the start of the string table
			public UInt32 DataOffset;//Folder Index if subfolder
			public UInt32 DataSize;
			public UInt32 Unknown3;

			public String Name;

			public override string ToString()
			{
				return Name;
			}
		}

		public byte[] Data;

		public SFSDirectory ToFileSystem()
		{
			SFSDirectory[] dirs = new SFSDirectory[ArchiveInfo.NrFolderTableEntries];
			for (int i = (int)ArchiveInfo.NrFolderTableEntries - 1; i >= 0; i--)
			{
				dirs[i] = new SFSDirectory(FolderTable[i].Name, (FolderTable[i].Identifier == "ROOT"));
				for (int j = 0; j < FolderTable[i].NrFileEntries; j++)
				{
					var v = FileTable[j + FolderTable[i].FirstFileEntryIndex];
					if (v.FileID != 0xFFFF)
					{
						var f = new SFSFile(v.FileID, v.Name, dirs[i]);
						f.Data = new byte[v.DataSize];
						Array.Copy(Data, v.DataOffset, f.Data, 0, v.DataSize);
						dirs[i].Files.Add(f);
					}
					else
					{
						if (v.Name == "." || v.Name == "..") continue;
						dirs[v.DataOffset].Parent = dirs[i];
						dirs[i].SubDirectories.Add(dirs[v.DataOffset]);
					}
				}
			}
			return dirs[0];
		}

		public class RARCIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Archives;
			}

			public override string GetFileDescription()
			{
				return "JSystem Resource Archive (RARC)";
			}

			public override string GetFileFilter()
			{
				return "JSystem Resource Archive (*.arc, *.rarc)|*.arc;*.rarc";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 0x40 && File.Data[0] == 'R' && File.Data[1] == 'A' && File.Data[2] == 'R' && File.Data[3] == 'C') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
