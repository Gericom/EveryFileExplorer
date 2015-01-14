using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.IO.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class BinaryBooleanSizeAttribute : BinaryAttribute
	{
		readonly BooleanSize Size;

		public BinaryBooleanSizeAttribute(BooleanSize Size)
		{
			this.Size = Size;
		}

		public override object Value
		{
			get { return Size; }
		}
	}

	public enum BooleanSize
	{
		U8,
		U16,
		U32
	}
}
