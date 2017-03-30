using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NDS.Nitro;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;
using NDS.UI;
using LibEveryFileExplorer.IO;

namespace NDS
{
	public class UtilityBin : FileFormat<UtilityBin.UtilityBinIdentifier>, IViewable, IWriteable
	{
		public UtilityBin(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			Header = new UtilityBinHeader(er);

			er.BaseStream.Position = Header.FileNameTableOffset;
			FileNameTable = new UtilityBinFNT(er);

			er.BaseStream.Position = Header.FileAllocationTableOffset;
			FileAllocationTable = new FileAllocationEntry[Header.FileAllocationTableSize / 8];
			for (int i = 0; i < Header.FileAllocationTableSize / 8; i++)
			{
				FileAllocationTable[i] = new FileAllocationEntry(er);
			}

			FileData = new byte[Header.FileAllocationTableSize / 8][];
			for (int i = 0; i < Header.FileAllocationTableSize / 8; i++)
			{
				er.BaseStream.Position = FileAllocationTable[i].fileTop;
				FileData[i] = er.ReadBytes((int)FileAllocationTable[i].fileSize);
			}
			er.Close();
		}

		public Form GetDialog()
		{
			return new UtilityBinViewer(this);
		}

		public string GetSaveDefaultFileFilter()
		{
			return "NDS Wifi Archive (utility.bin)|utility.bin";
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			er.Write((uint)0);
			er.Write((uint)0);

			er.Write((uint)0);
			er.Write((uint)0);

			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = 0;
			er.Write((uint)curpos);
			er.BaseStream.Position = curpos;

			FileNameTable.Write(er);
			while ((er.BaseStream.Position % 4) != 0) er.Write((byte)0xFF);

			long curpos2 = er.BaseStream.Position;
			er.BaseStream.Position = 4;
			er.Write((uint)(curpos2 - curpos));
			er.BaseStream.Position = curpos2;

			while((er.BaseStream.Position % 0x20) != 0) er.Write((byte)0);

			curpos = er.BaseStream.Position;
			er.BaseStream.Position = 8;
			er.Write((uint)curpos);
			er.BaseStream.Position = curpos;

			er.BaseStream.Position += FileAllocationTable.Length * 8;

			curpos2 = er.BaseStream.Position;
			er.BaseStream.Position = 12;
			er.Write((uint)(curpos2 - curpos));
			er.BaseStream.Position = curpos2;

			while ((er.BaseStream.Position % 0x20) != 0) er.Write((byte)0);

			for (int i = 0; i < FileData.Length; i++)
			{
				while ((er.BaseStream.Position % 4) != 0) er.Write((byte)0xFF);
				FileAllocationTable[i].fileTop = (uint)er.BaseStream.Position;
				FileAllocationTable[i].fileBottom = (uint)er.BaseStream.Position + (uint)FileData[i].Length;
				er.Write(FileData[i], 0, FileData[i].Length);
			}

			while ((er.BaseStream.Position % 4) != 0) er.Write((byte)0xFF);

			er.BaseStream.Position = curpos;
			foreach (var v in FileAllocationTable) v.Write(er);

			byte[] result = m.ToArray();
			er.Close();
			return result;
		}

		public UtilityBinHeader Header;
		public class UtilityBinHeader
		{
			public UtilityBinHeader(EndianBinaryReader er)
			{
				FileNameTableOffset = er.ReadUInt32();
				FileNameTableSize = er.ReadUInt32();
				FileAllocationTableOffset = er.ReadUInt32();
				FileAllocationTableSize = er.ReadUInt32();
			}
			public UInt32 FileNameTableOffset;
			public UInt32 FileNameTableSize;
			public UInt32 FileAllocationTableOffset;
			public UInt32 FileAllocationTableSize;
		}
		public UtilityBinFNT FileNameTable;
		public class UtilityBinFNT
		{
			public UtilityBinFNT(EndianBinaryReader er)
			{
				DirectoryTable = new List<DirectoryTableEntry>();
				DirectoryTable.Add(new DirectoryTableEntry(er));
				for (int i = 0; i < DirectoryTable[0].dirParentID - 1; i++)
				{
					DirectoryTable.Add(new DirectoryTableEntry(er));
				}
				EntryNameTable = new List<EntryNameTableEntry>();
				int dirend = 0;
				while (dirend < DirectoryTable[0].dirParentID)
				{
					byte entryNameLength = er.ReadByte();
					er.BaseStream.Position--;
					if (entryNameLength == 0)
					{
						EntryNameTable.Add(new EntryNameTableEndOfDirectoryEntry(er));
						dirend++;
					}
					else if (entryNameLength < 0x80) EntryNameTable.Add(new EntryNameTableFileEntry(er));
					else EntryNameTable.Add(new EntryNameTableDirectoryEntry(er));
				}
			}
			public void Write(EndianBinaryWriter er)
			{
				foreach (DirectoryTableEntry e in DirectoryTable) e.Write(er);
				foreach (EntryNameTableEntry e in EntryNameTable) e.Write(er);
			}
			public List<DirectoryTableEntry> DirectoryTable;
			public List<EntryNameTableEntry> EntryNameTable;
		}
		public FileAllocationEntry[] FileAllocationTable;
		public Byte[][] FileData;

