using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.GFX;

namespace WiiU.GPU
{
    public class Textures
    {
        public enum TileMode : uint
        {
            Default = 0,
            LinearAligned = 1,
            Tiled1DThin1 = 2,
            Tiled1DThick = 3,
            Tiled2DThin1 = 4,
            Tiled2DThin2 = 5,
            Tiled2DThin4 = 6,
            Tiled2DThick = 7,
            Tiled2BThin1 = 8,
            Tiled2BThin2 = 9,
            Tiled2BThin4 = 10,
            Tiled2BThick = 11,
            Tiled3DThin1 = 12,
            Tiled3DThick = 13,
            Tiled3BThin1 = 14,
            Tiled3BThick = 15,
            LinearSpecial = 16
        }

        public enum ImageFormat : uint
        {
            RGBA8 = 0,
            RGB8 = 1,
            RGBA5551 = 2,
            RGB565 = 3,
            RGBA4 = 4,
            LA8 = 5,
            HILO8 = 6,
            L8 = 7,
            A8 = 8,
            LA4 = 9,
            L4 = 10,
            A4 = 11,
            ETC1 = 12,
            ETC1A4 = 13,
            DXT5 = 14,
        }

        //private static readonly int[] Bpp = { 32, 24, 16, 16, 16, 16, 16, 8, 8, 8, 4, 4, 4, 8 };

        private static readonly int[] TileOrder =
        {
             0,  1,   4,  5,
             2,  3,   6,  7,

             8,  9,  12, 13,
            10, 11,  14, 15
        };
        //1. Swap column 1 and 2, 4 and 5 etc.
        //2. Put back 8x8 tiles in 16x16 blocks in the order 0, 3
        //													 1, 2

        private static readonly int[,] ETC1Modifiers =
        {
            { 2, 8 },
            { 5, 17 },
            { 9, 29 },
            { 13, 42 },
            { 18, 60 },
            { 24, 80 },
            { 33, 106 },
            { 47, 183 }
        };

        //public static int GetBpp(ImageFormat Format) { return Bpp[(uint)Format]; }

        public static Bitmap ToBitmap(byte[] Data, int Width, int Height, ImageFormat Format, TileMode TileMode, uint SwizzleMode, bool ExactSize = false)
        {
            return ToBitmap(Data, 0, Width, Height, Format, TileMode, SwizzleMode, ExactSize);
        }

