using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using NDS.UI;

namespace NDS
{
	public class NDS : FileFormat<NDS.NDSIdentifier>, IViewable
	{
		public NDS(byte[] data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(data), Endianness.LittleEndian);
			header = new ROM_Header(er);

			er.BaseStream.Position = header.main_rom_offset;
			main_rom = er.ReadBytes((int)header.main_size);

			er.BaseStream.Position = header.sub_rom_offset;
			sub_rom = er.ReadBytes((int)header.sub_size);

			er.BaseStream.Position = header.fnt_offset;
			fnt = new ROM_FNT(er);

			er.BaseStream.Position = header.main_ovt_offset;
			main_ovt = new ROM_OVT[header.main_ovt_size / 32];
			for (int i = 0; i < header.main_ovt_size / 32; i++) main_ovt[i] = new ROM_OVT(er);

			er.BaseStream.Position = header.sub_ovt_offset;
			sub_ovt = new ROM_OVT[header.sub_ovt_size / 32];
			for (int i = 0; i < header.sub_ovt_size / 32; i++) sub_ovt[i] = new ROM_OVT(er);

			er.BaseStream.Position = header.fat_offset;
			fat = new FileAllocationEntry[header.fat_size / 8];
			for (int i = 0; i < header.fat_size / 8; i++) fat[i] = new FileAllocationEntry(er);

			er.BaseStream.Position = header.banner_offset;
			banner = new BannerFile(er);

			FileData = new byte[header.fat_size / 8][];
			for (int i = 0; i < header.fat_size / 8; i++)
			{
				er.BaseStream.Position = fat[i].fileTop;
				FileData[i] = er.ReadBytes((int)fat[i].fileSize);
			}

			er.Close();
		}

		public Form GetDialog()
		{
			return new NDSViewer(this);
		}

		public ROM_Header header;
		public class ROM_Header
		{
			public ROM_Header(EndianBinaryReader er)
			{
				game_name = er.ReadString(Encoding.ASCII, 12).Replace("\0", "");
				game_code = er.ReadString(Encoding.ASCII, 4).Replace("\0", "");
				maker_code = er.ReadString(Encoding.ASCII, 2).Replace("\0", "");
				product_id = er.ReadByte();
				device_type = er.ReadByte();
				device_size = er.ReadByte();
				reserved_A = er.ReadBytes(9);
				game_version = er.ReadByte();
				property = er.ReadByte();

				main_rom_offset = er.ReadUInt32();
				main_entry_address = er.ReadUInt32();
				main_ram_address = er.ReadUInt32();
				main_size = er.ReadUInt32();
				sub_rom_offset = er.ReadUInt32();
				sub_entry_address = er.ReadUInt32();
				sub_ram_address = er.ReadUInt32();
				sub_size = er.ReadUInt32();

				fnt_offset = er.ReadUInt32();
				fnt_size = er.ReadUInt32();

				fat_offset = er.ReadUInt32();
				fat_size = er.ReadUInt32();

				main_ovt_offset = er.ReadUInt32();
				main_ovt_size = er.ReadUInt32();

				sub_ovt_offset = er.ReadUInt32();
				sub_ovt_size = er.ReadUInt32();

				rom_param_A = er.ReadBytes(8);
				banner_offset = er.ReadUInt32();
				secure_crc = er.ReadUInt16();
				rom_param_B = er.ReadBytes(2);

				main_autoload_done = er.ReadUInt32();
				sub_autoload_done = er.ReadUInt32();

				rom_param_C = er.ReadBytes(8);
				rom_size = er.ReadUInt32();
				header_size = er.ReadUInt32();
				reserved_B = er.ReadBytes(0x38);

				logo_data = er.ReadBytes(0x9C);
				logo_crc = er.ReadUInt16();
				header_crc = er.ReadUInt16();
			}
			public String game_name;//12
			public String game_code;//4
			public String maker_code;//2
			public Byte product_id;
			public Byte device_type;
			public Byte device_size;
			public byte[] reserved_A;//9
			public Byte game_version;
			public Byte property;

			public UInt32 main_rom_offset;
			public UInt32 main_entry_address;
			public UInt32 main_ram_address;
			public UInt32 main_size;
			public UInt32 sub_rom_offset;
			public UInt32 sub_entry_address;
			public UInt32 sub_ram_address;
			public UInt32 sub_size;

			public UInt32 fnt_offset;
			public UInt32 fnt_size;

			public UInt32 fat_offset;
			public UInt32 fat_size;

			public UInt32 main_ovt_offset;
			public UInt32 main_ovt_size;

			public UInt32 sub_ovt_offset;
			public UInt32 sub_ovt_size;

			public byte[] rom_param_A;//8
			public UInt32 banner_offset;
			public UInt16 secure_crc;
			public byte[] rom_param_B;//2

			public UInt32 main_autoload_done;
			public UInt32 sub_autoload_done;

			public byte[] rom_param_C;//8
			public UInt32 rom_size;
			public UInt32 header_size;
			public byte[] reserved_B;//0x38

