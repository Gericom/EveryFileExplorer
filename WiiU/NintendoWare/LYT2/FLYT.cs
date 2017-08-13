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
                //int blocknr = 0;
                //while (blocknr < Header.NrBlocks)
                //{
                //    String sig = er.ReadString(Encoding.ASCII, 4);
                //    switch (sig)
                //    {
                //        case "lyt1": Layout = new lyt1(er); break;
                //        case "txl1": TextureList = new txl1(er); break;
                //    }
                //}
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

        public txl1 TextureList;
        public class txl1
        {
            public txl1(EndianBinaryReader er)
            {
                long startpos = er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "txl1") throw new SignatureNotCorrectException(Signature, "txl1", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                NrTextures = er.ReadUInt32();
                long baseoffset = er.BaseStream.Position;
                TextureNameOffsets = er.ReadUInt32s((int)NrTextures);
                TextureNames = new string[NrTextures];
                for (int i = 0; i < NrTextures; i++)
                {
                    er.BaseStream.Position = baseoffset + TextureNameOffsets[i];
                    TextureNames[i] = er.ReadStringNT(Encoding.ASCII);
                }
                //padding
                er.BaseStream.Position = startpos + SectionSize;
            }
            public String Signature;
            public UInt32 SectionSize;
            public UInt32 NrTextures;
            public UInt32[] TextureNameOffsets;

            public String[] TextureNames;
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