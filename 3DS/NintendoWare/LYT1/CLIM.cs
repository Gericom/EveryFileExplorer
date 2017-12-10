using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using _3DS.UI;
using _3DS.GPU;
using LibEveryFileExplorer.IO;

namespace _3DS.NintendoWare.LYT1
{
    public class CLIM : FileFormat<CLIM.CLIMIdentifier>, IConvertable, IFileCreatable, IViewable, IWriteable
    {
        public CLIM()
        {
            Data = new byte[0];
            Header = new CLIMHeader();
            Image = new imag();
        }
        public CLIM(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            er.BaseStream.Position = Data.Length - 0x28;
            try
            {
                Header = new CLIMHeader(er);
                Image = new imag(er);
                DataLength = er.ReadUInt32();
                er.BaseStream.Position = 0;
                this.Data = er.ReadBytes((int)DataLength);
            }
            finally
            {
                er.Close();
            }
        }

        public System.Windows.Forms.Form GetDialog()
        {
            return new CLIMViewer(this);
        }

        public string GetSaveDefaultFileFilter()
        {
            return "CTR Layout Images (*.bclim)|*.bclim";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
            er.Write(Data, 0, Data.Length);
            Header.Write(er);
            Image.Write(er);
            er.Write(DataLength);
            long curpos = er.BaseStream.Position;
            er.BaseStream.Position = Data.Length + 0xC;
            er.Write((uint)curpos);
            er.BaseStream.Position = curpos;
            byte[] result = m.ToArray();
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
                    ToBitmap().Save(Path, System.Drawing.Imaging.ImageFormat.Png);
                    return true;
            }
            return false;
        }

