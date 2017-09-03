using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Compression;
using System.Runtime.InteropServices;

namespace CommonCompressors
{
    public unsafe class LZ10 : CompressionFormat<LZ10.LZ10Identifier>
    {
        public override byte[] Decompress(byte[] Data)
        {
            UInt32 leng = (uint)(Data[1] | (Data[2] << 8) | (Data[3] << 16));
            byte[] Result = new byte[leng];
            int Offs = 4;
            int dstoffs = 0;
            while (true)
            {
                byte header = Data[Offs++];
                for (int i = 0; i < 8; i++)
                {
                    if ((header & 0x80) == 0) Result[dstoffs++] = Data[Offs++];
                    else
                    {
                        byte a = Data[Offs++];
                        int offs;
                        int length;
                        if ((a >> 4) == 0)
                        {
                            byte b = Data[Offs++];
                            byte c = Data[Offs++];
                            length = (((a & 0xF) << 4) | (b >> 4)) + 0x10;
                            offs = (((b & 0xF) << 8) | c) + 1;
                        }

                        if (dstoffs >= leng) return Result;
                        header <<= 1;
                    }
                }
            }
        }
        public class LZ10Identifier : CompressionFormatIdentifier
        {
            public override string GetCompressionDescription()
            {
                return "LZ10";
            }

            public override bool IsFormat(byte[] Data)
            {
                return Data.Length > 4 && Data[0] == 0x10;
            }
        }
    }
}