			public byte[] logo_data;//0x9C
			public UInt16 logo_crc;
			public UInt16 header_crc;
		}
		public Byte[] main_rom;
		public Byte[] sub_rom;
		public ROM_FNT fnt;
		public class ROM_FNT
		{
			public ROM_FNT(EndianBinaryReader er)
			{
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
			}
			public List<DirectoryTableEntry> directoryTable;
			public List<EntryNameTableEntry> entryNameTable;
		}
		public ROM_OVT[] main_ovt;
		public ROM_OVT[] sub_ovt;
		public class ROM_OVT
		{
			[Flags]
			public enum OVT_Flag : byte
			{
				Compressed = 1,
				Authentication_Code = 2
			}
			public ROM_OVT(EndianBinaryReader er)
			{
				id = er.ReadUInt32();
				ram_address = er.ReadUInt32();
				ram_size = er.ReadUInt32();
				bss_size = er.ReadUInt32();
				sinit_init = er.ReadUInt32();
				sinit_init_end = er.ReadUInt32();
				file_id = er.ReadUInt32();
				UInt32 tmp = er.ReadUInt32();
				compressed = tmp & 0xFFFFFF;
				flag = (OVT_Flag)(tmp >> 24);
			}
			public UInt32 id;
			public UInt32 ram_address;
			public UInt32 ram_size;
			public UInt32 bss_size;
			public UInt32 sinit_init;
			public UInt32 sinit_init_end;
			public UInt32 file_id;

			public UInt32 compressed;//:24;
			public OVT_Flag flag;// :8;
		}
		public FileAllocationEntry[] fat;
		public BannerFile banner;
		public class BannerFile
		{
			public BannerFile(EndianBinaryReader er)
			{
				h = new BannerHeader(er);
				v1 = new BannerFileV1(er);
			}
			BannerHeader h;
			public class BannerHeader
			{
				public BannerHeader(EndianBinaryReader er)
				{
					version = er.ReadByte();
					reserved_A = er.ReadByte();
					crc16_v1 = er.ReadUInt16();
					reserved_B = er.ReadBytes(28);
				}
				public Byte version;
				public Byte reserved_A;
				public UInt16 crc16_v1;
				public Byte[] reserved_B;//28
			}
			BannerFileV1 v1;
			public class BannerFileV1
			{
				public BannerFileV1(EndianBinaryReader er)
				{
					image = er.ReadBytes(32 * 32 / 2);
					pltt = er.ReadBytes(16 * 2);
					gameName = new string[6];
					gameName[0] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
					gameName[1] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
					gameName[2] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
					gameName[3] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
					gameName[4] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
					gameName[5] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
				}
				public Byte[] image;//32*32/2
				public Byte[] pltt;//16*2
				public String[] gameName;//6, 128 chars (UTF16-LE)
			}
		}
		public Byte[][] FileData;

		public SFSDirectory ToFileSystem()
		{
			bool treereconstruct = false;//Some programs do not write the Directory Table well, so sometimes I need to reconstruct the tree based on the fnt, which is bad!
			List<SFSDirectory> dirs = new List<SFSDirectory>();
			dirs.Add(new SFSDirectory("/", true));
			dirs[0].DirectoryID = 0xF000;

			uint nrdirs = fnt.directoryTable[0].dirParentID;
			for (int i = 1; i < nrdirs; i++)
			{
				dirs.Add(new SFSDirectory((ushort)(0xF000 + i)));
			}
			for (int i = 1; i < nrdirs; i++)
			{
				if (fnt.directoryTable[i].dirParentID - 0xF000 == i)
				{
					treereconstruct = true;
					foreach (var v in dirs)
					{
						v.Parent = null;
					}
					break;
				}
				dirs[i].Parent = dirs[fnt.directoryTable[i].dirParentID - 0xF000];
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
			ushort fileid = fnt.directoryTable[0].dirEntryFileID;
			SFSDirectory curdir = null;
			foreach (EntryNameTableEntry e in fnt.entryNameTable)
			{
				for (int i = 0; i < nrdirs; i++)
				{
					if (offset == fnt.directoryTable[i].dirEntryStart)
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
			for (int i = (int)(header.main_ovt_size / 32 + header.sub_ovt_size / 32); i < header.fat_size / 8; i++)
			{
				//byte[] data = new byte[fat[i].fileSize];
				//Array.Copy(FileData FIMG.fileImage, fat[i].fileTop, data, 0, data.Length);
				dirs[0].GetFileByID((ushort)i).Data = FileData[i];//data;
			}
			return dirs[0];
		}

		public class NDSIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Roms;
			}

			public override string GetFileDescription()
			{
				return "Nintendo DS Rom (NDS)";
			}

			public override string GetFileFilter()
			{
				return "Nintendo DS Rom (*.nds, *.srl)|*.nds;*.srl";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Name.ToLower().EndsWith(".nds") || File.Name.ToLower().EndsWith(".srl")) return FormatMatch.Extension;
				return FormatMatch.No;
			}

		}
	}
}
