using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.Files.SimpleFileSystem;

namespace NDS.Nitro
{
	public class FileAllocationEntry
	{
		public FileAllocationEntry(UInt32 Offset, UInt32 Size)
		{
			fileTop = Offset;
			fileBottom = Offset + Size;
		}
		public FileAllocationEntry(EndianBinaryReader er)
		{
			fileTop = er.ReadUInt32();
			fileBottom = er.ReadUInt32();
		}
		public void Write(EndianBinaryWriter er)
		{
			er.Write(fileTop);
			er.Write(fileBottom);
		}
		public UInt32 fileTop;
		public UInt32 fileBottom;

		public UInt32 fileSize
		{
			get { return fileBottom - fileTop; }
		}
	}
	public class DirectoryTableEntry
	{
		public DirectoryTableEntry() { }
		public DirectoryTableEntry(EndianBinaryReader er)
		{
			dirEntryStart = er.ReadUInt32();
			dirEntryFileID = er.ReadUInt16();
			dirParentID = er.ReadUInt16();
		}
		public void Write(EndianBinaryWriter er)
		{
			er.Write(dirEntryStart);
			er.Write(dirEntryFileID);
			er.Write(dirParentID);
		}
		public UInt32 dirEntryStart;
		public UInt16 dirEntryFileID;
		public UInt16 dirParentID;
	}
	public class EntryNameTableEntry
	{
		protected EntryNameTableEntry() { }
		public EntryNameTableEntry(EndianBinaryReader er)
		{
			entryNameLength = er.ReadByte();
		}
		public virtual void Write(EndianBinaryWriter er)
		{
			er.Write(entryNameLength);
		}
		public Byte entryNameLength;
	}
	public class EntryNameTableEndOfDirectoryEntry : EntryNameTableEntry
	{
		public EntryNameTableEndOfDirectoryEntry() { }
		public EntryNameTableEndOfDirectoryEntry(EndianBinaryReader er)
			: base(er) { }
		public override void Write(EndianBinaryWriter er)
		{
			base.Write(er);
		}
	}
	public class EntryNameTableFileEntry : EntryNameTableEntry
	{
		public EntryNameTableFileEntry(String Name)
		{
			entryNameLength = (byte)Name.Length;
			entryName = Name;
		}
		public EntryNameTableFileEntry(EndianBinaryReader er)
			: base(er)
		{
			entryName = er.ReadString(Encoding.ASCII, entryNameLength);
		}
		public override void Write(EndianBinaryWriter er)
		{
			base.Write(er);
			er.Write(entryName, Encoding.ASCII, false);
		}
		public String entryName;
	}
	public class EntryNameTableDirectoryEntry : EntryNameTableEntry
	{
		public EntryNameTableDirectoryEntry(String Name, UInt16 DirectoryID)
		{
			entryNameLength = (byte)(Name.Length | 0x80);
			entryName = Name;
			directoryID = DirectoryID;
		}
		public EntryNameTableDirectoryEntry(EndianBinaryReader er)
			: base(er)
		{
			entryName = er.ReadString(Encoding.ASCII, entryNameLength & 0x7F);
			directoryID = er.ReadUInt16();
		}
		public override void Write(EndianBinaryWriter er)
		{
			base.Write(er);
			er.Write(entryName, Encoding.ASCII, false);
			er.Write(directoryID);
		}
		public String entryName;
		public UInt16 directoryID;
	}

	public class NitroFSUtil
	{
		public static void GenerateDirectoryTable(List<DirectoryTableEntry> directoryTable, SFSDirectory dir)
		{
			DirectoryTableEntry cur = new DirectoryTableEntry();
			if (dir.IsRoot)
			{
				cur.dirParentID = (ushort)(dir.TotalNrSubDirectories + 1);
				cur.dirEntryStart = cur.dirParentID * 8u;
			}
			else cur.dirParentID = dir.Parent.DirectoryID;
			dir.DirectoryID = (ushort)(0xF000 + directoryTable.Count);
			directoryTable.Add(cur);
			foreach (SFSDirectory d in dir.SubDirectories)
			{
				GenerateDirectoryTable(directoryTable, d);
			}
		}

		public static void GenerateEntryNameTable(List<DirectoryTableEntry> directoryTable, List<EntryNameTableEntry> entryNameTable, SFSDirectory dir, ref uint Offset, ref ushort FileId)
		{
			directoryTable[dir.DirectoryID - 0xF000].dirEntryStart = Offset;
			directoryTable[dir.DirectoryID - 0xF000].dirEntryFileID = FileId;

			foreach (SFSDirectory d in dir.SubDirectories)
			{
				entryNameTable.Add(new EntryNameTableDirectoryEntry(d.DirectoryName, d.DirectoryID));
				Offset += (uint)d.DirectoryName.Length + 3u;
			}
			foreach (SFSFile f in dir.Files)
			{
				f.FileID = FileId;
				entryNameTable.Add(new EntryNameTableFileEntry(f.FileName));
				Offset += (uint)f.FileName.Length + 1u;
				FileId++;
			}
			entryNameTable.Add(new EntryNameTableEndOfDirectoryEntry());
			Offset++;

			foreach (SFSDirectory d in dir.SubDirectories)
			{
				GenerateEntryNameTable(directoryTable, entryNameTable, d, ref Offset, ref FileId);
			}
		}
	}
}
