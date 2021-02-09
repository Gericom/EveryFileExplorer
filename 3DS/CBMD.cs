using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;
using LibEveryFileExplorer.IO;
using System.IO;
using System.Runtime.InteropServices;

namespace _3DS
{
    public class CBMD : FileFormat<CBMD.CBMDIdentifier>, IViewable
    {
        public CBMD(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new CBMDHeader(er);
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

        public CBMDHeader Header;
        public class CBMDHeader
        {
            public CBMDHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "CBMD") throw new SignatureNotCorrectException(Signature, "CBMD", er.BaseStream.Position - 4);
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

        public class CBMDIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return "3DS";
            }

            public override string GetFileDescription()
            {
                return "CTR Banner Model Data (CBMD)";
            }

            public override string GetFileFilter()
            {
                return "CTR Banner Model Data (*.cbmd)|*.cbmd";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'B' && File.Data[2] == 'M' && File.Data[3] == 'D') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
