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
                Signature = new SSEQSignature(er);
                DATA = new DATAChunkHeader(er);
            }
            finally
            {
                er.Close();
            }
        }
        public System.Windows.Forms.Form GetDialog()
        {
            return new Form();
            //return new SSEQViewer(this);
        }

        public SSEQSignature Signature;
        public class SSEQSignature
        {
            public SSEQSignature()
            {
                Signature = "SSEQ";
                HeaderSize = 0x8;
            }

            public SSEQSignature(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "SSEQ") throw new SignatureNotCorrectException(Signature, "SSEQ", er.BaseStream.Position - 4);
                HeaderSize = er.ReadUInt16();
                Version = er.ReadUInt32();
            }
            public String Signature;
            public UInt16 HeaderSize;
            public UInt32 Version;
        }

        public DATAChunkHeader DATA;
        public class DATAChunkHeader
        {
            public DATAChunkHeader()
            {
                Signature = "DATA";
                HeaderSize = 0x4;
            }

            public DATAChunkHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                HeaderSize = er.ReadUInt16();
            }
            public String Signature;
            public UInt16 HeaderSize;
            public UInt32 Version;
        }

        public class SSEQIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return null;
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
