using System;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using System.Text;
using System.Windows.Forms;

namespace GCNWii.JSystem
{
    public class RFNT:FileFormat<RFNT.RFNTIdentifier>, IViewable
    {
        public RFNT(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new RFNTHeader(er);
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

        public RFNTHeader Header;
        public class RFNTHeader
        {
            public RFNTHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "RFNT") throw new SignatureNotCorrectException(Signature, "RFNT", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                HeaderSize = er.ReadUInt16();
                Version = er.ReadUInt32();
                FileSize = er.ReadUInt32();
                NrBlocks = er.ReadUInt32();
            }
            public String Signature;
            public UInt16 Endianness;
            public UInt16 HeaderSize;
            public UInt32 Version;
            public UInt32 FileSize;
            public UInt32 NrBlocks;
        }

        public class RFNTIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Fonts;
            }

            public override string GetFileDescription()
            {
                return "JSystem Resource Font (RFNT)";
            }

            public override string GetFileFilter()
            {
                return "JSystem Resource Font (*.brfnt)|*.brfnt";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'F' && File.Data[2] == 'N' && File.Data[3] == 'T') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
