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
using LibEveryFileExplorer;
using LibEveryFileExplorer.Script;

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
			if (Arguments.Length > 0 && Arguments[0].EndsWith(".efesc"))
			{
				if (!Win32Util.AttachConsole(-1)) Win32Util.AllocConsole(); 
				AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
				PluginManager = new PluginManager();
				String Script = File.ReadAllText(Arguments[0]);
				string[] args = new string[Arguments.Length - 1];
				Array.Copy(Arguments, 1, args, 0, args.Length);
				Console.WriteLine();
				Console.WriteLine("Executing " + Path.GetFileName(Arguments[0]) + "...");
				EFEScript.Execute(Script, args);
				Win32Util.FreeConsole();
				//Using this causes scripts to be runned over and over again if opened by double-clicking
				//But without this, you need to press ENTER before be able to use the cmd again if you run it from there...
				//SendKeys.SendWait("{ENTER}");
				return;
			}

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
				foreach (var p in System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName))
				{
					if (p != System.Diagnostics.Process.GetCurrentProcess()) Win32Util.SendString(/*(IntPtr)Win32Util.HWND_BROADCAST*/p.MainWindowHandle, arg0);
				}
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
			catch (NotSupportedException e)
			{
				MessageBox.Show("Unblock " + AssemblyName.GetAssemblyName(assemblyPath) + " from external sources!");
			}
			return assembly;
		}
	}
}
