using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using EveryFileExplorer.Plugins;
using EveryFileExplorer.Files;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace EveryFileExplorer
{
	static class Program
	{
		private static Mutex mutex = new Mutex(true, "{069E262C-2440-4AFA-87AA-5CDE18753101}");

		public static PluginManager PluginManager { get; private set; }
		public static FileManager FileManager { get; private set; }
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] Arguments)
		{
			if (mutex.WaitOne(TimeSpan.Zero, true))
			{
				AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
				PluginManager = new PluginManager();
				FileManager = new FileManager();
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				if (Arguments.Length > 0) Application.Run(new Form1(Arguments[0]));
				else Application.Run(new Form1());
			}
			else
			{
				String arg0 = "";
				if (Arguments.Length > 0) arg0 = Arguments[0];
				Win32MessageHelper.SendString((IntPtr)Win32MessageHelper.HWND_BROADCAST, arg0);
			}
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Plugins\\";
			string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
			if (File.Exists(assemblyPath) == false) return null;
			try
			{
				AssemblyName.GetAssemblyName(assemblyPath);
			}
			catch (BadImageFormatException)
			{
				return null;//This is not a .net assembly
			}
			Assembly assembly = null;
			try
			{
				assembly = Assembly.LoadFrom(assemblyPath);
			}
			catch(NotSupportedException e)
			{
				MessageBox.Show("Unblock " + AssemblyName.GetAssemblyName(assemblyPath) + " from external sources!");
			}
			return assembly;
		}
	}
}
