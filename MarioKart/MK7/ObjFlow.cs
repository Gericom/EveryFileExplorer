using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;

namespace MarioKart.MK7
{
	public class ObjFlow : FileFormat<ObjFlow.ObjFlowIdentifier>, IViewable
	{
		public ObjFlow(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new ObjFlowHeader(er);
				Objects = new ObjFlowEntry[Header.NrObjects];
				//int lastid = 0;
				for (int i = 0; i < Header.NrObjects; i++)
				{
					Objects[i] = new ObjFlowEntry(er);
					//for (int j = 0; j < Objects[i].ObjectID - lastid - 1; j++) Console.WriteLine("\"?\",");
					//lastid = Objects[i].ObjectID;
					//Console.WriteLine("\"" + Objects[i].Name + "\",");
				}
			}
			finally
			{
				er.Close();
			}
		}

		public System.Windows.Forms.Form GetDialog()
		{
			throw new NotImplementedException();
		}

		public ObjFlowHeader Header;
		public class ObjFlowHeader
		{
			public ObjFlowHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "FBOC") throw new SignatureNotCorrectException(Signature, "FBOC", er.BaseStream.Position - 4);
				HeaderSize = er.ReadUInt16();
				NrObjects = er.ReadUInt16();
			}
			public String Signature;
			public UInt16 HeaderSize;
			public UInt16 NrObjects;
		}
		public ObjFlowEntry[] Objects;
		public class ObjFlowEntry
		{
			public ObjFlowEntry(EndianBinaryReader er)
			{
				ObjectID = er.ReadUInt16();
				Unknown1 = er.ReadUInt16();
				Unknown2 = er.ReadUInt16();
				Unknown3 = er.ReadUInt16();
				Unknown4 = er.ReadUInt32();
				Unknown5 = er.ReadUInt32();
				Unknown6 = er.ReadUInt32();
				Unknown7 = er.ReadUInt16();
				Unknown8 = er.ReadUInt16();
				Unknown9 = er.ReadUInt16();
				Unknown10 = er.ReadUInt16();
				Unknown11 = er.ReadUInt32();
				Name = er.ReadString(Encoding.ASCII, 0x40).Replace("\0", "");
				ParticleName = er.ReadString(Encoding.ASCII, 0x40).Replace("\0", "");
				//Console.WriteLine("|-");
				//Console.WriteLine(string.Format("|{0:X2}{1:X2}||{2}||{3}", (ObjectID >> 8), (ObjectID & 0xFF), Name, ParticleName));
			}
			public UInt16 ObjectID;
			public UInt16 Unknown1;
			public UInt16 Unknown2;
			public UInt16 Unknown3;
			public UInt32 Unknown4;
			public UInt32 Unknown5;
			public UInt32 Unknown6;
			public UInt16 Unknown7;
			public UInt16 Unknown8;
			public UInt16 Unknown9;
			public UInt16 Unknown10;
			public UInt32 Unknown11;
			public String Name;
			public String ParticleName;

			public override string ToString()
			{
				return Name + " (" + string.Format("{0:X2}{1:X2}", (ObjectID >> 8), (ObjectID & 0xFF)) + ")";
			}
		}

		public class ObjFlowIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return "Mario Kart 7";
			}

			public override string GetFileDescription()
			{
				return "Mario Kart 7 ObjFlow (FBOC)";
			}

			public override string GetFileFilter()
			{
				return "Mario Kart 7 ObjFlow (ObjFlow.bin)|ObjFlow.bin";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'F' && File.Data[1] == 'B' && File.Data[2] == 'O' && File.Data[3] == 'C') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
