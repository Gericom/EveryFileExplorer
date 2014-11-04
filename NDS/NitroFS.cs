using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NDS
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
}
