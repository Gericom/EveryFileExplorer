using System;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using System.Windows.Forms;
using _3DS.UI;

namespace _3DS.NintendoWare.SND
{
    public class CSEQ : FileFormat<CSEQ.CSEQIdentifier>, IViewable
    {
        public CSEQ(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new CSEQHeader(er);
            }
            finally
            {
                er.Close();
            }
        }
        public System.Windows.Forms.Form GetDialog()
        {
            return new CSEQViewer(this);
        }

        public CSEQHeader Header;
        public class CSEQHeader
        {
            public CSEQHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "CSEQ") throw new SignatureNotCorrectException(Signature, "CSEQ", er.BaseStream.Position - 4);
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

        public class CSEQIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return null;
            }

            public override string GetFileDescription()
            {
                return "CTR Sequence (CSEQ)";
            }

            public override string GetFileFilter()
            {
                return "CTR Sequence (*.bcseq, *.cseq)|*.bcseq;*.cseq";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'S' && File.Data[2] == 'E' && File.Data[3] == 'Q') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
