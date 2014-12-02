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
using System.Xml.Serialization;
using System.Xml;

namespace NDS.Nitro
{
	public class NDS : FileFormat<NDS.NDSIdentifier>, IViewable, IWriteable
	{
		public NDS() { }
		public NDS(byte[] data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(data), Endianness.LittleEndian);
			Header = new RomHeader(er);

			er.BaseStream.Position = Header.MainRomOffset;
			MainRom = er.ReadBytes((int)Header.MainSize);
			if (er.ReadUInt32() == 0xDEC00621)//Nitro Footer!
			{
				er.BaseStream.Position -= 4;
				StaticFooter = new NitroFooter(er);
			}

			er.BaseStream.Position = Header.SubRomOffset;
			SubRom = er.ReadBytes((int)Header.SubSize);

			er.BaseStream.Position = Header.FntOffset;
			Fnt = new RomFNT(er);

			er.BaseStream.Position = Header.MainOvtOffset;
			MainOvt = new RomOVT[Header.MainOvtSize / 32];
			for (int i = 0; i < Header.MainOvtSize / 32; i++) MainOvt[i] = new RomOVT(er);

			er.BaseStream.Position = Header.SubOvtOffset;
			SubOvt = new RomOVT[Header.SubOvtSize / 32];
			for (int i = 0; i < Header.SubOvtSize / 32; i++) SubOvt[i] = new RomOVT(er);

			er.BaseStream.Position = Header.FatOffset;
			Fat = new FileAllocationEntry[Header.FatSize / 8];
			for (int i = 0; i < Header.FatSize / 8; i++) Fat[i] = new FileAllocationEntry(er);

			if (Header.BannerOffset != 0)
			{
				er.BaseStream.Position = Header.BannerOffset;
				Banner = new RomBanner(er);
			}

			FileData = new byte[Header.FatSize / 8][];
			for (int i = 0; i < Header.FatSize / 8; i++)
			{
				er.BaseStream.Position = Fat[i].fileTop;
				FileData[i] = er.ReadBytes((int)Fat[i].fileSize);
			}
			//RSA Signature
			if (Header.RomSize + 0x88 <= er.BaseStream.Length)
			{
				er.BaseStream.Position = Header.RomSize;
				byte[] RSASig = er.ReadBytes(0x88);
				for (int i = 0; i < RSASig.Length; i++)
				{
					//It could be padding, so check if there is something other than 0xFF or 0x00
					if (RSASig[i] != 0xFF || RSASig[i] != 0x00)
					{
						RSASignature = RSASig;
						break;
					}
				}
			}
			er.Close();
		}

		public Form GetDialog()
		{
			return new NDSViewer(this);
		}

