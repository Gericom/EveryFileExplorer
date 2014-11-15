using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using LibEveryFileExplorer;
using LibEveryFileExplorer.Files;
using NDS.NitroSystem.FND;

namespace NDS.UI
{
	public partial class UtilityBinViewer : Form, IChildReactive
	{
		UtilityBin Archive;
		SFSDirectory Root;
		public UtilityBinViewer(UtilityBin Archive)
		{
			this.Archive = Archive;
			Root = Archive.ToFileSystem();
			InitializeComponent();
		}

		private void NARCViewer_Load(object sender, EventArgs e)
		{
			fileBrowser1.UpdateDirectories(Root.GetTreeNodes());
		}

		private void fileBrowser1_OnDirectoryChanged(string Path)
		{
			var d = Root.GetDirectoryByPath(Path);
			fileBrowser1.UpdateContent(d.GetContent());
		}

		private void fileBrowser1_OnFileActivated(string Path)
		{
			var s = Root.GetFileByPath(Path);
			EveryFileExplorerUtil.OpenFile(new EFESFSFile(s), ((ViewableFile)Tag).File);
		}

		private void fileBrowser1_OnAddDirectory(object sender, EventArgs e)
		{
			var dir = Root.GetDirectoryByPath(fileBrowser1.SelectedFolderPath);
			if (dir == null) return;
			String name = null;
		retry:
			name = Microsoft.VisualBasic.Interaction.InputBox("Please give the name of the new directory:", "New Directory", name);
			if (name == null || name.Length == 0) return;
			if (dir.IsValidName(name))
			{
				SFSDirectory d = new SFSDirectory(name, false);
				d.Parent = dir;
				dir.SubDirectories.Add(d);
				Archive.FromFileSystem(Root);
				fileBrowser1.UpdateDirectories(Root.GetTreeNodes(), true);
			}
			else
			{
				MessageBox.Show("The name contains either one or more invalid chars, or is already in use!", "Invalid Name");
				goto retry;
			}
		}

		private void fileBrowser1_OnAddFile(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& openFileDialog1.FileName.Length > 0)
			{
				var dir = Root.GetDirectoryByPath(fileBrowser1.SelectedFolderPath);
				if (!dir.IsValidName(System.IO.Path.GetFileName(openFileDialog1.FileName)))
				{
					switch (MessageBox.Show("A file with the same name as the selected file exists. Do you want to replace this file?", "Replace", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					{
						case System.Windows.Forms.DialogResult.Yes:
							var file = Root.GetFileByPath(fileBrowser1.SelectedFolderPath + "/" + System.IO.Path.GetFileName(openFileDialog1.FileName));
							file.Data = System.IO.File.ReadAllBytes(openFileDialog1.FileName);
							break;
						case System.Windows.Forms.DialogResult.No:
							int nr = 0;
							String NewName;
							do
							{
								NewName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog1.FileName) + "_" + nr++ + System.IO.Path.GetExtension(openFileDialog1.FileName);
							}
							while (!dir.IsValidName(NewName));
							SFSFile f = new SFSFile(-1, NewName, dir);
							f.Data = System.IO.File.ReadAllBytes(openFileDialog1.FileName);
							dir.Files.Add(f);
							break;
					}
				}
				else
				{
					SFSFile f = new SFSFile(-1, System.IO.Path.GetFileName(openFileDialog1.FileName), dir);
					f.Data = System.IO.File.ReadAllBytes(openFileDialog1.FileName);
					dir.Files.Add(f);
				}
				Archive.FromFileSystem(Root);
				fileBrowser1.UpdateDirectories(Root.GetTreeNodes(), true);
			}
		}

