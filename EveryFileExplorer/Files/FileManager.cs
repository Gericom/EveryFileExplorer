using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using EveryFileExplorer.Plugins;
using System.Windows.Forms;
using LibEveryFileExplorer;

namespace EveryFileExplorer.Files
{
	public class FileManager
	{
		private List<ViewableFile> ViewedFiles = new List<ViewableFile>();

		public bool OpenFile(EFEFile File, EFEFile Parent = null)
		{
			foreach (var v in ViewedFiles)
			{
				if (v.File.Equals(File))
				{
					MessageBox.Show("This file has already been opened!");
					((Form1)Application.OpenForms[0]).BringMDIWindowToFront(v.Dialog);
					return false;
				}
			}
			Type[] formats = GetPossibleFormats(File);
			if (formats.Length == 0) return false;
			List<Type> Viewables = new List<Type>();
			foreach (Type t in formats)
			{
				if (t.GetInterfaces().Contains(typeof(IViewable))) Viewables.Add(t);
			}
			Type tt;
			if (Viewables.Count == 0) return false;
			else if (Viewables.Count == 1) tt = Viewables[0];
			else
			{
				MessageBox.Show("Multiple types match!");
				return false;
			}

			ViewableFile vv;
			try
			{
				vv = new ViewableFile(File, tt);
			}
			catch (Exception e)
			{
				MessageBox.Show("An error occured while opening the file:\n" + e);
				return false;
			}

			ViewedFiles.Add(vv);
			vv.DialogClosing += new ViewableFile.DialogClosingEventHandler(v_DialogClosing);
			vv.ShowDialog(Application.OpenForms[0]);
			if (Parent != null)
			{
				File.Parent = Parent;
				Parent.Children.Add(File);
			}

			foreach (var v in ViewedFiles)
			{
				if (vv != v && v.Dialog is IUseOtherFiles) ((IUseOtherFiles)v.Dialog).FileOpened(vv);
			}
			return true;
		}

		public bool CreateEmptyFileWithType(Type tt)
		{
			ViewableFile vv = new ViewableFile(new EFEDiskFile(), tt, true);
			ViewedFiles.Add(vv);
			vv.DialogClosing += new ViewableFile.DialogClosingEventHandler(v_DialogClosing);
			vv.ShowDialog(Application.OpenForms[0]);
			foreach (var v in ViewedFiles)
			{
				if (vv != v && v.Dialog is IUseOtherFiles) ((IUseOtherFiles)v.Dialog).FileOpened(vv);
			}
			return true;
		}

		public bool CreateFileFromFileWithType(Type tt)
		{
			ViewableFile vv = new ViewableFile(new EFEDiskFile(), tt, true);
			if (!((IFileCreatable)vv.FileFormat).CreateFromFile()) return false;
			ViewedFiles.Add(vv);
			vv.DialogClosing += new ViewableFile.DialogClosingEventHandler(v_DialogClosing);
			vv.ShowDialog(Application.OpenForms[0]);
			foreach (var v in ViewedFiles)
			{
				if (vv != v && v.Dialog is IUseOtherFiles) ((IUseOtherFiles)v.Dialog).FileOpened(vv);
			}
			return true;
		}

		bool v_DialogClosing(ViewableFile VFile)
		{
			return CloseFile(VFile);
		}

		public ViewableFile GetViewableFileFromPath(String Path)
		{
			foreach (var v in ViewedFiles)
			{
				if (v.File is EFEDiskFile)
				{
					if (((EFEDiskFile)v.File).Path == Path) return v;
				}
			}
			return null;
		}

		public ViewableFile GetViewableFileFromFile(EFEFile File)
		{
			foreach (var v in ViewedFiles)
			{
				if (v.File.Equals(File)) return v;
			}
			return null;
		}

		public ViewableFile GetViewableFileFromWindow(Form Window)
		{
			foreach (var v in ViewedFiles)
			{
				if (v.Dialog == Window) return v;
			}
			return null;
		}

		public void DisableFileDependencyDialog()
		{
			ClosingDepDisabled = Closing = true;
		}

		private bool Closing = false;
		private bool ClosingDepDisabled = false;

		public void DisableDependenceMessage()
		{
			Closing = true;
		}

		public void EnableDependenceMessage()
		{
			Closing = false;
		}

		public void CloseAllFiles()
		{
			DisableDependenceMessage();
			while (ViewedFiles.Count > 0)
			{
				ViewedFiles[0].Dialog.Close();
			}
			EnableDependenceMessage();
		}

		public bool CloseFile(ViewableFile File)
		{
			if (File.File.Children.Count != 0)
			{
				if (!Closing && MessageBox.Show("One or more files are part of this file. These files will be closed aswell.\nDo you still want to close this file?", "File Dependency", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) return false;
				while (File.File.Children.Count > 0)
				{
					Closing = true;
					GetViewableFileFromFile(File.File.Children[0]).Dialog.Close();
					Closing = false;
				}
			}
			if (File.File.Parent != null) File.File.Parent.Children.Remove(File.File);
			ViewedFiles.Remove(File);
			if (!ClosingDepDisabled)
			{
				foreach (var v in ViewedFiles)
				{
					if (v.Dialog is IUseOtherFiles) ((IUseOtherFiles)v.Dialog).FileClosed(File);
				}
			}
			return true;
		}

		public Type[] GetPossibleFormats(EFEFile File)
		{
			Dictionary<Type, FormatMatch> Formats = new Dictionary<Type, FormatMatch>();
			bool gotContent = false;
			foreach (Plugin p in Program.PluginManager.Plugins)
			{
				foreach (Type t in p.FileFormatTypes)
				{
					dynamic d = new StaticDynamic(t);
					FormatMatch m = d.Identifier.IsFormat(File);
					if (m == FormatMatch.Content) gotContent = true;
					if (m != FormatMatch.No && !(gotContent && m == FormatMatch.Extension)) Formats.Add(t, m);
				}
			}
			if (gotContent)
			{
				foreach (Type t in Formats.Keys)
				{
					if (Formats[t] == FormatMatch.Extension) Formats.Remove(t);
				}
			}
			return Formats.Keys.ToArray();
		}

		public ViewableFile[] GetOpenFilesOfType(Type FileType)
		{
			List<ViewableFile> f = new List<ViewableFile>();
			foreach (var v in ViewedFiles)
			{
				if (v.FileFormat.GetType() == FileType) f.Add(v);
			}
			return f.ToArray();
		}
	}
}