		public string GetSaveDefaultFileFilter()
		{
			return "Nintendo DS Rom (*.nds)|*.nds";
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			//Header
			//skip the header, and write it afterwards
			er.BaseStream.Position = 16384;
			Header.HeaderSize = (uint)er.BaseStream.Position;
			//MainRom
			Header.MainRomOffset = (uint)er.BaseStream.Position;
			Header.MainSize = (uint)MainRom.Length;
			er.Write(MainRom, 0, MainRom.Length);
			//Static Footer
			if (StaticFooter != null) StaticFooter.Write(er);
			if (MainOvt.Length != 0)
			{
				while ((er.BaseStream.Position % 0x200) != 0) er.Write((byte)0);
				//Main Ovt
				Header.MainOvtOffset = (uint)er.BaseStream.Position;
				Header.MainOvtSize = (uint)MainOvt.Length * 0x20;
				foreach (var v in MainOvt) v.Write(er);
				foreach (var v in MainOvt)
				{
					while ((er.BaseStream.Position % 0x200) != 0) er.Write((byte)0);
					Fat[v.FileId].fileTop = (uint)er.BaseStream.Position;
					Fat[v.FileId].fileBottom = (uint)er.BaseStream.Position + (uint)FileData[v.FileId].Length;
					er.Write(FileData[v.FileId], 0, FileData[v.FileId].Length);
				}
			}
			else
			{
				Header.MainOvtOffset = 0;
				Header.MainOvtSize = 0;
			}
			while ((er.BaseStream.Position % 0x200) != 0) er.Write((byte)0xFF);
			//SubRom
			Header.SubRomOffset = (uint)er.BaseStream.Position;
			Header.SubSize = (uint)SubRom.Length;
			er.Write(SubRom, 0, SubRom.Length);
			//I assume this works the same as the main ovt?
			if (SubOvt.Length != 0)
			{
				while ((er.BaseStream.Position % 0x200) != 0) er.Write((byte)0);
				//Sub Ovt
				Header.SubOvtOffset = (uint)er.BaseStream.Position;
				Header.SubOvtSize = (uint)SubOvt.Length * 0x20;
				foreach (var v in SubOvt) v.Write(er);
				foreach (var v in SubOvt)
				{
					while ((er.BaseStream.Position % 0x200) != 0) er.Write((byte)0);
					Fat[v.FileId].fileTop = (uint)er.BaseStream.Position;
					Fat[v.FileId].fileBottom = (uint)er.BaseStream.Position + (uint)FileData[v.FileId].Length;
					er.Write(FileData[v.FileId], 0, FileData[v.FileId].Length);
				}

			}
			else
			{
				Header.SubOvtOffset = 0;
				Header.SubOvtSize = 0;
			}
			while ((er.BaseStream.Position % 0x200) != 0) er.Write((byte)0xFF);
			//FNT
			Header.FntOffset = (uint)er.BaseStream.Position;
			Fnt.Write(er);
			Header.FntSize = (uint)er.BaseStream.Position - Header.FntOffset;
			while ((er.BaseStream.Position % 0x200) != 0) er.Write((byte)0xFF);
			//FAT
			Header.FatOffset = (uint)er.BaseStream.Position;
			Header.FatSize = (uint)Fat.Length * 8;
			//Skip the fat, and write it after writing the data itself
			er.BaseStream.Position += Header.FatSize;
			//Banner
			if (Banner != null)
			{
				while ((er.BaseStream.Position % 0x200) != 0) er.Write((byte)0xFF);
				Header.BannerOffset = (uint)er.BaseStream.Position;
				Banner.Write(er);
			}
			else Header.BannerOffset = 0;
			//Files
			for (int i = (int)(Header.MainOvtSize / 32 + Header.SubOvtSize / 32); i < FileData.Length; i++)
			{
				while ((er.BaseStream.Position % 0x200) != 0) er.Write((byte)0xFF);
				Fat[i].fileTop = (uint)er.BaseStream.Position;
				Fat[i].fileBottom = (uint)er.BaseStream.Position + (uint)FileData[i].Length;
				er.Write(FileData[i], 0, FileData[i].Length);
			}
			while ((er.BaseStream.Position % 4/*0x200*/) != 0) er.Write((byte)0);
			long curpos = er.BaseStream.Position;
			Header.RomSize = (uint)er.BaseStream.Position;
			uint CapacitySize = Header.RomSize;
			CapacitySize |= CapacitySize >> 16;
			CapacitySize |= CapacitySize >> 8;
			CapacitySize |= CapacitySize >> 4;
			CapacitySize |= CapacitySize >> 2;
			CapacitySize |= CapacitySize >> 1;
			CapacitySize++;
			if (CapacitySize <= 0x20000) CapacitySize = 0x20000;
			int Capacity = -18;
			while (CapacitySize != 0) { CapacitySize >>= 1; Capacity++; }
			Header.DeviceSize = (byte)((Capacity < 0) ? 0 : Capacity);
			//RSA!
			if (RSASignature != null) er.Write(RSASignature, 0, 0x88);
			//Fat
			er.BaseStream.Position = Header.FatOffset;
			foreach (var v in Fat) v.Write(er);
			//Header
			er.BaseStream.Position = 0;
			Header.Write(er);
			byte[] result = m.ToArray();
			er.Close();
			return result;
		}

