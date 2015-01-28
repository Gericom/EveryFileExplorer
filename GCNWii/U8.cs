using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.IO.Serialization;
using LibEveryFileExplorer.IO;
using System.IO;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using GCNWii.UI;

namespace GCNWii
{
	public class U8 : FileFormat<U8.U8Identifier>, IViewable
	{
		public U8(byte[] Data)
		{
			EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.BigEndian);
			try
			{
				Header = new U8Header(er);
				er.BaseStream.Position = Header.FileSystemTableOffset;
				FileSystemTableEntry root = new FileSystemTableEntry(er);
				FileSystemTable = new FileSystemTableEntry[root.DataLength];
				FileSystemTable[0] = root;
				for (int i = 1; i < root.DataLength; i++) FileSystemTable[i] = new FileSystemTableEntry(er);
				FileNameTable = new Dictionary<uint,string>();
				uint offs = 0;
				for (int i = 0; i < root.DataLength; i++)
				{
					String s = er.ReadStringNT(Encoding.ASCII);
					FileNameTable.Add(offs, s);
					offs += (uint)s.Length + 1;
				}
				er.BaseStream.Position = Header.FileDataOffset;
				this.Data = er.ReadBytes((int)(er.BaseStream.Length - Header.FileDataOffset));
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new U8Viewer(this);
		}

		public U8Header Header;
		public class U8Header
		{
			public U8Header(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}
			[BinaryByteArraySignature(0x55, 0xAA, 0x38, 0x2D)]
			[BinaryFixedSize(4)]
			public byte[] Signature;
			public UInt32 FileSystemTableOffset;
			public UInt32 FileSystemTableLength;
			public UInt32 FileDataOffset;
			[BinaryFixedSize(16)]
			public Byte[] Padding;
		}

		public FileSystemTableEntry[] FileSystemTable;
		public class FileSystemTableEntry
		{
			public FileSystemTableEntry(EndianBinaryReader er)
			{
				NameOffset = er.ReadUInt32();
				IsFolder = (NameOffset >> 24) == 1;
				NameOffset &= 0xFFFFFF;
				DataOffset = er.ReadUInt32();
				DataLength = er.ReadUInt32();
			}
			public UInt32 NameOffset;
			public Boolean IsFolder;
			public UInt32 DataOffset;//Parent Entry Index if folder
			public UInt32 DataLength;//Nr Files if folder
		}
		public Dictionary<UInt32, String> FileNameTable;

		public byte[] Data;

		public SFSDirectory ToFileSystem()
		{
			SFSDirectory[] dirs = new SFSDirectory[FileSystemTable.Length];
			dirs[1] = new SFSDirectory("/", true);
			var curdir = dirs[1];
			for (int i = 2; i < FileSystemTable.Length; i++)
			{
				if (FileSystemTable[i].IsFolder)
				{
					var folder = new SFSDirectory(FileNameTable[FileSystemTable[i].NameOffset], false);
					dirs[i] = folder;
					folder.Parent = dirs[FileSystemTable[i].DataOffset];
					dirs[FileSystemTable[i].DataOffset].SubDirectories.Add(folder);
					curdir = folder;
				}
				else
				{
					var file = new SFSFile(-1, FileNameTable[FileSystemTable[i].NameOffset], curdir);
					byte[] data = new byte[FileSystemTable[i].DataLength];
					Array.Copy(Data, FileSystemTable[i].DataOffset - Header.FileDataOffset, data, 0, FileSystemTable[i].DataLength);
					file.Data = data;
					curdir.Files.Add(file);
				}
			}
			return dirs[1];
		}

		public class U8Identifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Archives;
			}

			public override string GetFileDescription()
			{
				return "U8 Archive";
			}

			public override string GetFileFilter()
			{
				return "U8 Archive (*.arc, *.szs)|*.arc;*.szs";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 0x55 && File.Data[1] == 0xAA && File.Data[2] == 0x38 && File.Data[3] == 0x2D) return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
