using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.IO.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class BinaryStringEncodingAttribute : BinaryAttribute
	{
		readonly Encoding Type;

		public BinaryStringEncodingAttribute(Encoding Type)
		{
			this.Type = Type;
		}

		public override object Value
		{
			get { return Type; }
		}
	}
}
