using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Math
{
	public class MathUtil
	{
		public static int Clamp(int value, int min, int max)
		{
			if (value < min) value = min;
			if (value > max) value = max;
			return (short)value;
		}

		public static float RadToDeg(float Radians)
		{
			return Radians * (180f / (float)System.Math.PI);
		}

		public static double RadToDeg(double Radians)
		{
			return Radians * (180.0 / System.Math.PI);
		}
	}
}