		public RomHeader Header;
		[Serializable]
		public class RomHeader
		{
			public RomHeader() { }
			public RomHeader(EndianBinaryReader er)
			{
				GameName = er.ReadString(Encoding.ASCII, 12).Replace("\0", "");
				GameCode = er.ReadString(Encoding.ASCII, 4).Replace("\0", "");
				MakerCode = er.ReadString(Encoding.ASCII, 2).Replace("\0", "");
				ProductId = er.ReadByte();
				DeviceType = er.ReadByte();
				DeviceSize = er.ReadByte();
				ReservedA = er.ReadBytes(9);
				GameVersion = er.ReadByte();
				Property = er.ReadByte();

				MainRomOffset = er.ReadUInt32();
				MainEntryAddress = er.ReadUInt32();
				MainRamAddress = er.ReadUInt32();
				MainSize = er.ReadUInt32();
				SubRomOffset = er.ReadUInt32();
				SubEntryAddress = er.ReadUInt32();
				SubRamAddress = er.ReadUInt32();
				SubSize = er.ReadUInt32();

				FntOffset = er.ReadUInt32();
				FntSize = er.ReadUInt32();

				FatOffset = er.ReadUInt32();
				FatSize = er.ReadUInt32();

				MainOvtOffset = er.ReadUInt32();
				MainOvtSize = er.ReadUInt32();

				SubOvtOffset = er.ReadUInt32();
				SubOvtSize = er.ReadUInt32();

				RomParamA = er.ReadBytes(8);
				BannerOffset = er.ReadUInt32();
				SecureCRC = er.ReadUInt16();
				RomParamB = er.ReadBytes(2);

				MainAutoloadDone = er.ReadUInt32();
				SubAutoloadDone = er.ReadUInt32();

				RomParamC = er.ReadBytes(8);
				RomSize = er.ReadUInt32();
				HeaderSize = er.ReadUInt32();
				ReservedB = er.ReadBytes(0x38);

				LogoData = er.ReadBytes(0x9C);
				LogoCRC = er.ReadUInt16();
				HeaderCRC = er.ReadUInt16();
			}
			public void Write(EndianBinaryWriter er)
			{
				MemoryStream m = new MemoryStream();
				EndianBinaryWriter ew = new EndianBinaryWriter(m, Endianness.LittleEndian);
				ew.Write(GameName.PadRight(12, '\0'), Encoding.ASCII, false);
				ew.Write(GameCode.PadRight(4, '\0'), Encoding.ASCII, false);
				ew.Write(MakerCode.PadRight(2, '\0'), Encoding.ASCII, false);
				ew.Write(ProductId);
				ew.Write(DeviceType);
				ew.Write(DeviceSize);
				ew.Write(ReservedA, 0, 9);
				ew.Write(GameVersion);
				ew.Write(Property);

				ew.Write(MainRomOffset);
				ew.Write(MainEntryAddress);
				ew.Write(MainRamAddress);
				ew.Write(MainSize);
				ew.Write(SubRomOffset);
				ew.Write(SubEntryAddress);
				ew.Write(SubRamAddress);
				ew.Write(SubSize);

				ew.Write(FntOffset);
				ew.Write(FntSize);

				ew.Write(FatOffset);
				ew.Write(FatSize);

				ew.Write(MainOvtOffset);
				ew.Write(MainOvtSize);

				ew.Write(SubOvtOffset);
				ew.Write(SubOvtSize);

				ew.Write(RomParamA, 0, 8);
				ew.Write(BannerOffset);
				ew.Write(SecureCRC);
				ew.Write(RomParamB, 0, 2);

				ew.Write(MainAutoloadDone);
				ew.Write(SubAutoloadDone);

				ew.Write(RomParamC, 0, 8);
				ew.Write(RomSize);
				ew.Write(HeaderSize);
				ew.Write(ReservedB, 0, 0x38);

				ew.Write(LogoData, 0, 0x9C);
				LogoCRC = CRC16.GetCRC16(LogoData);
				ew.Write(LogoCRC);

				byte[] header = m.ToArray();
				ew.Close();

				HeaderCRC = CRC16.GetCRC16(header);

				er.Write(header, 0, header.Length);
				er.Write(HeaderCRC);
			}
			public String GameName;//12
			public String GameCode;//4
			public String MakerCode;//2
			public Byte ProductId;
			public Byte DeviceType;
			public Byte DeviceSize;
			public byte[] ReservedA;//9
			public Byte GameVersion;
			public Byte Property;