        public static unsafe Bitmap ToBitmap(byte[] Data, int Offset, int Width, int Height, ImageFormat Format, TileMode TileMode, uint SwizzleMode, bool ExactSize = false)
        {
            if (Data == null || Data.Length < 1 || Offset < 0 || Offset >= Data.Length || Width < 1 || Height < 1) return null;
            if (ExactSize && ((Width % 8) != 0 || (Height % 8) != 0)) return null;
            int physicalwidth = Width;
            int physicalheight = Height;
            if (!ExactSize)
            {
                Width = 1 << (int)Math.Ceiling(Math.Log(Width, 2));
                Height = 1 << (int)Math.Ceiling(Math.Log(Height, 2));
            }
            Bitmap bitm = new Bitmap(Width, Height);//physicalwidth, physicalheight);
            BitmapData d = bitm.LockBits(new Rectangle(0, 0, bitm.Width, bitm.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            uint* res = (uint*)d.Scan0;
            int offs = Offset;//0;
            int stride = d.Stride / 4;
            switch (Format)
            {
                case ImageFormat.RGBA8:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (y >= physicalheight) continue;
                            res[y * stride + x] =
                                GFXUtil.ConvertColorFormat(
                                    IOUtil.ReadU32LE(Data, offs),
                                    ColorFormat.RGBA8888,
                                    ColorFormat.ARGB8888);
                            offs += 4;
                        }
                    }
                    break;
                case ImageFormat.RGB8:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (y >= physicalheight) continue;
                            res[y * stride + x] =
                                GFXUtil.ConvertColorFormat(
                                    IOUtil.ReadU24LE(Data, offs),
                                    ColorFormat.RGB888,
                                    ColorFormat.ARGB8888);
                            offs += 3;
                        }
                    }
                    break;
                case ImageFormat.RGBA5551:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (y >= physicalheight) continue;
                            res[y * stride + x] =
                                GFXUtil.ConvertColorFormat(
                                    IOUtil.ReadU16LE(Data, offs),
                                    ColorFormat.RGBA5551,
                                    ColorFormat.ARGB8888);
                            offs += 2;
                        }
                    }
                    break;
                case ImageFormat.RGB565:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            //if (x >= physicalwidth) continue;
                            if (y >= physicalheight) continue;
                            res[y * stride + x] =
                                GFXUtil.ConvertColorFormat(
                                    IOUtil.ReadU16LE(Data, offs),
                                    ColorFormat.RGB565,
                                    ColorFormat.ARGB8888);
                            offs += 2;
                        }
                    }
                    break;
                case ImageFormat.RGBA4:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (y >= physicalheight) continue;
                            res[y * stride + x] =
                                GFXUtil.ConvertColorFormat(
                                    IOUtil.ReadU16LE(Data, offs),
                                    ColorFormat.RGBA4444,
                                    ColorFormat.ARGB8888);
                            offs += 2;
                        }
                    }
                    break;
                case ImageFormat.LA8: throw new NotImplementedException("LA8 format is not implemented yet.");
                case ImageFormat.HILO8: throw new NotImplementedException("HILO8 format is not implemented yet.");
                case ImageFormat.L8: throw new NotImplementedException("L8 format is not implemented yet.");
                case ImageFormat.A8: throw new NotImplementedException("A8 format is not implemented yet.");
                case ImageFormat.LA4: throw new NotImplementedException("LA4 format is not implemented yet.");
                case ImageFormat.L4: throw new NotImplementedException("L4 format is not implemented yet.");
                case ImageFormat.A4: throw new NotImplementedException("A4 format is not implemented yet.");
                case ImageFormat.ETC1://Some reference: http://www.khronos.org/registry/gles/extensions/OES/OES_compressed_ETC1_RGB8_texture.txt
                case ImageFormat.ETC1A4:
                    {
                        for (int y = 0; y < Height; y += 8)
                        {
                            for (int x = 0; x < Width; x += 8)
                            {
                                for (int i = 0; i < 8; i += 4)
                                {
                                    for (int j = 0; j < 8; j += 4)
                                    {
                                        ulong alpha = 0xFFFFFFFFFFFFFFFF;
                                        ulong data = IOUtil.ReadU64BE(Data, offs);
                                        if (Format == ImageFormat.ETC1A4)
                                        {
                                            offs += 8;
                                            alpha = IOUtil.ReadU64BE(Data, offs);
                                        }
                                        bool diffbit = ((data >> 33) & 1) == 1;
                                        bool flipbit = ((data >> 32) & 1) == 1; //0: |||, 1: |-|
                                        int r1, r2, g1, g2, b1, b2;
                                        if (diffbit) //'differential' mode
                                        {
                                            int r = (int)((data >> 59) & 0x1F);
                                            int g = (int)((data >> 51) & 0x1F);
                                            int b = (int)((data >> 43) & 0x1F);
                                            r1 = (r << 3) | ((r & 0x1C) >> 2);
                                            g1 = (g << 3) | ((g & 0x1C) >> 2);
                                            b1 = (b << 3) | ((b & 0x1C) >> 2);
                                            r += (int)((data >> 56) & 0x7) << 29 >> 29;
                                            g += (int)((data >> 48) & 0x7) << 29 >> 29;
                                            b += (int)((data >> 40) & 0x7) << 29 >> 29;
                                            r2 = (r << 3) | ((r & 0x1C) >> 2);
                                            g2 = (g << 3) | ((g & 0x1C) >> 2);
                                            b2 = (b << 3) | ((b & 0x1C) >> 2);
                                        }
                                        else //'individual' mode
                                        {
                                            r1 = (int)((data >> 60) & 0xF) * 0x11;
                                            g1 = (int)((data >> 52) & 0xF) * 0x11;
                                            b1 = (int)((data >> 44) & 0xF) * 0x11;
                                            r2 = (int)((data >> 56) & 0xF) * 0x11;
                                            g2 = (int)((data >> 48) & 0xF) * 0x11;
                                            b2 = (int)((data >> 40) & 0xF) * 0x11;
                                        }
                                        int Table1 = (int)((data >> 37) & 0x7);
                                        int Table2 = (int)((data >> 34) & 0x7);
                                        for (int y3 = 0; y3 < 4; y3++)
                                        {
                                            for (int x3 = 0; x3 < 4; x3++)
                                            {
                                                if (x + j + x3 >= physicalwidth) continue;
                                                if (y + i + y3 >= physicalheight) continue;

                                                int val = (int)((data >> (x3 * 4 + y3)) & 0x1);
                                                bool neg = ((data >> (x3 * 4 + y3 + 16)) & 0x1) == 1;
                                                uint c;
                                                if ((flipbit && y3 < 2) || (!flipbit && x3 < 2))
                                                {
                                                    int add = ETC1Modifiers[Table1, val] * (neg ? -1 : 1);
                                                    c = GFXUtil.ToColorFormat((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r1 + add), (byte)ColorClamp(g1 + add), (byte)ColorClamp(b1 + add), ColorFormat.ARGB8888);
                                                }
                                                else
                                                {
                                                    int add = ETC1Modifiers[Table2, val] * (neg ? -1 : 1);
                                                    c = GFXUtil.ToColorFormat((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r2 + add), (byte)ColorClamp(g2 + add), (byte)ColorClamp(b2 + add), ColorFormat.ARGB8888);
                                                }
                                                res[(i + y3) * stride + x + j + x3] = c;
                                            }
                                        }
                                        offs += 8;
                                    }
                                }
                            }
                            res += stride * 8;
                        }
                    }
                    break;
                case ImageFormat.DXT5:
                    for (int y2 = 0; y2 < Height; y2 += 4)
                    {
                        for (int x2 = 0; x2 < Width; x2 += 4)
                        {
                            ulong a_data = IOUtil.ReadU64LE(Data, offs);
                            byte[] AlphaPalette = new byte[8];
                            AlphaPalette[0] = (byte)(a_data & 0xFF);
                            AlphaPalette[1] = (byte)((a_data >> 8) & 0xFF);
                            a_data >>= 16;
                            if (AlphaPalette[0] > AlphaPalette[1])
                            {
                                AlphaPalette[2] = (byte)((6 * AlphaPalette[0] + 1 * AlphaPalette[1]) / 7);
                                AlphaPalette[3] = (byte)((5 * AlphaPalette[0] + 2 * AlphaPalette[1]) / 7);
                                AlphaPalette[4] = (byte)((4 * AlphaPalette[0] + 3 * AlphaPalette[1]) / 7);
                                AlphaPalette[5] = (byte)((3 * AlphaPalette[0] + 4 * AlphaPalette[1]) / 7);
                                AlphaPalette[6] = (byte)((2 * AlphaPalette[0] + 5 * AlphaPalette[1]) / 7);
                                AlphaPalette[7] = (byte)((1 * AlphaPalette[0] + 6 * AlphaPalette[1]) / 7);
                            }
                            else
                            {
                                AlphaPalette[2] = (byte)((4 * AlphaPalette[0] + 1 * AlphaPalette[1]) / 5);
                                AlphaPalette[3] = (byte)((3 * AlphaPalette[0] + 2 * AlphaPalette[1]) / 5);
                                AlphaPalette[4] = (byte)((2 * AlphaPalette[0] + 3 * AlphaPalette[1]) / 5);
                                AlphaPalette[5] = (byte)((1 * AlphaPalette[0] + 4 * AlphaPalette[1]) / 5);
                                AlphaPalette[6] = 0;
                                AlphaPalette[7] = 255;
                            }
                            offs += 8;
                            ushort color0 = IOUtil.ReadU16LE(Data, offs);
                            ushort color1 = IOUtil.ReadU16LE(Data, offs + 2);
                            uint data = IOUtil.ReadU32LE(Data, offs + 4);
                            uint[] Palette = new uint[4];
                            Palette[0] = GFXUtil.ConvertColorFormat(color0, ColorFormat.RGB565, ColorFormat.ARGB8888);
                            Palette[1] = GFXUtil.ConvertColorFormat(color1, ColorFormat.RGB565, ColorFormat.ARGB8888);
                            Color a = System.Drawing.Color.FromArgb((int)Palette[0]);
                            Color b = System.Drawing.Color.FromArgb((int)Palette[1]);
                            if (color0 > color1)//1/3 and 2/3
                            {
                                Palette[2] = GFXUtil.ToColorFormat((a.R * 2 + b.R * 1) / 3, (a.G * 2 + b.G * 1) / 3, (a.B * 2 + b.B * 1) / 3, ColorFormat.ARGB8888);
                                Palette[3] = GFXUtil.ToColorFormat((a.R * 1 + b.R * 2) / 3, (a.G * 1 + b.G * 2) / 3, (a.B * 1 + b.B * 2) / 3, ColorFormat.ARGB8888);
                            }
                            else//1/2 and transparent
                            {
                                Palette[2] = GFXUtil.ToColorFormat((a.R + b.R) / 2, (a.G + b.G) / 2, (a.B + b.B) / 2, ColorFormat.ARGB8888);
                                Palette[3] = 0;
                            }

                            int q = 30;
                            int aq = 45;
                            for (int y3 = 0; y3 < 4; y3++)
                            {
                                for (int x3 = 0; x3 < 4; x3++)
                                {
                                    //if (x2 + x3 >= physicalwidth) continue;
                                    if (y2 + y3 >= physicalheight) continue;
                                    res[(y2 + y3) * stride + x2 + x3] = (Palette[(data >> q) & 3] & 0xFFFFFF) | ((uint)AlphaPalette[(a_data >> aq) & 7] << 24);
                                    q -= 2;
                                    aq -= 3;
                                }
                            }
                            offs += 8;
                        }
                    }
                    break;
            }
            Detile(res, stride, Width, Height, physicalwidth, physicalheight, TileMode);
            bitm.UnlockBits(d);
            return bitm;
        }

        private static unsafe void Detile(uint* res, int stride, int width, int height, int physicalwidth, int physicalheight, TileMode Mode)
        {
            switch (Mode)
            {
                case Textures.TileMode.Tiled2DThin1:
                    DetileTiled2DThin1(res, stride, width, height, physicalwidth, physicalheight);
                    return;
                default:
                    throw new Exception("Unsupported Tilemode!");
            }
        }

        //private static readonly int[] Tiled2DThin1OrderA = { 0, 1, 3, 2 };
        //private static readonly int[] Tiled2DThin1OrderB = { 0, 2, 3, 1 };
        private static readonly int[] Tiled2DThin1Order = { 2, 0, 1, 3 };

        //Micro tiles: 8x8
        //Macro tiles: 32x16 (4x2 tiles = 4 banks, 2 pipes)

        //Alignment: 2048 pixels (I think), which matches exactly with the 128x16 blocks

        //First block:
        //  Macro:
        //      1 2  5 6
        //      0 3  4 7
        //Second block:
        //  Macro:
        //      5 6  1 2
        //      4 7  0 3
        //Third block:
        //  Macro:
        //      3 0  7 4
        //      2 1  6 5
        //Fourth block:
        //  Macro:
        //      7 4  3 0
        //      6 5  2 1
        private static unsafe void DetileTiled2DThin1(uint* res, int stride, int width, int height, int physicalwidth, int physicalheight)
        {
            uint[] right_order = new uint[width * height];
            int q = 0;
            //Let's put the tiles in the right order
            for (int y = 0; y < height; y += 8)
            {
                for (int xx = 0; xx < width; xx += 64)
                {
                    for (int yy = 0; yy < 8; yy++)
                    {
                        for (int x = 0; x < 64; x++)
                        {
                            right_order[q++] = res[(y + yy) * stride + x + xx];
                        }
                    }
                }
            }

            q = 0;
            uint[] Result = new uint[width * height];
            for (int y = 0; y < height; y += 8)
            {
                for (int x = 0; x < width; x += 8)
                {
                    for (int yy = 0; yy < 8; yy++)
                    {
                        for (int xx = 0; xx < 8; xx++)
                        {
                            Result[(y + yy) * width + x + xx] = right_order[q++];
                        }
                    }
                }
            }

            //

            /* uint[] Result = new uint[width * height];
            int px = 0;
            int py = 0;
            for (int y = 0; y < height; y += /*8/16)
            {
            for (int x = 0; x < width; x += 64)
            {

                /*for (int y2 = 0; y2 < 8; y2++)
                {
                    for (int x2 = 0; x2 < 64; x2++)
                    {
                        Result[(y + (x2 / 8)) * width + x + y2 * 8 + x2 % 8] =
                            res[(y + y2) * stride + x + x2];
                    }
                }/
                /*for (int i = 0; i < 64; i++)//y2 = 0; y2 < 64; y2 += 8)
                {
                    int tile = i;
                    int q = Tiled2DThin1Order[(tile / 2) % 4];
                    int p = tile & ~7;
                    int xx = (p % 16) * 2 + (q % 2) * 8;
                    //if ((i / 0x20) == 1)
                    //{
                    //xx += (i % 2) * 8;
                    //}
                    /*else /
                    xx += (i % 2) * 32;
                    int yy = p / 16 * 16 + (q / 2) * 8;
                    for (int y3 = 0; y3 < 8; y3++)
                    {
                        for (int x3 = 0; x3 < 8; x3++)
                        {
                            if (y + y3 + yy >= height || x + x3 + xx >= width) continue;
                            //if (x + x2 + x3 >= physicalwidth) continue;
                            //if (y + y3 + yy >= physicalheight) continue;
                            Result[(y + y3 + yy) * width + x + x3 + xx] = ReadPixel(res, stride, width, height, ref px, ref py);
                        }
                    }
                }/
            }
            }*/
            //TODO: We now have the tiles, so start constructing the macro tiles

            /*R600Tiling t = new R600Tiling();
            R600Tiling._ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_INPUT r = new R600Tiling._ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_INPUT();
            r.bpp = 32;
            r.tileMode = 4;
            r.tileType = 0;
            r.pitch = (uint)stride;
            r.height = (uint)height;
            r.numSlices = 1;
            r.numSamples = (uint)(height * width);
            R600Tiling._ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT o = new R600Tiling._ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT();
            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    r.x = (uint)x;
                    r.y = (uint)y;
                    r.sample = (uint)i;
                    int pixel_number = (int)t.ComputeSurfaceAddrFromCoord(ref r, ref o);
                    /*int pixel_number = 0;
                    pixel_number |= ((x >> 0) & 1) << 0; // pn[0] = x[0]
                    pixel_number |= ((x >> 1) & 1) << 1; // pn[1] = x[1]
                    pixel_number |= ((x >> 2) & 1) << 2; // pn[2] = x[2]
                    pixel_number |= ((y >> 1) & 1) << 3; // pn[3] = y[1]
                    pixel_number |= ((y >> 0) & 1) << 4; // pn[4] = y[0]
                    pixel_number |= ((y >> 2) & 1) << 5; // pn[5] = y[2]/

                    Result[y * width + x] = res[(pixel_number & 0xFFF) / 4];
                    i++;
                }
            }*/
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //if (x >= physicalwidth) continue;
                    if (y >= physicalheight) continue;
                    res[y * stride + x] = Result[y * width + x];
                }
            }
            /*
            //Swap columns first!
            for (int c = 0; c < width; c += 16)
            {
                //Swap c + 4 and c + 8
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        if (c + x >= physicalwidth) continue;
                        if (y >= physicalheight) continue;
                        uint a = res[y * stride + c + x + 4];
                        uint b = res[y * stride + c + x + 8];
                        res[y * stride + c + x + 4] = b;
                        res[y * stride + c + x + 8] = a;
                    }
                }
            }
            uint[] Result = new uint[width * height];
            //work in 16x16 and then in 8x8 tiles
            int px = 0;
            int py = 0;
            for (int y = 0; y < height; y += 32)
            {
                for (int x = 0; x < width; x += 32)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        int x4 = (Tiled2DThin1OrderA[j] % 2) * 16;
                        int y4 = ((Tiled2DThin1OrderA[j] & 2) >> 1) * 16;
                        //Read out the 256 pixels needed!
                        uint[] Pixels = new uint[256];
                        for (int i = 0; i < 256; i++) Pixels[i] = ReadPixel(res, stride, width, height, ref px, ref py);
                        int idx = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            int x2 = (Tiled2DThin1OrderB[i] % 2) * 8;
                            int y2 = ((Tiled2DThin1OrderB[i] & 2) >> 1) * 8;
                            for (int y3 = 0; y3 < 8; y3++)
                            {
                                for (int x3 = 0; x3 < 8; x3++)
                                {
                                    Result[(y + y2 + y3 + y4) * width + x + x2 + x3 + x4] = Pixels[idx++];
                                    //res[(y + y2 + y3) * stride + x + x2 + x3] = Pixels[idx++];
                                }
                            }
                        }
                    }
                }
            }
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x >= physicalwidth) continue;
                    if (y >= physicalheight) continue;
                    res[y * stride + x] = Result[y * width + x];
                }
            }
            */
        }

        private static unsafe uint ReadPixel(uint* res, int stride, int width, int height, ref int px, ref int py)
        {
            if (px >= width || py >= height || px < 0 || py < 0)
                return 0;//throw new ArgumentException("ReadPixel fail!");
            uint result = res[py * stride + px];
            px++;
            if (px == width)
            {
                px = 0;
                py++;
            }
            return result;
        }

        private static int ColorClamp(int Color)
        {
            if (Color > 255) Color = 255;
            if (Color < 0) Color = 0;
            return Color;
        }

    }
}
