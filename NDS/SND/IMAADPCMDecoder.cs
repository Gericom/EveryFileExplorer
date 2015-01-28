using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.Math;

namespace NDS.SND
{
	public class IMAADPCMDecoder
	{
		private bool IsInit = false;
		private int Last;
		private int Index;

		public IMAADPCMDecoder() { }

		public Int16[] GetWaveData(byte[] Data, int Offset, int Length)
		{
			List<Int16> DataOut = new List<short>();
			if (!IsInit)
			{
				Last = IOUtil.ReadS16LE(Data, Offset);
				Index = IOUtil.ReadS16LE(Data, Offset + 2) & 0x7F;
				Offset += 4;
				Length -= 4;
				DataOut.Add((short)Last);
				IsInit = true;
			}
			int end = Offset + Length;
			while (Offset < end)
			{
				byte sampd = Data[Offset++];
				for (int i = 0; i < 2; i++)
				{
					int val = (sampd >> (i * 4)) & 0xF;

					int diff =
						IMAADPCMConst.StepTable[Index] / 8 +
						IMAADPCMConst.StepTable[Index] / 4 * ((val >> 0) & 1) +
						IMAADPCMConst.StepTable[Index] / 2 * ((val >> 1) & 1) +
						IMAADPCMConst.StepTable[Index] * ((val >> 2) & 1);

					int samp = Last + diff * ((((val >> 3) & 1) == 1) ? -1 : 1);
					Last = (short)MathUtil.Clamp(samp, short.MinValue, short.MaxValue);
					Index = (short)MathUtil.Clamp(Index + IMAADPCMConst.IndexTable[val & 7], 0, 88);
					DataOut.Add((short)Last);
				}
			}
			return DataOut.ToArray();
		}
	}
}
