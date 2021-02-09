using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using NDS.Nitro;
using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.FND
{
	public class NARC : FileFormat<NARC.NARCIdentifier>, IEmptyCreatable, IViewable, IWriteable
	{
		public NARC()
		{
			Header = new ArchiveHeader();
			FATB = new FileAllocationTableBlock();
			FNTB = new FilenameTableBlock();
			FIMG = new FileImageBlock();
			FromFileSystem(new SFSDirectory("/", true) { DirectoryID = 0xF000 });
		}

		public NARC(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new ArchiveHeader(er);
				FATB = new FileAllocationTableBlock(er);
				FNTB = new FilenameTableBlock(er);
				FIMG = new FileImageBlock(er);
			}
			catch (SignatureNotCorrectException e)
			{
				MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new UI.NARCViewer(this);
		}

		public String GetSaveDefaultFileFilter()
		{
			return "Nitro Archive (*.narc)|*.narc";
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			Header.Write(er);
			FATB.Write(er);
			FNTB.Write(er);
			FIMG.Write(er);
			er.BaseStream.Position = 8;
			er.Write((UInt32)(er.BaseStream.Length));
			byte[] result = m.ToArray();
			er.Close();
			return result;
		}

		public ArchiveHeader Header;
		public class ArchiveHeader
		{
			public ArchiveHeader()
			{
				signature = "NARC";
				byteOrder = 0xFFFE;
				version = 0x0100;
				fileSize = 0;
				headerSize = 16;
				dataBlocks = 3;
			}
			public ArchiveHeader(EndianBinaryReader er)
			{
				signature = er.ReadString(Encoding.ASCII, 4);
				if (signature != "NARC") throw new SignatureNotCorrectException(signature, "NARC", er.BaseStream.Position - 4);
				byteOrder = er.ReadUInt16();
				version = er.ReadUInt16();
				fileSize = er.ReadUInt32();
				headerSize = er.ReadUInt16();
				dataBlocks = er.ReadUInt16();
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(signature, Encoding.ASCII, false);
				er.Write(byteOrder);
				er.Write(version);
				er.Write(fileSize);
				er.Write(headerSize);
				er.Write(dataBlocks);
			}
			public String signature;
			public UInt16 byteOrder;
			public UInt16 version;
			public UInt32 fileSize;
			public UInt16 headerSize;
			public UInt16 dataBlocks;
		}
		public FileAllocationTableBlock FATB;
		public class FileAllocationTableBlock
		{
			public FileAllocationTableBlock()
			{
				kind = "BTAF";
				size = 0;
				numFiles = 0;
				reserved = 0;
				allocationTable = new List<FileAllocationEntry>();
			}
			public FileAllocationTableBlock(EndianBinaryReader er)
			{
				kind = er.ReadString(Encoding.ASCII, 4);
				if (kind != "BTAF") throw new SignatureNotCorrectException(kind, "BTAF", er.BaseStream.Position);
				size = er.ReadUInt32();
				numFiles = er.ReadUInt16();
				reserved = er.ReadUInt16();
				allocationTable = new List<FileAllocationEntry>();
				for (int i = 0; i < numFiles; i++)
				{
					allocationTable.Add(new FileAllocationEntry(er));
				}
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(kind, Encoding.ASCII, false);
				er.Write((UInt32)(12 + (allocationTable.Count * 8)));
				er.Write((UInt16)allocationTable.Count);
				er.Write(reserved);
				foreach (FileAllocationEntry e in allocationTable) e.Write(er);
			}
			public String kind;
			public UInt32 size;
			public UInt16 numFiles;
			public UInt16 reserved;
			public List<FileAllocationEntry> allocationTable;
		}
		public FilenameTableBlock FNTB;
		public class FilenameTableBlock
		{
			public FilenameTableBlock()
			{
				kind = "BTNF";
				size = 0;
				directoryTable = new List<DirectoryTableEntry>();
				entryNameTable = new List<EntryNameTableEntry>();
			}
			public FilenameTableBlock(EndianBinaryReader er)
			{
				long start = er.BaseStream.Position;
				kind = er.ReadString(Encoding.ASCII, 4);
				if (kind != "BTNF") throw new SignatureNotCorrectException(kind, "BTNF", er.BaseStream.Position);
				size = er.ReadUInt32();
				directoryTable = new List<DirectoryTableEntry>();
				directoryTable.Add(new DirectoryTableEntry(er));
				for (int i = 0; i < directoryTable[0].dirParentID - 1; i++)
				{
					directoryTable.Add(new DirectoryTableEntry(er));
				}
				entryNameTable = new List<EntryNameTableEntry>();
				int dirend = 0;
				while (dirend < directoryTable[0].dirParentID)
				{
					byte entryNameLength = er.ReadByte();
					er.BaseStream.Position--;
					if (entryNameLength == 0)
					{
						entryNameTable.Add(new EntryNameTableEndOfDirectoryEntry(er));
						dirend++;
					}
					else if (entryNameLength < 0x80) entryNameTable.Add(new EntryNameTableFileEntry(er));
					else entryNameTable.Add(new EntryNameTableDirectoryEntry(er));
				}
				//while ((er.BaseStream.Position % 4) != 0) er.ReadByte();
				er.BaseStream.Position = start + size;
			}
			public void Write(EndianBinaryWriter er)
			{
				long basepos = er.BaseStream.Position;
				er.Write(kind, Encoding.ASCII, false);
				er.Write((UInt32)0);
				foreach (DirectoryTableEntry e in directoryTable) e.Write(er);
				foreach (EntryNameTableEntry e in entryNameTable) e.Write(er);
				while ((er.BaseStream.Position % 4) != 0) er.Write((byte)0xFF);
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = basepos + 4;
				er.Write((UInt32)(curpos - basepos));
				er.BaseStream.Position = curpos;
			}
			public String kind;
			public UInt32 size;
			public List<DirectoryTableEntry> directoryTable;
			public List<EntryNameTableEntry> entryNameTable;
		}
		public FileImageBlock FIMG;
		public class FileImageBlock
		{
			public FileImageBlock()
			{
				kind = "GMIF";
				size = 0;
			}
			public FileImageBlock(EndianBinaryReader er)
			{
				kind = er.ReadString(Encoding.ASCII, 4);
				if (kind != "GMIF") throw new SignatureNotCorrectException(kind, "GMIF", er.BaseStream.Position);
				size = er.ReadUInt32();
				//Some programs seem to write the size wrong, which causes index out of range exceptions
				fileImage = er.ReadBytes((int)(er.BaseStream.Length - er.BaseStream.Position));//er.ReadBytes((int)size - 8);
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(kind, Encoding.ASCII, false);
				er.Write((UInt32)(fileImage.Length + 8));
				er.Write(fileImage, 0, fileImage.Length);
			}
			public String kind;
			public UInt32 size;
			public Byte[] fileImage;
		}

		public void FromFileSystem(SFSDirectory Root)
		{
			int did = 0;
			int fid = 0;
			Root.UpdateIDs(ref did, ref fid);
			FATB.numFiles = (ushort)Root.TotalNrSubFiles;
			List<byte> Data = new List<byte>();
			FATB.allocationTable.Clear();
			for (ushort i = 0; i < FATB.numFiles; i++)
			{
				var f = Root.GetFileByID(i);
				FATB.allocationTable.Add(new FileAllocationEntry((uint)Data.Count, (uint)f.Data.Length));
				Data.AddRange(f.Data);
				while ((Data.Count % 4) != 0) Data.Add(0xFF);
			}
			FIMG.fileImage = Data.ToArray();
			FNTB.directoryTable.Clear();
			NitroFSUtil.GenerateDirectoryTable(FNTB.directoryTable, Root);
			uint offset2 = FNTB.directoryTable[0].dirEntryStart;
			ushort fileId = 0;
			FNTB.entryNameTable.Clear();
			NitroFSUtil.GenerateEntryNameTable(FNTB.directoryTable, FNTB.entryNameTable, Root, ref offset2, ref fileId);
		}

		public SFSDirectory ToFileSystem()
		{
			bool treereconstruct = false;//Some programs do not write the Directory Table well, so sometimes I need to reconstruct the tree based on the fnt, which is bad!
			List<SFSDirectory> dirs = new List<SFSDirectory>();
			dirs.Add(new SFSDirectory("/", true));
			dirs[0].DirectoryID = 0xF000;

			uint nrdirs = FNTB.directoryTable[0].dirParentID;
			for (int i = 1; i < nrdirs; i++)
			{
				dirs.Add(new SFSDirectory((ushort)(0xF000 + i)));
			}
			for (int i = 1; i < nrdirs; i++)
			{
				if (FNTB.directoryTable[i].dirParentID - 0xF000 == i)
				{
					treereconstruct = true;
					foreach (var v in dirs)
					{
						v.Parent = null;
					}
					break;
				}
				dirs[i].Parent = dirs[FNTB.directoryTable[i].dirParentID - 0xF000];
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
			ushort fileid = FNTB.directoryTable[0].dirEntryFileID;
			SFSDirectory curdir = null;
			foreach (EntryNameTableEntry e in FNTB.entryNameTable)
			{
				for (int i = 0; i < nrdirs; i++)
				{
					if (offset == FNTB.directoryTable[i].dirEntryStart)
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
			for (int i = 0; i < FATB.numFiles; i++)
			{
				byte[] data = new byte[FATB.allocationTable[i].fileSize];
				Array.Copy(FIMG.fileImage, FATB.allocationTable[i].fileTop, data, 0, data.Length);
				dirs[0].GetFileByID((ushort)i).Data = data;
			}
			return dirs[0];
		}

            public class NARCIdentifier : FileFormatIdentifier
            {
            public override string GetCategory()
            {
            return Category_Archives;
            }

			public override string GetFileDescription()
			{
				return "Nitro Archive (NARC)";
			}

			public override string GetFileFilter()
			{
				return "Nitro Archive (*.narc)|*.narc|Compress Nitro Archive (*.carc)|*.carc";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'N' && File.Data[1] == 'A' && File.Data[2] == 'R' && File.Data[3] == 'C') return FormatMatch.Content;
				return FormatMatch.No;
			}
			
		}
	}
}
