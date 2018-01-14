using System;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using System.Text;
using System.Windows.Forms;

namespace GCNWii.JSystem
{
    public class RSTM : FileFormat<RSTM.RSTMIdentifier>, IViewable
    {
        public RSTM(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new RSTMHeader(er);
                Head = new HEAD();
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

        public RSTMHeader Header;
        public class RSTMHeader
        {
            public RSTMHeader()
            {
                Signature = "RSTM";
                Endianness = 0xFEFF;
                HeaderSize = 0x40;
                Version = 0x02000000;
            }
            public RSTMHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "RSTM") throw new SignatureNotCorrectException(Signature, "RSTM", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                HeaderSize = er.ReadUInt16();
                Version = er.ReadUInt32();
            }
            public String Signature;
            public UInt16 Endianness;
            public UInt16 HeaderSize;
            public UInt32 Version;
        }

        public HEAD Head;
        public class HEAD
        {
            public HEAD()
            {
                Signature = "HEAD";
            }
            public HEAD(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "HEAD") throw new SignatureNotCorrectException(Signature, "HEAD", er.BaseStream.Position - 4);
            }
            public String Signature;
        }
        public class RSTMIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Audio;
            }

            public override string GetFileDescription()
            {
                return "JSystem Resource Stream (RSTM)";
            }

            public override string GetFileFilter()
            {
                return "JSystem Resource Stream (*.brstm)|*.brstm";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'S' && File.Data[2] == 'T' && File.Data[3] == 'M') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}