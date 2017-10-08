using System;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using System.Text;
using System.Windows.Forms;

namespace GCNWii.JSystem
{
    public class RLYT : FileFormat<RLYT.RLYTIdentifier>, IViewable
    {
        public RLYT(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new RLYTHeader(er);
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new Form();
            //return new RLYTViewer(this);
        }

        public RLYTHeader Header;
        public class RLYTHeader
        {
            public RLYTHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "RLYT") throw new SignatureNotCorrectException(Signature, "RLYT", er.BaseStream.Position - 4);
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

        //public lyt1 Layout;
        //public class lyt1

        public class RLYTIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Layouts;
            }

            public override string GetFileDescription()
            {
                return "JSystem Resource Layout (RLYT)";
            }

            public override string GetFileFilter()
            {
                return "JSystem Resource Layout (*.brlyt)|*.brlyt";
            }

            public override Bitmap GetIcon()
            {
                return Resource.zone;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'L' && File.Data[2] == 'Y' && File.Data[3] == 'T') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
