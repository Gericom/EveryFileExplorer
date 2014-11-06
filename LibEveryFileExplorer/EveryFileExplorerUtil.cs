using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using System.Runtime.InteropServices;

namespace LibEveryFileExplorer
{
	public class EveryFileExplorerUtil
	{
		public static dynamic Program = ((dynamic)new LibEveryFileExplorer.StaticDynamic(System.Reflection.Assembly.GetEntryAssembly().GetType("EveryFileExplorer.Program")));

		public static void DisableFileDependencyDialog()
		{
			Program.FileManager.DisableFileDependencyDialog();
		}

		public static String[] GetFileCategories()
		{
			List<String> s = new List<string>();
			foreach (dynamic p in Program.PluginManager.Plugins)
			{
				foreach (Type t in p.FileFormatTypes)
				{
					dynamic q = new StaticDynamic(t);
					String v = q.Identifier.GetCategory();
					if (!s.Contains(v)) s.Add(v);
				}
			}
			s.Sort();
			s.Insert(0, "Folders");
			return s.ToArray();
		}

		public static Dictionary<String, Bitmap> GetFileIcons()
		{
			Dictionary<String, Bitmap> s = new Dictionary<string,Bitmap>();
			foreach (dynamic p in Program.PluginManager.Plugins)
			{
				foreach (Type t in p.FileFormatTypes)
				{
					dynamic q = new StaticDynamic(t);
					Bitmap v = q.Identifier.GetIcon();
					if (v != null) s.Add(t.ToString(), v);
				}
			}
			return s;
		}

		public static ListViewItem GetFileItem(EFEFile File)
		{
			Type[] formats = Program.FileManager.GetPossibleFormats(File);
			ListViewItem i = new ListViewItem(File.Name);
			StringBuilder b = new StringBuilder(50);
			Win32Util.StrFormatByteSize(File.Data.Length, b, 50);
			i.SubItems.Add(b.ToString());
			if (formats.Length == 1)
			{
				i.ImageKey = formats[0].ToString();
				dynamic q = new StaticDynamic(formats[0]);
				i.Tag = q.Identifier.GetCategory();
			}
			if (File.IsCompressed) i.ForeColor = Color.Blue;
			return i;
		}

		public static void OpenFile(EFEFile File, EFEFile Parent = null)
		{
			Program.FileManager.OpenFile(File, Parent);
		}

		public static ViewableFile[] GetOpenFilesOfType(Type FileType)
		{
			return Program.FileManager.GetOpenFilesOfType(FileType);
		}

		public static ViewableFile GetViewableFileFromFile(EFEFile File)
		{
			return Program.FileManager.GetViewableFileFromFile(File);
		}
	
	}
}
