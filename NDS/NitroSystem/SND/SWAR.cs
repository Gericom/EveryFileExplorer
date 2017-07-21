using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;

namespace NDS.NitroSystem.SND
{
	public class SWAR : FileFormat<SWAR.SWARIdentifier>//, IViewable
	{
		public SWARHeader Header;
		public class SWARHeader
		{
			public SWARHeader(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}
			[BinaryStringSignature("SWAR")]
			[BinaryFixedSize(4)]
			public String Signature;
			[BinaryBOM(0xFFFE)]
			public UInt16 Endianness;
			public UInt16 Version;
			public UInt32 FileSize;
			public UInt16 HeaderSize;
			public UInt16 NrBlocks;
		}

		public DATA Data;
		public class DATA
		{
			public DATA(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
				WaveInfoOffsets = er.ReadUInt32s((int)NrWaves);

			}
			[BinaryStringSignature("DATA")]
			[BinaryFixedSize(4)]
			public String Signature;
			public UInt32 SectionSize;
			[BinaryFixedSize(32)]
			public Byte[] Padding;
			public UInt32 NrWaves;
			[BinaryIgnore]
			public UInt32[] WaveInfoOffsets;
		}
		public class SWARIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Archives;
			}

			public override string GetFileDescription()
			{
				return "Nitro Sound Wave Archive (SWAR)";
			}

			public override string GetFileFilter()
			{
				return "Nitro Sound Wave Archive (*.swar)|*.swar";
			}

			public override Bitmap GetIcon()
			{
				return Resource.speaker2_box;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'W' && File.Data[2] == 'A' && File.Data[3] == 'R') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
