using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Reflection;

namespace LibEveryFileExplorer.ComponentModel
{
	public class ValueTypeTypeConverter : ExpandableObjectConverter
	{
		public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
		{
			if (propertyValues == null)
				throw new ArgumentNullException("propertyValues");

			object boxed = Activator.CreateInstance(context.PropertyDescriptor.PropertyType);
			foreach (DictionaryEntry entry in propertyValues)
			{
				PropertyInfo pi = context.PropertyDescriptor.PropertyType.GetProperty(entry.Key.ToString());
				if ((pi != null) && (pi.CanWrite))
				{
					pi.SetValue(boxed, Convert.ChangeType(entry.Value, pi.PropertyType), null);
				}
			}
			return boxed;
		}
	}
}
