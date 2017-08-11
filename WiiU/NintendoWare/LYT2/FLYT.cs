using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.Collections;
using System.IO;
using WiiU.UI;

namespace WiiU.NintendoWare.LYT2
{
    public class FLYT : FileFormat<FLYT.FLYTIdentifier>, IViewable
    {
        public FLYT(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new FLYTHeader(er);
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new FLYTViewer(this);
        }

        public FLYTHeader Header;
        public class FLYTHeader
        {
            public FLYTHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "FLYT") throw new SignatureNotCorrectException(Signature, "FLYT", er.BaseStream.Position - 4);
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
        public lyt1 Layout;
        public class lyt1
        {
            public enum ScreenOriginType : uint
            {
                Classic = 0,
                Normal = 1
            }
            public lyt1(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "lyt1") throw new SignatureNotCorrectException(Signature, "lyt1", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                ScreenOrigin = (ScreenOriginType)er.ReadUInt32();
                LayoutSize = er.ReadVector2();
            }
            public String Signature;
            public UInt32 SectionSize;
            public ScreenOriginType ScreenOrigin;//u32
            public Vector2 LayoutSize;
        }

        public class FLYTIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Layouts;
            }

            public override string GetFileDescription()
            {
                return "Cafe Layout (FLYT)";
            }

            public override string GetFileFilter()
            {
                return "Cafe Layout (*.bflyt)|*.bflyt";
            }

            public override Bitmap GetIcon()
            {
                return Resource.zone;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'F' && File.Data[1] == 'L' && File.Data[2] == 'Y' && File.Data[3] == 'T') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
