using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.IO;

namespace MarioKart.MK64
{
	public class MIO0
	{
		public static byte[] Decompress(byte[] Compressed)
		{
			if (Compressed[0] != 'M' || Compressed[1] != 'I' || Compressed[2] != 'O' || Compressed[3] != '0') return null;
			uint OutputSize = IOUtil.ReadU32BE(Compressed, 4);
			byte[] Result = new byte[OutputSize];
			uint CompOffs = IOUtil.ReadU32BE(Compressed, 8);
			uint RawOffs = IOUtil.ReadU32BE(Compressed, 12);
			uint HeaderOffs = 16;
			uint OutOffs = 0;
			while (true)
			{
				byte header = Compressed[HeaderOffs++];
				for (int i = 0; i < 8; i++)
				{
					if ((header & 0x80) != 0) Result[OutOffs++] = Compressed[RawOffs++];
					else
					{
						ushort data = IOUtil.ReadU16BE(Compressed, (int)CompOffs);
						CompOffs += 2;
						int offs = (data & 0xFFF) + 1;
						int length = (data >> 12) + 3;
						for (int j = 0; j < length; j++)
						{
							Result[OutOffs] = Result[OutOffs - offs];
							OutOffs++;
						}
					}
					if (OutOffs >= OutputSize) return Result;
					header <<= 1;
				}
			}
		}
	}
}
