using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;

namespace MarioKart.MK7
{
	public class CDAB : FileFormat<CDAB.CDABIdentifier>, IViewable
	{
		public CDAB(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new CDABHeader(er);
				Shape = new SHAP(er);
				Streams = new STRM[Shape.NrStreams];
				for (int i = 0; i < Shape.NrStreams; i++)
				{
					Streams[i] = new STRM(er);
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
		public CDABHeader Header;
		public class CDABHeader
		{
			public CDABHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "BADC") throw new SignatureNotCorrectException(Signature, "BADC", er.BaseStream.Position - 4);
				FileSize = er.ReadUInt32();
				HeaderSize = er.ReadUInt32();
				Unknown = er.ReadUInt32();
			}
			public String Signature;
			public UInt32 FileSize;
			public UInt32 HeaderSize;
			public UInt32 Unknown;
		}
		public SHAP Shape;
		public class SHAP
		{
			public SHAP(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "PAHS") throw new SignatureNotCorrectException(Signature, "PAHS", er.BaseStream.Position - 4);
				NrStreams = er.ReadUInt32();
			}
			public String Signature;
			public UInt32 NrStreams;
		}
		public STRM[] Streams;
		public class STRM
		{
			public STRM(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "MRTS") throw new SignatureNotCorrectException(Signature, "MRTS", er.BaseStream.Position - 4);
				NrEntries = er.ReadUInt16();
				Unknown = er.ReadUInt16();
				Entries = new STRMEntry[NrEntries];
				for (int i = 0; i < NrEntries; i++)
				{
					Entries[i] = new STRMEntry(er);
				}
			}
			public String Signature;
			public UInt16 NrEntries;
			public UInt16 Unknown;
			public STRMEntry[] Entries;
			public class STRMEntry
			{
				public STRMEntry(EndianBinaryReader er)
				{
					Unknown1 = er.ReadSingle();
					Unknown2 = er.ReadSingle();
					Unknown3 = er.ReadSingle();
					Unknown4 = er.ReadSingle();
				}
				public Single Unknown1;
				public Single Unknown2;
				public Single Unknown3;
				public Single Unknown4;
			}
		}

		public class CDABIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return "Mario Kart 7";
			}

			public override string GetFileDescription()
			{
				return "CTR Dash AB (KMP)";
			}

			public override string GetFileFilter()
			{
				return "CTR Dash AB (*.div)|*.div";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'B' && File.Data[1] == 'A' && File.Data[2] == 'D' && File.Data[3] == 'C') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
