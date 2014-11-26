using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace LibEveryFileExplorer.ComponentModel
{
	public class UInt16HexTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			else
			{
				return base.CanConvertFrom(context, sourceType);
			}
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return true;
			}
			else
			{
				return base.CanConvertTo(context, destinationType);
			}
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string) && value.GetType() == typeof(UInt16))
			{
				string t = string.Format("{0:X4}", value);
				for (int i = 0; i < 4 - t.Length; i++)
				{
					t += "0";
				}
				return t;
			}
			else
			{
				return base.ConvertTo(context, culture, value, destinationType);
			}
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value.GetType() == typeof(string))
			{
				string input = (string)value;

				if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				{
					input = input.Substring(2);
				}

				return UInt16.Parse(input, System.Globalization.NumberStyles.HexNumber, culture);
			}
			else
			{
				return base.ConvertFrom(context, culture, value);
			}
		}
	}

}