			[XmlIgnore]
			public UInt32 MainRomOffset;
			public UInt32 MainEntryAddress;
			public UInt32 MainRamAddress;
			[XmlIgnore]
			public UInt32 MainSize;
			[XmlIgnore]
			public UInt32 SubRomOffset;
			public UInt32 SubEntryAddress;
			public UInt32 SubRamAddress;
			[XmlIgnore]
			public UInt32 SubSize;

			[XmlIgnore]
			public UInt32 FntOffset;
			[XmlIgnore]
			public UInt32 FntSize;

			[XmlIgnore]
			public UInt32 FatOffset;
			[XmlIgnore]
			public UInt32 FatSize;

			[XmlIgnore]
			public UInt32 MainOvtOffset;
			[XmlIgnore]
			public UInt32 MainOvtSize;

			[XmlIgnore]
			public UInt32 SubOvtOffset;
			[XmlIgnore]
			public UInt32 SubOvtSize;

			public byte[] RomParamA;//8
			[XmlIgnore]
			public UInt32 BannerOffset;
			public UInt16 SecureCRC;
			public byte[] RomParamB;//2

			public UInt32 MainAutoloadDone;
			public UInt32 SubAutoloadDone;

			public byte[] RomParamC;//8
			[XmlIgnore]
			public UInt32 RomSize;
			[XmlIgnore]
			public UInt32 HeaderSize;
			public byte[] ReservedB;//0x38

			public byte[] LogoData;//0x9C
			[XmlIgnore]
			public UInt16 LogoCRC;
			[XmlIgnore]
			public UInt16 HeaderCRC;
		}
		public Byte[] MainRom;
		public NitroFooter StaticFooter;
		[Serializable]
		public class NitroFooter
		{
			public NitroFooter() { }
			public NitroFooter(EndianBinaryReader er)
			{
				NitroCode = er.ReadUInt32();
				_start_ModuleParamsOffset = er.ReadUInt32();
				Unknown = er.ReadUInt32();
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(NitroCode);
				er.Write(_start_ModuleParamsOffset);
				er.Write(Unknown);
			}
			public UInt32 NitroCode;
			public UInt32 _start_ModuleParamsOffset;
			public UInt32 Unknown;
		}


		public Byte[] SubRom;
		public RomFNT Fnt;
		public class RomFNT
		{
			public RomFNT() 
			{
				DirectoryTable = new List<DirectoryTableEntry>();
				EntryNameTable = new List<EntryNameTableEntry>();
			}
			public RomFNT(EndianBinaryReader er)
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
		public RomOVT[] MainOvt;
		public RomOVT[] SubOvt;
		[Serializable]
		public class RomOVT
		{
			[Flags]
			public enum OVTFlag : byte
			{
				Compressed = 1,
				AuthenticationCode = 2
			}
			public RomOVT() { }
			public RomOVT(EndianBinaryReader er)
			{
				Id = er.ReadUInt32();
				RamAddress = er.ReadUInt32();
				RamSize = er.ReadUInt32();
				BssSize = er.ReadUInt32();
				SinitInit = er.ReadUInt32();
				SinitInitEnd = er.ReadUInt32();
				FileId = er.ReadUInt32();
				UInt32 tmp = er.ReadUInt32();
				Compressed = tmp & 0xFFFFFF;
				Flag = (OVTFlag)(tmp >> 24);
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(Id);
				er.Write(RamAddress);
				er.Write(RamSize);
				er.Write(BssSize);
				er.Write(SinitInit);
				er.Write(SinitInitEnd);
				er.Write(FileId);
				er.Write((uint)((((uint)Flag) & 0xFF) << 24 | (Compressed & 0xFFFFFF)));
			}
			[XmlAttribute]
			public UInt32 Id;
			public UInt32 RamAddress;
			public UInt32 RamSize;
			public UInt32 BssSize;
			public UInt32 SinitInit;
			public UInt32 SinitInitEnd;
			[XmlIgnore]
			public UInt32 FileId;

