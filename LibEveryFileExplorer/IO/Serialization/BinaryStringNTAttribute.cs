using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.IO.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class BinaryStringNTAttribute : BinaryAttribute
	{
		readonly bool NullTerminated;

		public BinaryStringNTAttribute(bool NullTerminated = true)
		{
			this.NullTerminated = NullTerminated;
		}

		public override object Value
		{
			get { return NullTerminated; }
		}
	}
}