        public bool CreateFromFile()
        {
            System.Windows.Forms.OpenFileDialog f = new System.Windows.Forms.OpenFileDialog();
            f.Filter = "PNG Files (*.png)|*.png";
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && f.FileName.Length > 0)
            {
                Bitmap b = new Bitmap(new MemoryStream(File.ReadAllBytes(f.FileName)));
                UI.BCLIMGenDialog BC = new BCLIMGenDialog();
                BC.ShowDialog();
                Image.Width = (ushort)b.Width;
                Image.Height = (ushort)b.Height;
                switch (BC.index)
                {
                    case 0:
                        //Image.Format = 0;
                        //Data = Textures.FromBitmap(b, Textures.ImageFormat.L8);
                        //break;
                        throw new NotSupportedException("L8 Format is not supported.");
                    case 1:
                        //Image.Format = 1;
                        //Data = Textures.FromBitmap(b, Textures.ImageFormat.A8);
                        //break;
                        throw new NotSupportedException("A8 Format is not supported.");
                    case 2:
                        //Image.Format = 2;
                        //Data = Textures.FromBitmap(b, Textures.ImageFormat.LA4);
                        //break;
                        throw new NotSupportedException("LA4 Format is not supported.");
                    case 3:
                        //Image.Format = 3;
                        //Data = Textures.FromBitmap(b, Textures.ImageFormat.LA8);
                        //break;
                        throw new NotSupportedException("LA8 Format is not supported.");
                    case 4:
                        //Image.Format = 4;
                        //Data = Textures.FromBitmap(b, Textures.ImageFormat.HILO8);
                        //break;
                        throw new NotSupportedException("HILO8 Format is not supported.");
                    case 5:
                        Image.Format = 5;
                        Data = Textures.FromBitmap(b, Textures.ImageFormat.RGB565);
                        break;
                    case 6:
                        Image.Format = 6;
                        Data = Textures.FromBitmap(b, Textures.ImageFormat.RGB8);
                        break;
                    case 7:
                        Image.Format = 7;
                        Data = Textures.FromBitmap(b, Textures.ImageFormat.RGBA5551);
                        break;
                    case 8:
                        Image.Format = 8;
                        Data = Textures.FromBitmap(b, Textures.ImageFormat.RGBA4);
                        break;
                    case 9:
                        Image.Format = 9;
                        Data = Textures.FromBitmap(b, Textures.ImageFormat.RGBA8);
                        break;
                    case 10:
                        Image.Format = 10;
                        Data = Textures.FromBitmap(b, Textures.ImageFormat.ETC1);
                        break;
                    case 11:
                        Image.Format = 11;
                        Data = Textures.FromBitmap(b, Textures.ImageFormat.ETC1A4);
                        break;
                    case 12:
                        //Image.Format = 12;
                        //Data = Textures.FromBitmap(b, Textures.ImageFormat.L4);
                        //break;
                        throw new NotSupportedException("L4 Format is not supported.");
                    case 13:
                        //Image.Format = 13;
                        //Data = Textures.FromBitmap(b, Textures.ImageFormat.A4);
                        //break;
                        throw new NotSupportedException("A4 Format is not supported.");
                    default:
                        throw new Exception("Please select the Image format!");
                }
                DataLength = (uint)Data.Length;
                return true;
            }
            return false;
        }

        public byte[] Data;

        public CLIMHeader Header;
        public class CLIMHeader
        {
            public CLIMHeader()
            {
                Signature = "CLIM";
                Endianness = 0xFEFF;
                HeaderSize = 0x14;
                Version = 0x02020000;
                NrBlocks = 1;
            }
            public CLIMHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "CLIM") throw new SignatureNotCorrectException(Signature, "CLIM", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                HeaderSize = er.ReadUInt16();
                Version = er.ReadUInt32();
                FileSize = er.ReadUInt32();
                NrBlocks = er.ReadUInt32();
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(Endianness);
                er.Write(HeaderSize);
                er.Write(Version);
                er.Write((uint)0);
                er.Write(NrBlocks);
            }
            public String Signature;
            public UInt16 Endianness;
            public UInt16 HeaderSize;
            public UInt32 Version;
            public UInt32 FileSize;
            public UInt32 NrBlocks;
        }

        public imag Image;
        public class imag
        {
            public imag()
            {
                Signature = "imag";
                SectionSize = 0x10;
            }
            public imag(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "imag") throw new SignatureNotCorrectException(Signature, "imag", er.BaseStream.Position);
                SectionSize = er.ReadUInt32();
                Width = er.ReadUInt16();
                Height = er.ReadUInt16();
                Format = er.ReadUInt32();
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(SectionSize);
                er.Write(Width);
                er.Write(Height);
                er.Write(Format);
            }
            public String Signature;
            public UInt32 SectionSize;
            public UInt16 Width;
            public UInt16 Height;
            public UInt32 Format;

            public Textures.ImageFormat GetGPUTextureFormat()
            {
                switch (Format)
                {
                    case 0: return Textures.ImageFormat.L8;
                    case 1: return Textures.ImageFormat.A8;
                    case 2: return Textures.ImageFormat.LA4;
                    case 3: return Textures.ImageFormat.LA8;
                    case 4: return Textures.ImageFormat.HILO8;
                    case 5: return Textures.ImageFormat.RGB565;
                    case 6: return Textures.ImageFormat.RGB8;
                    case 7: return Textures.ImageFormat.RGBA5551;
                    case 8: return Textures.ImageFormat.RGBA4;
                    case 9: return Textures.ImageFormat.RGBA8;
                    case 10: return Textures.ImageFormat.ETC1;
                    case 11: return Textures.ImageFormat.ETC1A4;
                    case 12: return Textures.ImageFormat.L4;
                    case 13: return Textures.ImageFormat.A4;
                    case 19: return Textures.ImageFormat.ETC1;
                }
                throw new Exception("Unknown Image Format!");
            }
        }
        public UInt32 DataLength;

        public Bitmap ToBitmap()
        {
            return GPU.Textures.ToBitmap(Data, Image.Width, Image.Height, Image.GetGPUTextureFormat());
        }

        public class CLIMIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Graphics;
            }

            public override string GetFileDescription()
            {
                return "CTR Layout Images (CLIM)";
            }

            public override string GetFileFilter()
            {
                return "CTR Layout Images (*.bclim)|*.bclim";
            }

            public override Bitmap GetIcon()
            {
                return Resource.image;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 0x28 && File.Data[File.Data.Length - 0x28] == 'C' && File.Data[File.Data.Length - 0x27] == 'L' && File.Data[File.Data.Length - 0x26] == 'I' && File.Data[File.Data.Length - 0x25] == 'M' && IOUtil.ReadU32LE(File.Data, File.Data.Length - 0x4) == (File.Data.Length - 0x28)) return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}
