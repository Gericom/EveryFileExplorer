using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;
using LibEveryFileExplorer.IO;
using System.IO;
using LibEveryFileExplorer.Files.SimpleFileSystem;

namespace _3DS
{
    public class CTPK : FileFormat<CTPK.CTPKIdentifier>, IViewable
    {
        public CTPK()
        {
          //Header = new CTPKHeader();
          //FromFileSystem(new SFSDirectory("/", true)
        }
            public CTPK(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new CTPKHeader(er);

            }
            catch (SignatureNotCorrectException e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        public CTPKHeader Header;
        public class CTPKHeader
        {
            public CTPKHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "CTPK") throw new SignatureNotCorrectException(Signature, "CTPK", er.BaseStream.Position - 4);
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

        public class CTPKIdentifier : FileFormatIdentifier
        {
             public override string GetCategory()
            {
                return Category_Archives;
            }

                public override string GetFileDescription()
            {
                return "CTR Texture Package (CTPK)";
            }

            public override string GetFileFilter()
            {
                return "CTR Texture Package (*.ctpk)|*.ctpk";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'T' && File.Data[2] == 'P' && File.Data[3] == 'K') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
