using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDS.SND
{
	//Tempoarly from mkdscm
	//I need to rewrite this all
	public class ADPCM
	{
		private static int[] indexTable = 
		{ -1, -1, -1, -1, 2, 4, 6, 8,
          -1, -1, -1, -1, 2, 4, 6, 8 };

		private static int[] stepsizeTable = 
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

		public unsafe static void ConvertImaAdpcm(byte* buf, int length, byte* outbuffer)
		{
			int a = ((short*)buf)[0];
			int b = ((short*)buf)[1] & 0x7F;
			((short*)outbuffer)[0] = (short)a;
			ConvertImaAdpcm(buf + 4, length - 4, outbuffer + 2, ref a, ref b);
		}

		public unsafe static void ConvertImaAdpcm(byte* buf, int length, byte* outbuffer, ref int decompSample, ref int stepIndex)
		{
			uint destOff = 0;
			uint curOffset = 0;

			byte compByte;
			while (curOffset < length)
			{
				compByte = buf[curOffset++];
				process_nibble(compByte, ref stepIndex, ref decompSample);
				((short*)outbuffer)[destOff++] = (short)decompSample;
				process_nibble((byte)((compByte & 0xF0) >> 4), ref stepIndex, ref decompSample);
				((short*)outbuffer)[destOff++] = (short)decompSample;
			}
		}

		private static int IMAMax(int samp) { return (samp > 0x7FFF) ? ((short)0x7FFF) : samp; }
		private static int IMAMin(int samp) { return (samp < -0x7FFF) ? ((short)-0x7FFF) : samp; }
		private static int IMAIndexMinMax(int index, int min, int max) { return (index > max) ? max : ((index < min) ? min : index); }

		private static void process_nibble(byte data4bit, ref int Index, ref int Pcm16bit)
		{
			int Diff = stepsizeTable[Index] / 8;
			if ((data4bit & 1) != 0) Diff = Diff + stepsizeTable[Index] / 4;
			if ((data4bit & 2) != 0) Diff = Diff + stepsizeTable[Index] / 2;
			if ((data4bit & 4) != 0) Diff = Diff + stepsizeTable[Index] / 1;

			if ((data4bit & 8) == 0) Pcm16bit = IMAMax(Pcm16bit + Diff);
			if ((data4bit & 8) == 8) Pcm16bit = IMAMin(Pcm16bit - Diff);
			Index = IMAIndexMinMax(Index + indexTable[data4bit & 7], 0, 88);
		}

		private static void clamp_step_index(ref int stepIndex)
		{
			if (stepIndex < 0) stepIndex = 0;
			if (stepIndex > 88) stepIndex = 88;
		}

		private static void clamp_sample(ref int decompSample)
		{
			if (decompSample < -32768) decompSample = -32768;
			if (decompSample > 32767) decompSample = 32767;
		}
	}
}
