using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer
{
	public class HexUtil
	{
		public static String GetHexReverse(sbyte Value)
		{
			return (Value & 0xFF).ToString("X2");
		}
		public static String GetHexReverse(byte Value)
		{
			return Value.ToString("X2");
		}
		public static String GetHexReverse(short Value)
		{
			return BitConverter.ToString(BitConverter.GetBytes(Value)).Replace("-", "");
		}
		public static String GetHexReverse(ushort Value)
		{
			return BitConverter.ToString(BitConverter.GetBytes(Value)).Replace("-", "");
		}
		public static String GetHexReverse(int Value)
		{
			return BitConverter.ToString(BitConverter.GetBytes(Value)).Replace("-", "");
		}
		public static String GetHexReverse(uint Value)
		{
			return BitConverter.ToString(BitConverter.GetBytes(Value)).Replace("-", "");
		}
	}
}
