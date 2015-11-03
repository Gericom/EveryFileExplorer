using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;
using LibEveryFileExplorer.Files;

namespace NDS.NitroSystem.SND
{
	public class SWAV : FileFormat<SWAR.SWARIdentifier>//, IViewable
	{
		public SWAVHeader Header;
		public class SWAVHeader
		{
			public SWAVHeader(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}
			[BinaryStringSignature("SWAV")]
			[BinaryFixedSize(4)]
			public String Signature;
			[BinaryBOM(0xFFFE)]
			public UInt16 Endianness;
			public UInt16 Version;
			public UInt32 FileSize;
			public UInt16 HeaderSize;
			public UInt16 NrBlocks;
		}
	}
}
