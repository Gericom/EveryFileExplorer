using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.SND
{
	public class SNDUtil
	{
		public static byte[] InterleaveChannels(params Int16[][] Channels)
		{
			if(Channels.Length == 0) return new byte[0];
			byte[] Result = new byte[Channels[0].Length * Channels.Length * 2];
			for (int i = 0; i < Channels[0].Length; i++)
			{
				for (int j = 0; j < Channels.Length; j++)
				{
					Result[i * 2 * Channels.Length + j * 2] = (byte)(Channels[j][i] & 0xFF);
					Result[i * 2 * Channels.Length + j * 2 + 1] = (byte)(Channels[j][i] >> 8);
				}
			}
			return Result;
		}
	}
}
