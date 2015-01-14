using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.IO.Serialization
{
	public abstract class BinaryAttribute : Attribute
	{
		public abstract Object Value { get; }
	}
}
