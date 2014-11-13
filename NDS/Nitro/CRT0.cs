using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.IO;
using System.IO;

namespace NDS.Nitro
{
	public class CRT0
	{
		public class ModuleParams
		{
			public ModuleParams(byte[] Data, uint Offset)
			{
				AutoLoadListOffset = IOUtil.ReadU32LE(Data, (int)Offset + 0);
				AutoLoadListEnd = IOUtil.ReadU32LE(Data, (int)Offset + 4);
				AutoLoadStart = IOUtil.ReadU32LE(Data, (int)Offset + 8);
				StaticBssStart = IOUtil.ReadU32LE(Data, (int)Offset + 12);
				StaticBssEnd = IOUtil.ReadU32LE(Data, (int)Offset + 16);
				CompressedStaticEnd = IOUtil.ReadU32LE(Data, (int)Offset + 20);
				SDKVersion = IOUtil.ReadU32LE(Data, (int)Offset + 24);
				NitroCodeBE = IOUtil.ReadU32LE(Data, (int)Offset + 28);
				NitroCodeLE = IOUtil.ReadU32LE(Data, (int)Offset + 32);
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(AutoLoadListOffset);
				er.Write(AutoLoadListEnd);
				er.Write(AutoLoadStart);
				er.Write(StaticBssStart);
				er.Write(StaticBssEnd);
				er.Write(CompressedStaticEnd);
				er.Write(SDKVersion);
				er.Write(NitroCodeBE);
				er.Write(NitroCodeLE);
			}
			public UInt32 AutoLoadListOffset;
			public UInt32 AutoLoadListEnd;
			public UInt32 AutoLoadStart;
			public UInt32 StaticBssStart;
			public UInt32 StaticBssEnd;
			public UInt32 CompressedStaticEnd;
			public UInt32 SDKVersion;
			public UInt32 NitroCodeBE;
			public UInt32 NitroCodeLE;
		}

		public class AutoLoadEntry
		{
			public AutoLoadEntry(UInt32 Address, byte[] Data)
			{
				this.Address = Address;
				this.Data = Data;
				Size = (uint)Data.Length;
				BssSize = 0;
			}
			public AutoLoadEntry(byte[] Data, uint Offset)
			{
				Address = IOUtil.ReadU32LE(Data, (int)Offset + 0);
				Size = IOUtil.ReadU32LE(Data, (int)Offset + 4);
				BssSize = IOUtil.ReadU32LE(Data, (int)Offset + 8);
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(Address);
				er.Write(Size);
				er.Write(BssSize);
			}
			public UInt32 Address;
			public UInt32 Size;
			public UInt32 BssSize;

			public byte[] Data;
		}

		public static byte[] MIi_UncompressBackward(byte[] Data)
		{
			UInt32 leng = IOUtil.ReadU32LE(Data, Data.Length - 4) + (uint)Data.Length;
			byte[] Result = new byte[leng];
			Array.Copy(Data, Result, Data.Length);
			int Offs = (int)(Data.Length - (IOUtil.ReadU32LE(Data, Data.Length - 8) >> 24));
			int dstoffs = (int)leng;
			while (true)
			{
				byte header = Result[--Offs];
				for (int i = 0; i < 8; i++)
				{
					if ((header & 0x80) == 0) Result[--dstoffs] = Result[--Offs];
					else
					{
						byte a = Result[--Offs];
						byte b = Result[--Offs];
						int offs = (((a & 0xF) << 8) | b) + 2;//+ 1;
						int length = (a >> 4) + 2;
						do
						{
							Result[dstoffs - 1] = Result[dstoffs + offs];
							dstoffs--;
							length--;
						}
						while (length >= 0);
					}
					if (Offs <= (Data.Length - (IOUtil.ReadU32LE(Data, Data.Length - 8) & 0xFFFFFF))) return Result;
					header <<= 1;
				}
			}
		}
	}
}
