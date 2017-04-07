using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;
using System.Windows.Forms;

namespace MarioKart.MK7
{
	public class CDAB : FileFormat<CDAB.CDABIdentifier>, IViewable
	{
		public CDAB(byte[] Data)
		{
			EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
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
		public Form GetDialog()
		{
			return new Form();
		}
		public CDABHeader Header;
		public class CDABHeader
		{
			public CDABHeader(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}
			[BinaryStringSignature("BADC")]
			[BinaryFixedSize(4)]
			public String Signature;
			public UInt32 FileSize;
			public UInt32 HeaderSize;
			public UInt32 Unknown;
		}
		public SHAP Shape;
		public class SHAP
		{
			public SHAP(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}
			[BinaryStringSignature("PAHS")]
			[BinaryFixedSize(4)]
			public String Signature;
			public UInt32 NrStreams;
		}
		public STRM[] Streams;
		public class STRM
		{
			public STRM(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
				Entries = new STRMEntry[NrEntries];
				for (int i = 0; i < NrEntries; i++)
				{
					Entries[i] = new STRMEntry(er);
				}
			}
			[BinaryStringSignature("MRTS")]
			[BinaryFixedSize(4)]
			public String Signature;
			public UInt16 NrEntries;
			public UInt16 Unknown;
			[BinaryIgnore]
			public STRMEntry[] Entries;
			public class STRMEntry
			{
				public STRMEntry(EndianBinaryReaderEx er)
				{
					er.ReadObject(this);
					//Unknown1 = er.ReadSingle();
					//Unknown2 = er.ReadSingle();
					//Unknown3 = er.ReadSingle();
					//Unknown4 = er.ReadSingle();
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
				return "Mario Kart 7 Dash AB (KMP)";
			}

			public override string GetFileFilter()
			{
				return "Mario Kart 7 Dash AB (*.div)|*.div";
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
