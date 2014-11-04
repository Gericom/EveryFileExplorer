using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Compression;
using System.Runtime.InteropServices;

namespace CommonCompressors
{
	public unsafe class LZ77 : CompressionFormat<LZ77.LZ77Identifier>, ICompressable
	{
		public unsafe byte[] Compress(byte[] Data)
		{
			byte* dataptr = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(Data, 0);

			byte[] result = new byte[Data.Length + Data.Length / 8 + 4];
			byte* resultptr = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(result, 0);
			*resultptr++ = 0x10;
			*resultptr++ = (byte)(Data.Length & 0xFF);
			*resultptr++ = (byte)((Data.Length >> 8) & 0xFF);
			*resultptr++ = (byte)((Data.Length >> 16) & 0xFF);
			int length = Data.Length;
			int dstoffs = 4;
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
						int maxnum = 18;
						if (length - Offs < maxnum) maxnum = length - Offs;
						int maxback = 0x1000;
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
						*resultptr++ = (byte)((((back - 1) >> 8) & 0xF) | (((nr - 3) & 0xF) << 4));
						*resultptr++ = (byte)((back - 1) & 0xFF);
						dstoffs += 2;
						comp = 1;
					}
					else
					{
						*resultptr++ = *dataptr++;
						dstoffs++;
						Offs++;
					}
					header = (byte)((header << 1) | (comp & 1));
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
						byte b = Data[Offs++];
						int offs = (((a & 0xF) << 8) | b) + 1;
						int length = (a >> 4) + 3;
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

		public class LZ77Identifier : CompressionFormatIdentifier
		{
			public override string GetCompressionDescription()
			{
				return "LZ77";
			}

			public override bool IsFormat(byte[] Data)
			{
				return Data.Length > 4 && (Data[0] == 0x10 || Data[0] == 0x00);
			}
		}
	}

	public class LZ77Header : CompressionFormat<LZ77Header.LZ77HeaderIdentifier>, ICompressable
	{
		public byte[] Compress(byte[] Data)
		{
			byte[] Comp = new LZ77().Compress(Data);
			byte[] Result = new byte[Comp.Length + 4];
			Result[0] = 0x4C;
			Result[1] = 0x5A;
			Result[2] = 0x37;
			Result[3] = 0x37;
			Array.Copy(Comp, 0, Result, 4, Comp.Length);
			return Result;
		}

		public override byte[] Decompress(byte[] Data)
		{
			if (!(Data[0] == 'L' && Data[1] == 'Z' && Data[2] == '7' && Data[3] == '7')) throw new ArgumentException("LZ77 Header Missing!");
			byte[] Data2 = new byte[Data.Length - 4];
			Array.Copy(Data, 4, Data2, 0, Data.Length - 4);
			return new LZ77().Decompress(Data2);
		}

		public class LZ77HeaderIdentifier : CompressionFormatIdentifier
		{
			public override string GetCompressionDescription()
			{
				return "LZ77 (Header)";
			}

			public override bool IsFormat(byte[] Data)
			{
				return Data.Length > 8 && (Data[0] == 'L' && Data[1] == 'Z' && Data[2] == '7' && Data[3] == '7');
			}
		}
	}
}
