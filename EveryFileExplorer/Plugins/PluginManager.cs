using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace EveryFileExplorer.Plugins
{
	public class PluginManager
	{
		public Plugin[] Plugins { get; private set; }
		public PluginManager()
		{
			string[] d = Directory.GetFiles(Path.GetDirectoryName(Application.ExecutablePath) + "\\Plugins\\", "*.dll", SearchOption.TopDirectoryOnly);
			List<Plugin> p = new List<Plugin>();
			foreach (var s in d)
			{
				try
				{
					AssemblyName.GetAssemblyName(s);
				}
				catch (BadImageFormatException)
				{
					continue;//This is not a .net assembly
				}
				Assembly ass = null;
				try
				{
					ass = Assembly.LoadFile(s);
				}
				catch(NotSupportedException e)
				{
					MessageBox.Show("Unblock " + AssemblyName.GetAssemblyName(s) + " from external sources!");
				}
				if (Plugin.IsPlugin(ass)) p.Add(new Plugin(ass));
			}
			Plugins = p.ToArray();
		}
	}
}
