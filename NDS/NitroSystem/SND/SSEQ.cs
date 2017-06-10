using System;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using System.Windows.Forms;

namespace NDS.NitroSystem.SND
{
    public class SSEQ : FileFormat<SSEQ.SSEQIdentifier>, IViewable
    {
        public SSEQ(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new SSEQHeader(er);
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

        public SSEQHeader Header;
        public class SSEQHeader
        {
            public SSEQHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "SSEQ") throw new SignatureNotCorrectException(Signature, "SSEQ", er.BaseStream.Position - 4);
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

        public class SSEQIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return "SSEQ";
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Sequence (SSEQ)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Sequence (*.sseq)|*.sseq";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'S' && File.Data[2] == 'E' && File.Data[3] == 'Q') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
