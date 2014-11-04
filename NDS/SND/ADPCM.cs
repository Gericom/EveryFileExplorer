using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.Math;

namespace NDS.SND
{
	public class ADPCM
	{
		private static int[] IndexTable = 
		{ -1, -1, -1, -1, 2, 4, 6, 8,
          -1, -1, -1, -1, 2, 4, 6, 8 };

		private static int[] StepTable = 
		{ 7, 8, 9, 10, 11, 12, 13, 14,
		  16, 17, 19, 21, 23, 25, 28,
		  31, 34, 37, 41, 45, 50, 55,
	      60, 66, 73, 80, 88, 97, 107,
	      118, 130, 143, 157, 173, 190, 209,
	      230, 253, 279, 307, 337, 371, 408,
	      449, 494, 544, 598, 658, 724, 796,
	      876, 963, 1060, 1166, 1282, 1411, 1552,
	      1707, 1878, 2066, 2272, 2499, 2749, 3024, 3327, 3660, 4026,
	      4428, 4871, 5358, 5894, 6484, 7132, 7845, 8630,
	      9493, 10442, 11487, 12635, 13899, 15289, 16818,
	      18500, 20350, 22385, 24623, 27086, 29794, 32767 };

		private bool IsInit = false;
		private int Last;
		private int Index;

		public ADPCM() { }

		public Int16[] GetWaveData(byte[] Data, int Offset, int Length)
		{
			List<Int16> DataOut = new List<short>();
			if (!IsInit)
			{
				Last = IOUtil.ReadS16LE(Data, Offset);
				Index = IOUtil.ReadS16LE(Data, Offset + 2) & 0x7F;
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
						StepTable[Index] / 8 +
						StepTable[Index] / 4 * ((val >> 0) & 1) +
						StepTable[Index] / 2 * ((val >> 1) & 1) +
						StepTable[Index] * ((val >> 2) & 1);

					int samp = Last + diff * ((((val >> 3) & 1) == 1) ? -1 : 1);
					Last = (short)MathUtil.Clamp(samp, short.MinValue, short.MaxValue);
					Index = (short)MathUtil.Clamp(Index + IndexTable[val & 7], 0, 88);
					DataOut.Add((short)Last);
				}
			}
			return DataOut.ToArray();
		}
	}
}
