using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using _3DS.UI;
using LibEveryFileExplorer.IO;

namespace _3DS.NintendoWare.LYT1
{
	public class DARC : FileFormat<DARC.darcIdentifier>, IViewable
	{
		public DARC(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new darcHeader(er);
				er.BaseStream.Position = Header.FileTableOffset;
				FileTableEntry root = new FileTableEntry(er);
				Entries = new FileTableEntry[root.DataLength];
				Entries[0] = root;
				for (int i = 1; i < root.DataLength; i++) Entries[i] = new FileTableEntry(er);
				FileNameTable = new Dictionary<uint,string>();
				uint offs = 0;
				for (int i = 0; i < root.DataLength; i++)
				{
					String s = er.ReadStringNT(Encoding.Unicode);
					FileNameTable.Add(offs, s);
					offs += (uint)s.Length * 2 + 2;
				}
				er.BaseStream.Position = Header.FileDataOffset;
				this.Data = er.ReadBytes((int)(Header.FileSize - Header.FileDataOffset));
			}
			finally
			{
				er.Close();
			}
		}


		public System.Windows.Forms.Form GetDialog()
		{
			return new DARCViewer(this);
		}

		public darcHeader Header;
		public class darcHeader
		{
			public darcHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "darc") throw new SignatureNotCorrectException(Signature, "darc", er.BaseStream.Position - 4);
				Endianness = er.ReadUInt16();
				HeaderSize = er.ReadUInt16();
				Version = er.ReadUInt32();
				FileSize = er.ReadUInt32();
				FileTableOffset = er.ReadUInt32();
				FileTableLength = er.ReadUInt32();
				FileDataOffset = er.ReadUInt32();
			}
			public String Signature;
			public UInt16 Endianness;
			public UInt16 HeaderSize;
			public UInt32 Version;
			public UInt32 FileSize;
			public UInt32 FileTableOffset;
			public UInt32 FileTableLength;
			public UInt32 FileDataOffset;
		}

		public FileTableEntry[] Entries;
		public class FileTableEntry
		{
			public FileTableEntry(EndianBinaryReader er)
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
			SFSDirectory[] dirs = new SFSDirectory[Entries.Length];
			dirs[1] = new SFSDirectory("/", true);
			var curdir = dirs[1];
			for (int i = 2; i < Entries.Length; i++)
			{
				if (Entries[i].IsFolder)
				{
					var folder = new SFSDirectory(FileNameTable[Entries[i].NameOffset], false);
					dirs[i] = folder;
					folder.Parent = dirs[Entries[i].DataOffset];
					dirs[Entries[i].DataOffset].SubDirectories.Add(folder);
					curdir = folder;
				}
				else
				{
					var file = new SFSFile(-1, FileNameTable[Entries[i].NameOffset], curdir);
					byte[] data = new byte[Entries[i].DataLength];
					Array.Copy(Data, Entries[i].DataOffset - Header.FileDataOffset, data, 0, Entries[i].DataLength);
					file.Data = data;
					curdir.Files.Add(file);
				}
			}
			return dirs[1];
		}

            public class darcIdentifier : FileFormatIdentifier
            {
            public override string GetCategory()
            {
            return "Darc Archives";
            }

			public override string GetFileDescription()
			{
				return "Data Archive (darc)";
			}

			public override string GetFileFilter()
			{
				return "Data Archive (*.darc, *.arc, *.bcma)|*.darc;*.arc;*.bcma";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'd' && File.Data[1] == 'a' && File.Data[2] == 'r' && File.Data[3] == 'c') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
