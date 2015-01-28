using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.Math;

namespace NDS.SND
{
	public class IMAADPCMEncoder
	{
		public IMAADPCMEncoder() { }

		private bool IsInit = false;
		private int Last;
		private int Index;

		public byte[] Encode(Int16[] WaveData)
		{
			List<byte> Result = new List<byte>();
			int Offset = 0;
			if (!IsInit)
			{
				Last = WaveData[0];
				Index = GetBestTableIndex((WaveData[1] - WaveData[0]) * 8);
				byte[] Header = new byte[4];
				IOUtil.WriteS16LE(Header, 0, WaveData[0]);
				IOUtil.WriteS16LE(Header, 2, (short)Index);
				Result.AddRange(Header);
				Offset++;
				IsInit = true;
			}
			byte[] Nibbles = new byte[WaveData.Length - Offset];//nibbles, lets merge it afterwards
			for (int i = Offset; i < WaveData.Length; i++)
			{
				int val = GetBestConfig(Index, WaveData[i] - Last);
				Nibbles[i - Offset] = (byte)val;

				int diff =
						IMAADPCMConst.StepTable[Index] / 8 +
						IMAADPCMConst.StepTable[Index] / 4 * ((val >> 0) & 1) +
						IMAADPCMConst.StepTable[Index] / 2 * ((val >> 1) & 1) +
						IMAADPCMConst.StepTable[Index] * ((val >> 2) & 1);

				int samp = Last + diff * ((((val >> 3) & 1) == 1) ? -1 : 1);
				Last = (short)MathUtil.Clamp(samp, short.MinValue, short.MaxValue);
				Index = (short)MathUtil.Clamp(Index + IMAADPCMConst.IndexTable[val & 7], 0, 88);
			}
			for (int i = 0; i < Nibbles.Length; i += 2)
			{
				if (i == Nibbles.Length - 1)
				{
					Result.Add((byte)(Nibbles[i]));
				}
				else Result.Add((byte)(Nibbles[i] | (Nibbles[i + 1] << 4)));
			}
			return Result.ToArray();
		}

		private int GetBestTableIndex(int Diff)
		{
			int LowestDiff = int.MaxValue;
			int LowestIdx = -1;
			for (int i = 0; i < IMAADPCMConst.StepTable.Length; i++)
			{
				int diff2 = Math.Abs(Math.Abs(Diff) - IMAADPCMConst.StepTable[i]);
				if (diff2 < LowestDiff)
				{
					LowestDiff = diff2;
					LowestIdx = i;
				}
			}
			return LowestIdx;
		}

		private int GetBestConfig(int Index, int Diff)
		{
			int Result = 0;
			if (Diff < 0) Result |= 1 << 3;
			Diff = Math.Abs(Diff);
			int DiffNew = IMAADPCMConst.StepTable[Index] / 8;

			if (Math.Abs(DiffNew - Diff) >= IMAADPCMConst.StepTable[Index])
			{
				Result |= 1 << 2;
				DiffNew += IMAADPCMConst.StepTable[Index];
			}

			if (Math.Abs(DiffNew - Diff) >= IMAADPCMConst.StepTable[Index] / 2)
			{
				Result |= 1 << 1;
				DiffNew += IMAADPCMConst.StepTable[Index] / 2;
			}

			if (Math.Abs(DiffNew - Diff) >= IMAADPCMConst.StepTable[Index] / 4)
			{
				Result |= 1;
				DiffNew += IMAADPCMConst.StepTable[Index] / 4;
			}
			return Result;
		}
	}
}
