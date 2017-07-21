using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.SND
{
    public class SWAV:FileFormat<SWAV.SWAVIdentifier>//, IViewable
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
        public class SWAVIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return null;
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Wave (SWAV)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Wave (*.swav)|*.swav";
            }

            public override Bitmap GetIcon()
            {
                return Resource.speaker2_box;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'W' && File.Data[2] == 'A' && File.Data[3] == 'V') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
