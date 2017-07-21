using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;
using LibEveryFileExplorer.IO;
using System.IO;

namespace _3DS
{
	public class DVLB : FileFormat<DVLB.DVLBIdentifier>, IViewable
    {
        public Form GetDialog()
        {
            return new Form();
        }

        public DVLB(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new DVLBHeader(er);
            }
            finally
            {
                er.Close();
            }
        }

        public DVLBHeader Header;
        public class DVLBHeader
        {
            public DVLBHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DVLB") throw new SignatureNotCorrectException(Signature, "DVLB", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                HeaderSize = er.ReadUInt16();
                Version = er.ReadUInt32();
                FileSize = er.ReadUInt32();
                FileTableOffset = er.ReadUInt32();
                FileTableLength = er.ReadUInt32();
                FileDataOffset = er.ReadUInt32();
            }
            public String Signature;
            public UInt16 Endianness;
            public UInt16 HeaderSize;
            public UInt32 Version;
            public UInt32 FileSize;
            public UInt32 FileTableOffset;
            public UInt32 FileTableLength;
            public UInt32 FileDataOffset;
        }

        public class DVLBIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Shaders;
			}

			public override string GetFileDescription()
			{
				return "DMP Vertex Linker Binary (DVLB)";
			}

			public override string GetFileFilter()
			{
				return "DMP Vertex Linker Binary (*.shbin)|*.shbin";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'D' && File.Data[1] == 'V' && File.Data[2] == 'L' && File.Data[3] == 'B') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
