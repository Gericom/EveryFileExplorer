using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.IO.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class BinaryFixedPointAttribute : BinaryAttribute
	{
		readonly uint Format;

		public BinaryFixedPointAttribute(bool Sign, int IntPart, int FracPart)
		{
			if(IntPart < 0 || FracPart < 0) throw new ArgumentException("IntPart and FracPart shoulf be greater or equal to 0!");
			if (IntPart + FracPart + (Sign ? 1 : 0) > 64 || IntPart + FracPart + (Sign ? 1 : 0) == 0) throw new ArgumentException("Total number of bits should be greater than 0 and smaller or equal to 64!");
			this.Format = (uint)(Sign ? 1 : 0) << 14 | (uint)(IntPart & 0x7F) << 7 | (uint)(FracPart & 0x7F);
		}

		public override object Value
		{
			get { return Format; }
		}
	}
}
