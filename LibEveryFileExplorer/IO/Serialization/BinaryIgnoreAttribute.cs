using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.IO.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class BinaryIgnoreAttribute : BinaryAttribute
	{
		readonly bool Ignore;

		public BinaryIgnoreAttribute(bool Ignore = true)
		{
			this.Ignore = Ignore;
		}

		public override object Value
		{
			get { return Ignore; }
		}
	}
}
