using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.Files;
using System.Drawing;
using _3DS.UI;
using System.Windows.Forms;
using LibEveryFileExplorer.IO;

namespace _3DS
{
	public class SARCHashTable : FileFormat<SARCHashTable.SAHTIdentifier>, IEmptyCreatable, IViewable, IWriteable
	{
		public static SARCHashTable DefaultHashTable;

		static SARCHashTable()
		{
			try
			{
				String path = Path.GetDirectoryName(Application.ExecutablePath) + "\\Plugins\\HashTable.saht";
				if (File.Exists(path)) DefaultHashTable = new SARCHashTable(File.ReadAllBytes(path));
				else DefaultHashTable = null;
			}
			catch
			{
				DefaultHashTable = null;
			}
		}

		public SARCHashTable()
		{
			Header = new SAHTHeader();
			Entries = new List<SAHTEntry>();
		}

		public SARCHashTable(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new SAHTHeader(er);
				er.BaseStream.Position = Header.DataOffset;
				Entries = new List<SAHTEntry>();
				for (int i = 0; i < Header.NrEntries; i++) Entries.Add(new SAHTEntry(er));
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new SAHTViewer(this);
		}

		public string GetSaveDefaultFileFilter()
		{
			return "SARC Hash Table (*.saht)|*.saht";
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			Header.NrEntries = (uint)Entries.Count;
			Header.Write(er);

			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = 8;
			er.Write((uint)curpos);
			er.BaseStream.Position = curpos;

			for (int i = 0; i < Entries.Count; i++) Entries[i].Write(er);

			er.BaseStream.Position = 4;
			er.Write((UInt32)(er.BaseStream.Length));
			byte[] result = m.ToArray();
			er.Close();
			return result;
		}

		public SAHTHeader Header;
		public class SAHTHeader
		{
			public SAHTHeader()
			{
				Signature = "SAHT";
			}
			public SAHTHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "SAHT") throw new SignatureNotCorrectException(Signature, "SAHT", er.BaseStream.Position - 4);
				FileSize = er.ReadUInt32();
				DataOffset = er.ReadUInt32();
				NrEntries = er.ReadUInt32();
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(Signature, Encoding.ASCII, false);
				er.Write((uint)0);
				er.Write((uint)0);
				er.Write(NrEntries);
			}
			public String Signature;
			public UInt32 FileSize;
			public UInt32 DataOffset;
			public UInt32 NrEntries;
		}
		public List<SAHTEntry> Entries;
		public class SAHTEntry
		{
			public SAHTEntry(String Name)
			{
				this.Name = Name;
				Hash = SARC.GetHashFromName(Name, 0x65);
			}
			public SAHTEntry(EndianBinaryReader er)
			{
				Hash = er.ReadUInt32();
				Name = er.ReadStringNT(Encoding.ASCII);
				while ((er.BaseStream.Position % 0x10) != 0) er.ReadByte();
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(Hash);
				er.Write(Name, Encoding.ASCII, true);
				while ((er.BaseStream.Position % 0x10) != 0) er.Write((byte)0);
			}
			public UInt32 Hash;
			public String Name;
		}

		public SAHTEntry GetEntryByHash(UInt32 Hash)
		{
			foreach (var v in Entries) if (v.Hash == Hash) return v;
			return null;
		}

		public SAHTEntry GetEntryByName(String Name)
		{
			foreach (var v in Entries) if (v.Name == Name) return v;
			return null;
		}

		public class SAHTIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return "SARC Hash Tables";
			}

			public override string GetFileDescription()
			{
				return "SARC Hash Table (SAHT)";
			}

			public override string GetFileFilter()
			{
				return "SARC Hash Table (*.saht)|*.saht";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length >= 16 && File.Data[0] == 'S' && File.Data[1] == 'A' && File.Data[2] == 'H' && File.Data[3] == 'T') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
