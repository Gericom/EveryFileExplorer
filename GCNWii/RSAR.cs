using System;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using System.Text;
using System.Windows.Forms;

namespace GCNWii.JSystem
{
    public class RSAR : FileFormat<RSAR.RSARIdentifier>, IViewable
    {
        public RSAR(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new RSARHeader(er);
            }

            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new Form();
            //return new RSARViewer(this);
        }

        public RSARHeader Header;
        public class RSARHeader
        {
            public RSARHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "RSAR") throw new SignatureNotCorrectException(Signature, "RSAR", er.BaseStream.Position - 4);
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

        public SYMB SymbolBlock;
        public class SYMB
        {
            public SYMB(EndianBinaryReader er)
            {
                long baseoffset = er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "SYMB") throw new SignatureNotCorrectException(Signature, "SYMB", er.BaseStream.Position - 4);
            }
            public String Signature;
        }


        public class RSARIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Sound;
            }

            public override string GetFileDescription()
            {
                return "JSystem Resource Sound Archive (RSAR)";
            }

            public override string GetFileFilter()
            {
                return "JSystem Resource Sound Archive (*.brsar)|*.brsar";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'S' && File.Data[2] == 'A' && File.Data[3] == 'R' && File.Data[0x40] == 'S' && File.Data[0x41] == 'Y' && File.Data[0x42] == 'M' && File.Data[0x43] == 'B') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
