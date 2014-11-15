using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Compression;
using System.Runtime.InteropServices;

namespace CommonCompressors
{
	public unsafe class LZ11 : CompressionFormat<LZ11.LZ11Identifier>
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
							length = (((a & 0xF) << 4) | (b >> 4)) + 0x11;
							offs = (((b & 0xF) << 8) | c) + 1;
						}
						else if ((a >> 4) == 1)
						{
							byte b = Data[Offs++];
							byte c = Data[Offs++];
							byte d = Data[Offs++];
							length = (((a & 0xF) << 12) | (b << 4) | (c >> 4)) + 0x111;
							offs = (((c & 0xF) << 8) | d) + 1;
						}
						else
						{
							byte b = Data[Offs++];
							offs = (((a & 0xF) << 8) | b) + 1;
							length = (a >> 4) + 1;
						}
						for (int j = 0; j < length; j++)
						{
							Result[dstoffs] = Result[dstoffs - offs];
							dstoffs++;
						}
					}
					if (dstoffs >= leng) return Result;
					header <<= 1;
				}
			}
		}

		public class LZ11Identifier : CompressionFormatIdentifier
		{
			public override string GetCompressionDescription()
			{
				return "LZ11";
			}

			public override bool IsFormat(byte[] Data)
			{
				return Data.Length > 4 && Data[0] == 0x11;
			}
		}
	}
}