			public UInt32 Compressed;//:24;
			[XmlAttribute]
			public OVTFlag Flag;// :8;
		}
		public FileAllocationEntry[] Fat;
		public RomBanner Banner;
		[Serializable]
		public class RomBanner
		{
			public RomBanner() { }
			public RomBanner(EndianBinaryReader er)
			{
				Header = new BannerHeader(er);
				Banner = new BannerV1(er);
			}
			public void Write(EndianBinaryWriter er)
			{
				Header.CRC16_v1 = Banner.GetCRC();
				Header.Write(er);
				Banner.Write(er);
			}
			public BannerHeader Header;
			[Serializable]
			public class BannerHeader
			{
				public BannerHeader() { }
				public BannerHeader(EndianBinaryReader er)
				{
					Version = er.ReadByte();
					ReservedA = er.ReadByte();
					CRC16_v1 = er.ReadUInt16();
					ReservedB = er.ReadBytes(28);
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write(Version);
					er.Write(ReservedA);
					er.Write(CRC16_v1);
					er.Write(ReservedB, 0, 28);
				}
				public Byte Version;
				public Byte ReservedA;
				[XmlIgnore]
				public UInt16 CRC16_v1;
				public Byte[] ReservedB;//28
			}
			public BannerV1 Banner;
			[Serializable]
			public class BannerV1
			{
				public BannerV1() { }
				public BannerV1(EndianBinaryReader er)
				{
					Image = er.ReadBytes(32 * 32 / 2);
					Pltt = er.ReadBytes(16 * 2);
					GameName = new string[6];
					GameName[0] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
					GameName[1] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
					GameName[2] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
					GameName[3] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
					GameName[4] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
					GameName[5] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write(Image, 0, 32 * 32 / 2);
					er.Write(Pltt, 0, 16 * 2);
					foreach (string s in GameName) er.Write(GameName[0].PadRight(128, '\0'), Encoding.Unicode, false);
				}
				public Byte[] Image;//32*32/2
				public Byte[] Pltt;//16*2

				[XmlIgnore]
				public String[] GameName;//6, 128 chars (UTF16-LE)

				[XmlElement("GameName")]
				public String[] Base64GameName
				{
					get
					{
						String[] b = new String[6];
						for (int i = 0; i < 6; i++)
						{
							b[i] = Convert.ToBase64String(Encoding.Unicode.GetBytes(GameName[i]));
						}
						return b;
					}
					set
					{
						GameName = new string[6];
						for (int i = 0; i < 6; i++)
						{
							GameName[i] = Encoding.Unicode.GetString(Convert.FromBase64String(value[i]));
						}
					}
				}

				public ushort GetCRC()
				{
					byte[] Data = new byte[2080];
					Array.Copy(Image, Data, 512);
					Array.Copy(Pltt, 0, Data, 512, 32);
					Array.Copy(Encoding.Unicode.GetBytes(GameName[0].PadRight(128, '\0')), 0, Data, 544, 256);
					Array.Copy(Encoding.Unicode.GetBytes(GameName[1].PadRight(128, '\0')), 0, Data, 544 + 256, 256);
					Array.Copy(Encoding.Unicode.GetBytes(GameName[2].PadRight(128, '\0')), 0, Data, 544 + 256 * 2, 256);
					Array.Copy(Encoding.Unicode.GetBytes(GameName[3].PadRight(128, '\0')), 0, Data, 544 + 256 * 3, 256);
					Array.Copy(Encoding.Unicode.GetBytes(GameName[4].PadRight(128, '\0')), 0, Data, 544 + 256 * 4, 256);
					Array.Copy(Encoding.Unicode.GetBytes(GameName[5].PadRight(128, '\0')), 0, Data, 544 + 256 * 5, 256);
					return CRC16.GetCRC16(Data);
				}

				public Bitmap GetIcon()
				{
					return GPU.Textures.ToBitmap(Image, Pltt, 0, 32, 32, GPU.Textures.ImageFormat.PLTT16, GPU.Textures.CharFormat.CHAR, true);
				}
			}
		}
		public Byte[][] FileData;

