using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using System.Windows.Forms;
using WiiU.UI;
using System.Drawing.Imaging;
using LibEveryFileExplorer.IO.Serialization;
using _3DS.GPU;

namespace WiiU.NintendoWare.LYT2
{
    public class FLIM : FileFormat<FLIM.FLIMIdentifier>, IConvertable, IViewable, IWriteable, IFileCreatable
    {
        public FLIM()
        {
            Data = new byte[0];
            Header = new FLIMHeader();
            Image = new imag();
        }

        public FLIM(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
            er.BaseStream.Position = Data.Length - 0x28;
            try
            {
                Header = new FLIMHeader(er);
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

        public Form GetDialog()
        {
            return new FLIMViewer(this);
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Cafe Layout Images (*.bflim)|*.bflim";
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
                    ToBitmap().Save(Path, ImageFormat.Png);
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
                Image.Width = (ushort)b.Width;
                Image.Height = (ushort)b.Height;
                Image.Format = 11;
                Data = _3DS.GPU.Textures.FromBitmap(b,_3DS.GPU.Textures.ImageFormat.ETC1A4);
                DataLength = (uint)Data.Length;
                return true;
            }
            return false;
        }

        public byte[] Data;

        public FLIMHeader Header;
        public class FLIMHeader
        {
            public FLIMHeader()
            {
                Signature = "FLIM";
                Endianness = 0xFEFF;
                HeaderSize = 0x14;
                Version = 0x02020000;
                NrBlocks = 1;
            }

            public FLIMHeader(EndianBinaryReaderEx er)
            {
                er.ReadObject(this);
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
            [BinaryStringSignature("FLIM")]
            [BinaryFixedSize(4)]
            public String Signature;
            [BinaryBOM(0xFFFE)]
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
            public imag(EndianBinaryReaderEx er)
            {
                er.ReadObject(this);
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(SectionSize);
                er.Write(Width);
                er.Write(Height);
                er.Write(Format);
                er.Write(DataLength);
            }
            [BinaryStringSignature("imag")]
            [BinaryFixedSize(4)]
            public String Signature;
            public UInt32 SectionSize;
            public UInt16 Width;
            public UInt16 Height;
            public UInt16 Alignment;
            public Byte Format;
            public Byte Unknown;
            public UInt32 DataLength;

            //Tempoarly use 3ds stuff!
            public _3DS.GPU.Textures.ImageFormat GetGPUTextureFormat()
			{
				switch (Format)
				{
					case 0: return _3DS.GPU.Textures.ImageFormat.L8;
					case 1: return _3DS.GPU.Textures.ImageFormat.A8;
					case 2: return _3DS.GPU.Textures.ImageFormat.LA4;
					case 3: return _3DS.GPU.Textures.ImageFormat.LA8;
					case 4: return _3DS.GPU.Textures.ImageFormat.HILO8;
					case 5: return _3DS.GPU.Textures.ImageFormat.RGB565;
					case 6: return _3DS.GPU.Textures.ImageFormat.RGB8;
					case 7: return _3DS.GPU.Textures.ImageFormat.RGBA5551;
					case 8: return _3DS.GPU.Textures.ImageFormat.RGBA4;
					case 9: return _3DS.GPU.Textures.ImageFormat.RGBA8;
					case 10: return _3DS.GPU.Textures.ImageFormat.ETC1;
					case 11: return _3DS.GPU.Textures.ImageFormat.ETC1A4;
					case 0x12: return _3DS.GPU.Textures.ImageFormat.L4;
					case 0x13: return _3DS.GPU.Textures.ImageFormat.A4;
					//Wii U Formats:
					//case 0x17: return WiiU.GPU.Textures.ImageFormat.ETC1A4;
				}
				throw new Exception("Unknown Image Format!");
			}
        }
        public UInt32 DataLength;

        //Tempoarly use 3ds stuff!
        public Bitmap ToBitmap()
        {
            if (Header.Endianness == 0xFFFE)//3ds
            {
                _3DS.GPU.Textures.ImageFormat f3 = 0;
                switch (Image.Format)
                {
                    case 0: f3 = _3DS.GPU.Textures.ImageFormat.L8; break;
                    case 1: f3 = _3DS.GPU.Textures.ImageFormat.A8; break;
                    case 2: f3 = _3DS.GPU.Textures.ImageFormat.LA4; break;
                    case 3: f3 = _3DS.GPU.Textures.ImageFormat.LA8; break;
                    case 4: f3 = _3DS.GPU.Textures.ImageFormat.HILO8; break;
                    case 5: f3 = _3DS.GPU.Textures.ImageFormat.RGB565; break;
                    case 6: f3 = _3DS.GPU.Textures.ImageFormat.RGB8; break;
                    case 7: f3 = _3DS.GPU.Textures.ImageFormat.RGBA5551; break;
                    case 8: f3 = _3DS.GPU.Textures.ImageFormat.RGBA4; break;
                    case 9: f3 = _3DS.GPU.Textures.ImageFormat.RGBA8; break;
                    case 10: f3 = _3DS.GPU.Textures.ImageFormat.ETC1; break;
                    case 11: f3 = _3DS.GPU.Textures.ImageFormat.ETC1A4; break;
                    case 0x12: f3 = _3DS.GPU.Textures.ImageFormat.L4; break;
                    case 0x13: f3 = _3DS.GPU.Textures.ImageFormat.A4; break;
                    default: throw new Exception("Unknown Image Format!");
                }
                if (Image.Unknown == 0) return _3DS.GPU.Textures.ToBitmap(Data, Image.Width, Image.Height, f3);
                return _3DS.GPU.Textures.ToBitmap(Data, Image.Height, Image.Width, f3);
            }
            else
            {
                GPU.Textures.ImageFormat fu = 0;
                _3DS.GPU.Textures.ImageFormat f3 = 0;
                switch (Image.Format)
                {
                    case 0: f3 = _3DS.GPU.Textures.ImageFormat.L8; break;
                    case 1: f3 = _3DS.GPU.Textures.ImageFormat.A8; break;
                    case 2: f3 = _3DS.GPU.Textures.ImageFormat.LA4; break;
                    case 3: f3 = _3DS.GPU.Textures.ImageFormat.LA8; break;
                    case 4: f3 = _3DS.GPU.Textures.ImageFormat.HILO8; break;
                    case 5: fu = GPU.Textures.ImageFormat.RGB565; break;
                    case 6: f3 = _3DS.GPU.Textures.ImageFormat.RGB8; break;
                    case 7: f3 = _3DS.GPU.Textures.ImageFormat.RGBA5551; break;
                    case 8: f3 = _3DS.GPU.Textures.ImageFormat.RGBA4; break;
                    case 9: f3 = _3DS.GPU.Textures.ImageFormat.RGBA8; break;
                    case 10: f3 = _3DS.GPU.Textures.ImageFormat.ETC1; break;
                    case 11: f3 = _3DS.GPU.Textures.ImageFormat.ETC1A4; break;
                    case 0x12: f3 = _3DS.GPU.Textures.ImageFormat.L4; break;
                    case 0x13: f3 = _3DS.GPU.Textures.ImageFormat.A4; break;
                    case 0x17: fu = GPU.Textures.ImageFormat.DXT5; break;
                    default: throw new Exception("Unknown Image Format!");
                }
                return GPU.Textures.ToBitmap(Data, Image.Width, Image.Height, fu, (GPU.Textures.TileMode)(Image.Unknown & 0x1F), (uint)Image.Unknown >> 5);
            }
        }

        public class FLIMIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Graphics;
            }

            public override string GetFileDescription()
            {
                return "Cafe Layout Images (FLIM)";
            }

            public override string GetFileFilter()
            {
                return "Cafe Layout Images (*.bflim)|*.bflim";
            }

            public override Bitmap GetIcon()
            {
                return Resource.image;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 0x28 && File.Data[File.Data.Length - 0x28] == 'F' && File.Data[File.Data.Length - 0x27] == 'L' && File.Data[File.Data.Length - 0x26] == 'I' && File.Data[File.Data.Length - 0x25] == 'M' && (IOUtil.ReadU32LE(File.Data, File.Data.Length - 0x4) == (File.Data.Length - 0x28) || IOUtil.ReadU32BE(File.Data, File.Data.Length - 0x4) == (File.Data.Length - 0x28))) return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}
