using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Compression;
using System.Runtime.InteropServices;

namespace CommonCompressors
{
	public unsafe class YAZ0 : CompressionFormat<YAZ0.YAZ0Identifier>, ICompressable
	{
		//Compression could be optimized by using look-ahead.
		public unsafe byte[] Compress(byte[] Data)
		{
			byte* dataptr = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(Data, 0);

			byte[] result = new byte[Data.Length + Data.Length / 8 + 0x10];
			byte* resultptr = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(result, 0);
			*resultptr++ = (byte)'Y';
			*resultptr++ = (byte)'a';
			*resultptr++ = (byte)'z';
			*resultptr++ = (byte)'0';
			*resultptr++ = (byte)((Data.Length >> 24) & 0xFF);
			*resultptr++ = (byte)((Data.Length >> 16) & 0xFF);
			*resultptr++ = (byte)((Data.Length >> 8) & 0xFF);
			*resultptr++ = (byte)((Data.Length >> 0) & 0xFF);
			for (int i = 0; i < 8; i++) *resultptr++ = 0;
			int length = Data.Length;
			int dstoffs = 16;
			int Offs = 0;
			while (true)
			{
				int headeroffs = dstoffs++;
				resultptr++;
				byte header = 0;
				for (int i = 0; i < 8; i++)
				{
					int comp = 0;
					int back = 1;
					int nr = 2;
					{
						byte* ptr = dataptr - 1;
						int maxnum = 0x111;
						if (length - Offs < maxnum) maxnum = length - Offs;
						//Use a smaller amount of bytes back to decrease time
						int maxback = 0x400;//0x1000;
						if (Offs < maxback) maxback = Offs;
						maxback = (int)dataptr - maxback;
						int tmpnr;
						while (maxback <= (int)ptr)
						{
							if (*(ushort*)ptr == *(ushort*)dataptr && ptr[2] == dataptr[2])
							{
								tmpnr = 3;
								while (tmpnr < maxnum && ptr[tmpnr] == dataptr[tmpnr]) tmpnr++;
								if (tmpnr > nr)
								{
									if (Offs + tmpnr > length)
									{
										nr = length - Offs;
										back = (int)(dataptr - ptr);
										break;
									}
									nr = tmpnr;
									back = (int)(dataptr - ptr);
									if (nr == maxnum) break;
								}
							}
							--ptr;
						}
					}
					if (nr > 2)
					{
						Offs += nr;
						dataptr += nr;
						if (nr >= 0x12)
						{
							*resultptr++ = (byte)(((back - 1) >> 8) & 0xF);
							*resultptr++ = (byte)((back - 1) & 0xFF);
							*resultptr++ = (byte)((nr - 0x12) & 0xFF);
							dstoffs += 3;
						}
						else
						{
							*resultptr++ = (byte)((((back - 1) >> 8) & 0xF) | (((nr - 2) & 0xF) << 4));
							*resultptr++ = (byte)((back - 1) & 0xFF);
							dstoffs += 2;
						}
						comp = 1;
					}
					else
					{
						*resultptr++ = *dataptr++;
						dstoffs++;
						Offs++;
					}
					header = (byte)((header << 1) | ((comp == 1) ? 0 : 1));
					if (Offs >= length)
					{
						header = (byte)(header << (7 - i));
						break;
					}
				}
				result[headeroffs] = header;
				if (Offs >= length) break;
			}
			while ((dstoffs % 4) != 0) dstoffs++;
			byte[] realresult = new byte[dstoffs];
			Array.Copy(result, realresult, dstoffs);
			return realresult;
		}

		public override byte[] Decompress(byte[] Data)
		{
			UInt32 leng = (uint)(Data[4] << 24 | Data[5] << 16 | Data[6] << 8 | Data[7]);
			byte[] Result = new byte[leng];
			int Offs = 16;
			int dstoffs = 0;
			while (true)
			{
				byte header = Data[Offs++];
				for (int i = 0; i < 8; i++)
				{
					if ((header & 0x80) != 0) Result[dstoffs++] = Data[Offs++];
					else
					{
						byte b = Data[Offs++];
						int offs = ((b & 0xF) << 8 | Data[Offs++]) + 1;
						int length = (b >> 4) + 2;
						if (length == 2) length = Data[Offs++] + 0x12;
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

		public class YAZ0Identifier : CompressionFormatIdentifier
		{
			public override string GetCompressionDescription()
			{
				return "YAZ0";
			}

			public override bool IsFormat(byte[] Data)
			{
				return Data.Length > 16 && (Data[0] == 'Y' && Data[1] == 'a' && Data[2] == 'z' && Data[3] == '0');
			}
		}
	}
}
