using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Compression;
using System.Runtime.InteropServices;

namespace CommonCompressors
{
    /*public unsafe class LZ10 : CompressionFormat<LZ10.LZ10Identifier>
    {
        public override byte[] Decompress(byte[] Data)
        {*/

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
