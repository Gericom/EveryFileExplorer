using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCNWii.GPU;
using System.IO;
using LibEveryFileExplorer.Files;
using System.Drawing;
using GCNWii.UI;
using System.Windows.Forms;
using LibEveryFileExplorer.IO;

namespace GCNWii
{
    public class TPL : FileFormat<TPL.TPLIdentifier>, IViewable, IConvertable, IFileCreatable, IWriteable
    {
        public TPL(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.BigEndian);
            try
            {
                Header = new TPLHeader(er);
                Textures = new TPLTexture[Header.NrTextures];
                for (int i = 0; i < Header.NrTextures; i++)
                {
                    Textures[i] = new TPLTexture(er);
                }
            }
            finally
            {
                er.Close();
            }
        }

        public bool CreateFromFile()
        {
            System.Windows.Forms.OpenFileDialog F = new System.Windows.Forms.OpenFileDialog();
            F.Filter = "PNG Files (*.png)|*.png";
            if (F.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && F.FileName.Length > 0)
            {
                Bitmap Bit = new Bitmap(new MemoryStream(File.ReadAllBytes(F.FileName)));
                return true;
            }
            return false;
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Texture Palette Images (*.tpl)|*.tpl";
        }

        public byte[] Write()
        {
            MemoryStream MS = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(MS, Endianness.LittleEndian);
            Header.Write(er);
            byte[] result = MS.ToArray();
            er.Close();
            return result;
        }

        public string GetConversionFileFilters()
        {
            return "Portable Network Graphics (*.png)|*.png";
        }

        public bool Convert(int FilterIndex, string Path)
        {
            switch (FilterIndex)
            {
                case 0:
                    File.Create(Path).Close();
                    Textures[0].ToBitmap().Save(Path, System.Drawing.Imaging.ImageFormat.Png);
                    return true;
            }
            return false;
        }

        public Form GetDialog()
        {
            return new TPLViewer(this);
        }

        public TPLHeader Header;
        public class TPLHeader
        {
            public TPLHeader(EndianBinaryReader er)
            {
                Signature = er.ReadBytes(4);
                if (Signature[0] != 0 || Signature[1] != 0x20 || Signature[2] != 0xAF || Signature[3] != 0x30)
                    throw new SignatureNotCorrectException("{ " + BitConverter.ToString(Signature, 0, 4).Replace("-", ", ") + " }", "{ 0x00, 0x20, 0xAF, 0x30 }", er.BaseStream.Position - 4);
                NrTextures = er.ReadUInt32();
                HeaderSize = er.ReadUInt32();
            }
            public byte[] Signature;//0x00, 0x20, 0xAF, 0x30
            public UInt32 NrTextures;
            public UInt32 HeaderSize;

            public TPLHeader(EndianBinaryReaderEx er)
            {
                er.ReadObject(this);
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature[0]);
                er.Write(Signature[1]);
                er.Write(Signature[2]);
                er.Write(Signature[3]);
                er.Write(NrTextures);
                er.Write(HeaderSize);
            }
        }
        public TPLTexture[] Textures;
        public class TPLTexture
        {
            public TPLTexture(EndianBinaryReader er)
            {
                TextureHeaderOffset = er.ReadUInt32();
                PaletteHeaderOffset = er.ReadUInt32();
                long curpos = er.BaseStream.Position;
                er.BaseStream.Position = curpos;
                er.BaseStream.Position = TextureHeaderOffset;
                TextureHeader = new TPLTextureHeader(er);
                er.BaseStream.Position = TextureHeader.TextureDataOffset;
                TextureData = er.ReadBytes(GPU.Textures.GetDataSize(TextureHeader.TextureFormat, TextureHeader.Width, TextureHeader.Height));
                if (PaletteHeaderOffset != 0)
                {
                    er.BaseStream.Position = PaletteHeaderOffset;
                    PaletteHeader = new TPLPaletteHeader(er);
                    er.BaseStream.Position = PaletteHeader.PaletteDataOffset;
                    PaletteData = er.ReadBytes((int)(PaletteHeader.NrEntries * 2));
                }
                er.BaseStream.Position = curpos;
            }
            public UInt32 TextureHeaderOffset;
            public UInt32 PaletteHeaderOffset;

            public TPLTextureHeader TextureHeader;
            public class TPLTextureHeader
            {
                public TPLTextureHeader(EndianBinaryReader er)
                {
                    Height = er.ReadUInt16();
                    Width = er.ReadUInt16();
                    TextureFormat = (Textures.ImageFormat)er.ReadUInt32();
                    TextureDataOffset = er.ReadUInt32();
                    WrapS = er.ReadUInt32();
                    WrapT = er.ReadUInt32();
                    MinFilter = er.ReadUInt32();
                    MagFilter = er.ReadUInt32();
                    LodBias = er.ReadSingle();
                    EdgeLod = er.ReadByte();
                    MinLod = er.ReadByte();
                    MaxLod = er.ReadByte();
                    Padding = er.ReadByte();
                }
                public UInt16 Height;
                public UInt16 Width;
                public Textures.ImageFormat TextureFormat;//4
                public UInt32 TextureDataOffset;
                public UInt32 WrapS;
                public UInt32 WrapT;
                public UInt32 MinFilter;
                public UInt32 MagFilter;
                public Single LodBias;
                public Byte EdgeLod;
                public Byte MinLod;
                public Byte MaxLod;
                public Byte Padding;//?
            }
            public byte[] TextureData;

            public TPLPaletteHeader PaletteHeader;
            public class TPLPaletteHeader
            {
                public TPLPaletteHeader(EndianBinaryReader er)
                {
                    NrEntries = er.ReadUInt16();
                    Padding = er.ReadUInt16();
                    PaletteFormat = (Textures.PaletteFormat)er.ReadUInt32();
                    PaletteDataOffset = er.ReadUInt32();
                }
                public UInt16 NrEntries;
                public UInt16 Padding;//?
                public Textures.PaletteFormat PaletteFormat;//4
                public UInt32 PaletteDataOffset;
            }
            public byte[] PaletteData;

            public Bitmap ToBitmap()
            {
                return GPU.Textures.ToBitmap(TextureData, 0, PaletteData, 0, TextureHeader.Width, TextureHeader.Height, TextureHeader.TextureFormat, (PaletteHeader == null ? 0 : PaletteHeader.PaletteFormat));
            }
        }
        public class TPLIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Graphics;
            }

            public override string GetFileDescription()
            {
                return "Texture Palette Images (TPL)";
            }

            public override string GetFileFilter()
            {
                return "Texture Palette Images (*.tpl)|*.tpl";
            }

            public override Bitmap GetIcon()
            {
                return Resource.image;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 0x40 && File.Data[0] == 0x00 && File.Data[1] == 0x20 && File.Data[2] == 0xAF && File.Data[3] == 0x30) return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
