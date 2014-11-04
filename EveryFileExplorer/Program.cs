using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using EveryFileExplorer.Plugins;
using EveryFileExplorer.Files;
using System.Reflection;
using System.IO;

namespace EveryFileExplorer
{
	static class Program
	{
		public static PluginManager PluginManager { get; private set; }
		public static FileManager FileManager { get; private set; }
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			PluginManager = new PluginManager();
			FileManager = new FileManager();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Plugins\\";
			string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
			if (File.Exists(assemblyPath) == false) return null;
			Assembly assembly = Assembly.LoadFrom(assemblyPath);
			return assembly;
		}
	}
}
