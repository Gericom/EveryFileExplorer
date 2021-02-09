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
	public class MSBT:FileFormat<MSBT.MSBTIdentifier>, IViewable
    {
            public Form GetDialog()
            {
                return new Form();
            }
        public MSBT(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new MSBTHeader(er);
            }
            finally
            {
                er.Close();
            }
        }

        public MSBTHeader Header;
            public class MSBTHeader
            {
                public MSBTHeader(EndianBinaryReader er)
                {
                    Signature = er.ReadString(Encoding.ASCII, 8);
                    if (Signature != "MsgStdBn") throw new SignatureNotCorrectException(Signature, "MsgStdBn", er.BaseStream.Position - 8);
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

        public class MSBTIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return "MSBT Text";
            }

            public override string GetFileDescription()
			{
				return "Message Binary Text (MSBT)";
			}

			public override string GetFileFilter()
			{
				return "Message Binary Text (*.msbt)|*.msbt";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
			{
                if (File.Data.Length > 10 && File.Data[0] == 'M' && File.Data[1] == 's' && File.Data[2] == 'g' && File.Data[3] == 'S' && File.Data[4] == 't' && File.Data[5] == 'd' && File.Data[6] == 'B' && File.Data[7] == 'n') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