		public Byte[] RSASignature;

		public void FromFileSystem(SFSDirectory Root)
		{
			int did = 0;
			int fid = MainOvt.Length + SubOvt.Length;
			Root.UpdateIDs(ref did, ref fid);
			//FATB.numFiles = (ushort)Root.TotalNrSubFiles;
			//List<byte> Data = new List<byte>();
			uint nrfiles = Root.TotalNrSubFiles;
			FileAllocationEntry[] overlays = new FileAllocationEntry[MainOvt.Length + SubOvt.Length];
			Array.Copy(Fat, overlays, MainOvt.Length + SubOvt.Length);
			Fat = new FileAllocationEntry[(MainOvt.Length + SubOvt.Length) + nrfiles];
			Array.Copy(overlays, Fat, MainOvt.Length + SubOvt.Length);
			byte[][] overlaydata = new byte[MainOvt.Length + SubOvt.Length][];
			Array.Copy(FileData, overlaydata, MainOvt.Length + SubOvt.Length);
			FileData = new byte[(MainOvt.Length + SubOvt.Length) + nrfiles][];
			Array.Copy(overlaydata, FileData, MainOvt.Length + SubOvt.Length);
			//FATB.allocationTable.Clear();
			for (ushort i = (ushort)(MainOvt.Length + SubOvt.Length); i < nrfiles + MainOvt.Length + SubOvt.Length; i++)
			{
				var f = Root.GetFileByID(i);
				Fat[i] = new FileAllocationEntry(0, 0);
				FileData[i] = f.Data;
			}
			Fnt.DirectoryTable.Clear();
			NitroFSUtil.GenerateDirectoryTable(Fnt.DirectoryTable, Root);
			uint offset2 = Fnt.DirectoryTable[0].dirEntryStart;
			ushort fileId = (ushort)(MainOvt.Length + SubOvt.Length);//0;
			Fnt.EntryNameTable.Clear();
			NitroFSUtil.GenerateEntryNameTable(Fnt.DirectoryTable, Fnt.EntryNameTable, Root, ref offset2, ref fileId);
		}

		public SFSDirectory ToFileSystem()
		{
			bool treereconstruct = false;//Some programs do not write the Directory Table well, so sometimes I need to reconstruct the tree based on the fnt, which is bad!
			List<SFSDirectory> dirs = new List<SFSDirectory>();
			dirs.Add(new SFSDirectory("/", true));
			dirs[0].DirectoryID = 0xF000;

			uint nrdirs = Fnt.DirectoryTable[0].dirParentID;
			for (int i = 1; i < nrdirs; i++)
			{
				dirs.Add(new SFSDirectory((ushort)(0xF000 + i)));
			}
			for (int i = 1; i < nrdirs; i++)
			{
				if (Fnt.DirectoryTable[i].dirParentID - 0xF000 == i)
				{
					treereconstruct = true;
					foreach (var v in dirs)
					{
						v.Parent = null;
					}
					break;
				}
				dirs[i].Parent = dirs[Fnt.DirectoryTable[i].dirParentID - 0xF000];
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
			ushort fileid = Fnt.DirectoryTable[0].dirEntryFileID;
			SFSDirectory curdir = null;
			foreach (EntryNameTableEntry e in Fnt.EntryNameTable)
			{
				for (int i = 0; i < nrdirs; i++)
				{
					if (offset == Fnt.DirectoryTable[i].dirEntryStart)
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
			for (int i = (MainOvt.Length + SubOvt.Length); i < Fat.Length; i++)
			{
				//byte[] data = new byte[fat[i].fileSize];
				//Array.Copy(FileData FIMG.fileImage, fat[i].fileTop, data, 0, data.Length);
				dirs[0].GetFileByID((ushort)i).Data = FileData[i];//data;
			}
			return dirs[0];
		}

		public byte[] GetDecompressedARM9()
		{
			if (StaticFooter != null) return ARM9.Decompress(MainRom, StaticFooter._start_ModuleParamsOffset);
			else return ARM9.Decompress(MainRom);
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