		public void FromFileSystem(SFSDirectory Root)
		{
			int did = 0;
			int fid = 0;
			Root.UpdateIDs(ref did, ref fid);
			//FileNameTable.numFiles = (ushort)Root.TotalNrSubFiles;
			//List<byte> Data = new List<byte>();
			uint nrfiles = Root.TotalNrSubFiles;
			FileAllocationTable = new FileAllocationEntry[nrfiles];
			//FATB.allocationTable.Clear();
			for (ushort i = 0; i < nrfiles; i++)
			{
				var f = Root.GetFileByID(i);
				FileAllocationTable[i] = new FileAllocationEntry(0, 0);
				//FATB.allocationTable.Add(new FileAllocationEntry((uint)Data.Count, (uint)f.Data.Length));
				//Data.AddRange(f.Data);
				//while ((Data.Count % 4) != 0) Data.Add(0xFF);
			}
			//FIMG.fileImage = Data.ToArray();
			FileNameTable.DirectoryTable.Clear();
			NitroFSUtil.GenerateDirectoryTable(FileNameTable.DirectoryTable, Root);
			uint offset2 = FileNameTable.DirectoryTable[0].dirEntryStart;
			ushort fileId = 0;
			FileNameTable.EntryNameTable.Clear();
			NitroFSUtil.GenerateEntryNameTable(FileNameTable.DirectoryTable, FileNameTable.EntryNameTable, Root, ref offset2, ref fileId);
		}

		public SFSDirectory ToFileSystem()
		{
			bool treereconstruct = false;//Some programs do not write the Directory Table well, so sometimes I need to reconstruct the tree based on the fnt, which is bad!
			List<SFSDirectory> dirs = new List<SFSDirectory>();
			dirs.Add(new SFSDirectory("/", true));
			dirs[0].DirectoryID = 0xF000;

			uint nrdirs = FileNameTable.DirectoryTable[0].dirParentID;
			for (int i = 1; i < nrdirs; i++)
			{
				dirs.Add(new SFSDirectory((ushort)(0xF000 + i)));
			}
			for (int i = 1; i < nrdirs; i++)
			{
				if (FileNameTable.DirectoryTable[i].dirParentID - 0xF000 == i)
				{
					treereconstruct = true;
					foreach (var v in dirs)
					{
						v.Parent = null;
					}
					break;
				}
				dirs[i].Parent = dirs[FileNameTable.DirectoryTable[i].dirParentID - 0xF000];
			}
			if (!treereconstruct)
			{
				for (int i = 0; i < nrdirs; i++)
				{
					for (int j = 0; j < nrdirs; j++)
					{
						if (dirs[i] == dirs[j].Parent)
						{
							dirs[i].SubDirectories.Add(dirs[j]);
						}
					}
				}
			}
			uint offset = nrdirs * 8;
			ushort fileid = FileNameTable.DirectoryTable[0].dirEntryFileID;
			SFSDirectory curdir = null;
			foreach (EntryNameTableEntry e in FileNameTable.EntryNameTable)
			{
				for (int i = 0; i < nrdirs; i++)
				{
					if (offset == FileNameTable.DirectoryTable[i].dirEntryStart)
					{
						curdir = dirs[i];
						break;
					}
				}
				if (e is EntryNameTableEndOfDirectoryEntry)
				{
					curdir = null;
					offset++;
				}
				else if (e is EntryNameTableFileEntry)
				{
					curdir.Files.Add(new SFSFile(fileid++, ((EntryNameTableFileEntry)e).entryName, curdir));
					offset += 1u + e.entryNameLength;
				}
				else if (e is EntryNameTableDirectoryEntry)
				{
					if (treereconstruct)
					{
						dirs[((EntryNameTableDirectoryEntry)e).directoryID - 0xF000].Parent = curdir;
						curdir.SubDirectories.Add(dirs[((EntryNameTableDirectoryEntry)e).directoryID - 0xF000]);
					}
					dirs[((EntryNameTableDirectoryEntry)e).directoryID - 0xF000].DirectoryName = ((EntryNameTableDirectoryEntry)e).entryName;
					offset += 3u + (e.entryNameLength & 0x7Fu);
				}
			}
			for (int i = 0; i < FileAllocationTable.Length; i++)
			{
				//byte[] data = new byte[FATB.allocationTable[i].fileSize];
				//Array.Copy(FIMG.fileImage, FATB.allocationTable[i].fileTop, data, 0, data.Length);
				dirs[0].GetFileByID((ushort)i).Data = FileData[i];// data;
			}
			return dirs[0];
		}

		        public class UtilityBinIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				//Tempoarly
				return "NDS Wifi Archive";
			}

			public override string GetFileDescription()
			{
				return "NDS Wifi Archive (utility.bin)";
			}

			public override string GetFileFilter()
			{
				return "NDS Wifi Archive (utility.bin)|utility.bin";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if(File.Name.ToLower() == "utility.bin") return FormatMatch.Extension;
				return FormatMatch.No;
			}
			
		}
	}
}
