using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace NDS.NitroSystem.G2D
{
    public class NCLR : FileFormat<NCLR.NCLRIdentifier>, IViewable
    {
        public NCLR(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new NCLRHeader(er);
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

        public NCLRHeader Header;
        public class NCLRHeader
        {
            public NCLRHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "RLCN") throw new SignatureNotCorrectException(Signature, "RLCN", er.BaseStream.Position - 4);
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

        public class NCLRIdentifier:FileFormatIdentifier
		{
            public override string GetCategory()
			{
				return Category_Palettes;
			}

			public override string GetFileDescription()
			{
				return "Nitro Color Palette For Runtime (NCLR)";
			}

			public override string GetFileFilter()
			{
				return "Nitro Color Palette For Runtime (*.nclr)|*.nclr";
			}

			public override Bitmap GetIcon()
			{
				return Resource.color_swatch1;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'L' && File.Data[2] == 'C' && File.Data[3] == 'N') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
