using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.ComponentModel
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class HexReversedAttribute : Attribute
	{
		readonly bool hexReversed;

		// This is a positional argument
		public HexReversedAttribute(bool hexReversed = true)
		{
			this.hexReversed = hexReversed;
		}

		public bool HexReversed
		{
			get { return hexReversed; }
		}
	}
}
