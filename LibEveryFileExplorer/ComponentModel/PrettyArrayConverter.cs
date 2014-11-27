using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace LibEveryFileExplorer.ComponentModel
{
	public class PrettyArrayConverter : ArrayConverter
	{
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
		{
			return "";
		}
	}
}