		private void fileBrowser1_OnRemove(object sender, EventArgs e)
		{
			if (fileBrowser1.SelectedFolderPath == fileBrowser1.SelectedPath) return;
			var dir = Root.GetDirectoryByPath(fileBrowser1.SelectedPath);
			if (dir != null)
			{
				if (MessageBox.Show("Are you sure you permanently want to delete this directory: " + dir.DirectoryName + ", and all of it's files and subfolders?", "Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.No) return;
				dir.Parent.SubDirectories.Remove(dir);
				Archive.FromFileSystem(Root);
				fileBrowser1.UpdateDirectories(Root.GetTreeNodes(), true);
			}
			else
			{
				var file = Root.GetFileByPath(fileBrowser1.SelectedPath);
				if (MessageBox.Show("Are you sure you permanently want to delete this file: " + file.FileName + "?", "Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.No) return;
				file.Parent.Files.Remove(file);
				Archive.FromFileSystem(Root);
				fileBrowser1.UpdateDirectories(Root.GetTreeNodes(), true);
			}
		}

		private void fileBrowser1_OnRename(object sender, EventArgs e)
		{
			if (fileBrowser1.SelectedFolderPath == fileBrowser1.SelectedPath) return;
			var dir = Root.GetDirectoryByPath(fileBrowser1.SelectedPath);
			if (dir != null)
			{
				String name = dir.DirectoryName;
			retryd:
				name = Microsoft.VisualBasic.Interaction.InputBox("Please give the new name for this directory:", "Rename", name);
				if (name == null || name.Length == 0) return;
				if (dir.Parent.IsValidName(name))
				{
					String oldpath = dir.ToString();
					dir.DirectoryName = name;
					String newpath = dir.ToString();
					Archive.FromFileSystem(Root);
					fileBrowser1.UpdateDirectories(Root.GetTreeNodes(), true, oldpath.Remove(oldpath.Length - 1), newpath.Remove(newpath.Length - 1));
				}
				else
				{
					MessageBox.Show("The name contains either one or more invalid chars, or is already in use!", "Invalid Name");
					goto retryd;
				}
			}
			else//file
			{
				var file = Root.GetFileByPath(fileBrowser1.SelectedPath);
				String name = file.FileName;
			retryf:
				name = Microsoft.VisualBasic.Interaction.InputBox("Please give the new name for this file:", "Rename", name);
				if (name == null || name.Length == 0) return;
				if (file.Parent.IsValidName(name))
				{
					file.FileName = name;
					Archive.FromFileSystem(Root);
					fileBrowser1.UpdateDirectories(Root.GetTreeNodes(), true);
				}
				else
				{
					MessageBox.Show("The name contains either one or more invalid chars, or is already in use!", "Invalid Name");
					goto retryf;
				}
			}
		}

		private void fileBrowser1_OnSelectionChanged(object sender, EventArgs e)
		{
			fileBrowser1.RenameEnabled = fileBrowser1.DeleteEnabled = menuRename.Enabled = menuDelete.Enabled = !(fileBrowser1.SelectedPath == fileBrowser1.SelectedFolderPath);
			if (Root.GetDirectoryByPath(fileBrowser1.SelectedPath) == null) menuExport.Enabled = menuReplace.Enabled = true;
			else menuExport.Enabled = menuReplace.Enabled = false;
		}

		private void menuItem7_Click(object sender, EventArgs e)
		{
			var dir = Root.GetDirectoryByPath(fileBrowser1.SelectedFolderPath);
			if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& folderBrowserDialog1.SelectedPath.Length > 0)
			{
				dir.Export(folderBrowserDialog1.SelectedPath);
			}
		}

		private void fileBrowser1_OnRightClick(Point Location)
		{
			var dir = Root.GetDirectoryByPath(fileBrowser1.SelectedPath);
			if (dir != null)
			{

			}
			else
			{
				//var file = Root.GetFileByPath(fileBrowser1.SelectedPath);
				contextMenu1.Show(fileBrowser1, Location);
			}
		}

		private void menuExport_Click(object sender, EventArgs e)
		{
			var file = Root.GetFileByPath(fileBrowser1.SelectedPath);
			if (file == null) return;
			saveFileDialog1.Filter = System.IO.Path.GetExtension(fileBrowser1.SelectedPath).Replace(".", "").ToUpper() + " Files (*" + System.IO.Path.GetExtension(fileBrowser1.SelectedPath).ToLower() + ")|*" + System.IO.Path.GetExtension(fileBrowser1.SelectedPath).ToLower() + "|All Files (*.*)|*.*";
			saveFileDialog1.FileName = System.IO.Path.GetFileName(fileBrowser1.SelectedPath);
			if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& saveFileDialog1.FileName.Length > 0)
			{
				System.IO.File.Create(saveFileDialog1.FileName).Close();
				System.IO.File.WriteAllBytes(saveFileDialog1.FileName, file.Data);
			}
		}

		private void menuReplace_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				   && openFileDialog1.FileName.Length > 0)
			{
				var file = Root.GetFileByPath(fileBrowser1.SelectedPath);
				file.Data = System.IO.File.ReadAllBytes(openFileDialog1.FileName);
				Archive.FromFileSystem(Root);
				fileBrowser1.UpdateDirectories(Root.GetTreeNodes(), true);
			}
		}

		public void OnChildSave(ViewableFile File)
		{
			Archive.FromFileSystem(Root);
			fileBrowser1.UpdateDirectories(Root.GetTreeNodes(), true);
		}
	}
}
