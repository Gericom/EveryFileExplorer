using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _3DS.DSP
{
	public class ADPCM
	{
		Int16[] Table;
		double Last1 = 0;
		double Last2 = 0;
		public ADPCM(Int16[] CoefTable)
		{
			Table = CoefTable;
		}

		public void UpdateLastSamples(short Prev1, short Prev2)
		{
			Last1 = Prev1;
			Last2 = Prev2;
		}

		public Int16[] GetWaveData(byte[] Data, int Offset, int Length)
		{
			List<Int16> DataOut = new List<short>();
			
			for (int i = Offset + 1; i < (Offset + Length); i += 8)
			{
				int Scale = 1 << (Data[i - 1] & 0xF);
				int Coef = (Data[i - 1] >> 4) & 0xF;
				double Coef1 = Table[Coef * 2];
				double Coef2 = Table[Coef * 2 + 1];

				for (int j = 0; j < 7; j++)
				{
					int high = Data[i + j] >> 4;
					int low = Data[i + j] & 0xF;
					if (high >= 8) high -= 16;
					if (low >= 8) low -= 16;
					double val = (((high * Scale) << 11) + 1024.0 + (Coef1 * Last1 + Coef2 * Last2)) / 2048.0; //>> 11;
					short samp = Clamp((int)val, short.MinValue, short.MaxValue);
					DataOut.Add(samp);
					Last2 = Last1;
					Last1 = val;
					val = (((low * Scale) << 11) + 1024.0 + (Coef1 * Last1 + Coef2 * Last2)) / 2048.0;//>> 11;
					samp = Clamp((int)val, short.MinValue, short.MaxValue);
					DataOut.Add(samp);
					Last2 = Last1;
					Last1 = val;
				}
			}
			return DataOut.ToArray();
		}

		private static short Clamp(int value, int min, int max)
		{
			if (value < min) value = min;
			if (value > max) value = max;
			return (short)value;
		}
	}
}
