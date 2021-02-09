using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using LibEveryFileExplorer.IO;

namespace RuneFactory.RF4
{
    public class JPN:FileFormat<JPN.JPNIdentifier>, IViewable
    {
        public JPN(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new JPNHeader(er);
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

        public JPNHeader Header;
        public class JPNHeader
        {
            public JPNHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "TEXT") throw new SignatureNotCorrectException(Signature, "TEXT", er.BaseStream.Position - 4);
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

        public class JPNIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return "Rune Factory 4";
            }

            public override string GetFileDescription()
            {
                return "Rune Factory 4 Text (TEXT)";
            }

            public override string GetFileFilter()
            {
                return "Rune Factory 4 Text (*.jpn)|*.jpn";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'T' && File.Data[1] == 'E' && File.Data[2] == 'X' && File.Data[3] == 'T') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }

    }
}
