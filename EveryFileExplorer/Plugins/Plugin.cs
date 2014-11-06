using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.Compression;

namespace EveryFileExplorer.Plugins
{
	public class Plugin
	{
		public String Name;
		public String Description;
		public String Version;
		public Type[] CompressionTypes;
		public Type[] FileFormatTypes;
		public Type[] ProjectTypes;

		public Plugin(Assembly Assembly)
		{
			Name = Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false).OfType<AssemblyTitleAttribute>().FirstOrDefault().Title;
			Description = Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).OfType<AssemblyDescriptionAttribute>().FirstOrDefault().Description;
			try
			{
				Version = Assembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), false).OfType<AssemblyVersionAttribute>().FirstOrDefault().Version;
			}
			catch
			{
				Version = null;
			}
			Type[] tt = Assembly.GetExportedTypes();
			List<Type> fe = new List<Type>();
			List<Type> ce = new List<Type>();
			List<Type> pe = new List<Type>();
			foreach (var t in tt)
			{
				if (!t.IsClass) continue;
				var v = t.GetInterfaces();
				if (v.Length != 0 && t.GetInterfaces()[0].Name == "FileFormatBase") fe.Add(t);
				else if (v.Length != 0 && t.GetInterfaces()[0].Name == "CompressionFormatBase") ce.Add(t);
				else if (v.Length != 0 && t.GetInterfaces()[0].Name == "ProjectBase") pe.Add(t);
			}
			FileFormatTypes = fe.ToArray();
			CompressionTypes = ce.ToArray();
			ProjectTypes = pe.ToArray();
		}

		public static bool IsPlugin(Assembly Assembly)
		{
			Type[] tt = Assembly.GetExportedTypes();
			foreach (var t in tt)
			{
				if (!t.IsClass) continue;
				var v = t.GetInterfaces();
				if (v.Length != 0 && (t.GetInterfaces()[0].Name == "FileFormatBase" || t.GetInterfaces()[0].Name == "CompressionFormatBase" || t.GetInterfaces()[0].Name == "ProjectBase")) return true;
			}
			return false;
		}
	}

}
