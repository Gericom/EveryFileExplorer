using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.IO;
using System.IO;

namespace NDS.Nitro
{
	public class ARM9
	{
		private UInt32 RamAddress;

		private byte[] StaticData;

		private UInt32 _start_ModuleParamsOffset;
		private CRT0.ModuleParams _start_ModuleParams;

		private List<CRT0.AutoLoadEntry> AutoLoadList;

		public ARM9(byte[] Data, UInt32 RamAddress)
			: this(Data, RamAddress, FindModuleParams(Data)) { }

		public ARM9(byte[] Data, UInt32 RamAddress, UInt32 _start_ModuleParamsOffset)
		{
			//Unimportant static footer! Use it for _start_ModuleParamsOffset and remove it.
			if (IOUtil.ReadU32LE(Data, Data.Length - 12) == 0xDEC00621)
			{
				_start_ModuleParamsOffset = IOUtil.ReadU32LE(Data, Data.Length - 8);
				byte[] data_tmp = new byte[Data.Length - 12];
				Array.Copy(Data, data_tmp, Data.Length - 12);
				Data = data_tmp;
			}

			this.RamAddress = RamAddress;
			this._start_ModuleParamsOffset = _start_ModuleParamsOffset;
			_start_ModuleParams = new CRT0.ModuleParams(Data, _start_ModuleParamsOffset);
			if (_start_ModuleParams.CompressedStaticEnd != 0)
			{
				Data = Decompress(Data, _start_ModuleParamsOffset);
				_start_ModuleParams = new CRT0.ModuleParams(Data, _start_ModuleParamsOffset);
			}

			StaticData = new byte[_start_ModuleParams.AutoLoadStart - RamAddress];
			Array.Copy(Data, StaticData, _start_ModuleParams.AutoLoadStart - RamAddress);

			AutoLoadList = new List<CRT0.AutoLoadEntry>();
			uint nr = (_start_ModuleParams.AutoLoadListEnd - _start_ModuleParams.AutoLoadListOffset) / 0xC;
			uint Offset = _start_ModuleParams.AutoLoadStart - RamAddress;
			for (int i = 0; i < nr; i++)
			{
				var entry = new CRT0.AutoLoadEntry(Data, _start_ModuleParams.AutoLoadListOffset - RamAddress + (uint)i * 0xC);
				entry.Data = new byte[entry.Size];
				Array.Copy(Data, Offset, entry.Data, 0, entry.Size);
				AutoLoadList.Add(entry);
				Offset += entry.Size;
			}
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			er.Write(StaticData, 0, StaticData.Length);
			_start_ModuleParams.AutoLoadStart = (uint)er.BaseStream.Position + RamAddress;
			foreach (var v in AutoLoadList) er.Write(v.Data, 0, v.Data.Length);
			_start_ModuleParams.AutoLoadListOffset = (uint)er.BaseStream.Position + RamAddress;
			foreach (var v in AutoLoadList) v.Write(er);
			_start_ModuleParams.AutoLoadListEnd = (uint)er.BaseStream.Position + RamAddress;
			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = _start_ModuleParamsOffset;
			_start_ModuleParams.Write(er);
			er.BaseStream.Position = curpos;
			byte[] data = m.ToArray();
			er.Close();
			return data;
		}

		public void AddAutoLoadEntry(UInt32 Address, byte[] Data)
		{
			AutoLoadList.Add(new CRT0.AutoLoadEntry(Address, Data));
		}

		public bool WriteU16LE(UInt32 Address, UInt16 Value)
		{
			if (Address > RamAddress && Address < _start_ModuleParams.AutoLoadStart)
			{
				IOUtil.WriteU16LE(StaticData, (int)(Address - RamAddress), Value);
				return true;
			}
			foreach (var v in AutoLoadList)
			{
				if (Address > v.Address && Address < (v.Address + v.Size))
				{
					IOUtil.WriteU16LE(v.Data, (int)(Address - v.Address), Value);
					return true;
				}
			}
			return false;
		}

		public UInt32 ReadU32LE(UInt32 Address)
		{
			if (Address > RamAddress && Address < _start_ModuleParams.AutoLoadStart)
			{
				return IOUtil.ReadU32LE(StaticData, (int)(Address - RamAddress));
			}
			foreach (var v in AutoLoadList)
			{
				if (Address > v.Address && Address < (v.Address + v.Size))
				{
					return IOUtil.ReadU32LE(v.Data, (int)(Address - v.Address));
				}
			}
			return 0xFFFFFFFF;
		}

		public bool WriteU32LE(UInt32 Address, UInt32 Value)
		{
			if (Address > RamAddress && Address < _start_ModuleParams.AutoLoadStart)
			{
				IOUtil.WriteU32LE(StaticData, (int)(Address - RamAddress), Value);
				return true;
			}
			foreach (var v in AutoLoadList)
			{
				if (Address > v.Address && Address < (v.Address + v.Size))
				{
					IOUtil.WriteU32LE(v.Data, (int)(Address - v.Address), Value);
					return true;
				}
			}
			return false;
		}

		public static byte[] Decompress(byte[] Data)
		{
			return Decompress(Data, FindModuleParams(Data));
		}

		public static byte[] Decompress(byte[] Data, UInt32 _start_ModuleParamsOffset)
		{
			if (IOUtil.ReadU32LE(Data, (int)_start_ModuleParamsOffset + 0x14) == 0) return Data;//Not Compressed!
			byte[] Result = CRT0.MIi_UncompressBackward(Data);
			IOUtil.WriteU32LE(Data, (int)_start_ModuleParamsOffset + 0x14, 0);
			return Result;
		}

		private static uint FindModuleParams(byte[] Data)
		{
			return (uint)IndexOf(Data, new byte[] { 0x21, 0x06, 0xC0, 0xDE, 0xDE, 0xC0, 0x06, 0x21 }) - 0x1C;
		}

		private static unsafe long IndexOf(byte[] Data, byte[] Search)
		{
			fixed (byte* H = Data) fixed (byte* N = Search)
			{
				long i = 0;
				for (byte* hNext = H, hEnd = H + Data.LongLength; hNext < hEnd; i++, hNext++)
				{
					bool Found = true;
					for (byte* hInc = hNext, nInc = N, nEnd = N + Search.LongLength; Found && nInc < nEnd; Found = *nInc == *hInc, nInc++, hInc++) ;
					if (Found) return i;
				}
				return -1;
			}
		}
	}
}
