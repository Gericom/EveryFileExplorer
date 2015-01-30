using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace LibEveryFileExplorer.ComponentModel
{
	public class HexTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string)) return true;
			else return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string)) return true;
			else return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType != typeof(string)) return base.ConvertTo(context, culture, value, destinationType);
			bool reverse = false;

			PropertyDescriptor p = context.PropertyDescriptor;
			if (context.PropertyDescriptor.GetType().Name == "MergePropertyDescriptor")
			{
				System.Type propertyType = context.PropertyDescriptor.GetType();
				System.Reflection.FieldInfo fieldInfo = propertyType.GetField(
					"descriptors",
					System.Reflection.BindingFlags.NonPublic
					| System.Reflection.BindingFlags.Instance
				);
				PropertyDescriptor[] descriptors =
					(PropertyDescriptor[])(fieldInfo.GetValue(context.PropertyDescriptor));
				p = descriptors[0];
			}

			foreach (var v in p.Attributes)
			{
				if (v is HexReversedAttribute) reverse = ((HexReversedAttribute)v).HexReversed;
			}
			if (value.GetType() == typeof(UInt16))
			{
				UInt16 val = (UInt16)value;
				if (reverse) val = (UInt16)((((val >> 0) & 0xFF) << 8) | (((val >> 8) & 0xFF) << 0));
				string t = string.Format("{0:X4}", val);
				for (int i = 0; i < 4 - t.Length; i++)
				{
					t += "0";
				}
				return t;
			}
			else if (value.GetType() == typeof(UInt32))
			{
				UInt32 val = (UInt32)value;
				if (reverse) val = (UInt32)((((val >> 0) & 0xFF) << 24) | (((val >> 8) & 0xFF) << 16) | (((val >> 16) & 0xFF) << 8) | (((val >> 24) & 0xFF) << 0));
				string t = string.Format("{0:X8}", val);
				for (int i = 0; i < 8 - t.Length; i++)
				{
					t += "0";
				}
				return t;
			}
			else return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value.GetType() == typeof(string))
			{
				string input = (string)value;
				if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) input = input.Substring(2);

				PropertyDescriptor p = context.PropertyDescriptor;
				if (context.PropertyDescriptor.GetType().Name == "MergePropertyDescriptor")
				{
					System.Type propertyType = context.PropertyDescriptor.GetType();
					System.Reflection.FieldInfo fieldInfo = propertyType.GetField(
						"descriptors",
						System.Reflection.BindingFlags.NonPublic
						| System.Reflection.BindingFlags.Instance
					);
					PropertyDescriptor[] descriptors =
						(PropertyDescriptor[])(fieldInfo.GetValue(context.PropertyDescriptor));
					p = descriptors[0];
				}

				bool reverse = false;
				foreach (var v in p.Attributes)
				{
					if (v is HexReversedAttribute) reverse = ((HexReversedAttribute)v).HexReversed;
				}

				if (context.PropertyDescriptor.PropertyType == typeof(UInt16))
				{
					ushort val = UInt16.Parse(input, System.Globalization.NumberStyles.HexNumber, culture);
					if (reverse) val = (UInt16)((((val >> 0) & 0xFF) << 8) | (((val >> 8) & 0xFF) << 0));
					return val;
				}
				else if (context.PropertyDescriptor.PropertyType == typeof(UInt32))
				{
					uint val = UInt32.Parse(input, System.Globalization.NumberStyles.HexNumber, culture);
					if (reverse) val = (UInt32)((((val >> 0) & 0xFF) << 24) | (((val >> 8) & 0xFF) << 16) | (((val >> 16) & 0xFF) << 8) | (((val >> 24) & 0xFF) << 0));
					return val;
				}
				else return base.ConvertFrom(context, culture, value);
			}
			else return base.ConvertFrom(context, culture, value);
		}
	}

}
