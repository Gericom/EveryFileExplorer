using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer;

namespace _3DS.UI
{
	public partial class SARCViewer : Form, IChildReactive
	{
		SARC Archive;
		SFSDirectory Root;
		public SARCViewer(SARC Archive)
		{
			this.Archive = Archive;
			Root = Archive.ToFileSystem();
			InitializeComponent();
		}

		private void SARCViewer_Load(object sender, EventArgs e)
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
			EveryFileExplorerUtil.OpenFile(new SARCEFESFSFile(s), ((ViewableFile)Tag).File);
		}

		private void OnExport(object sender, EventArgs e)
		{
			var file = Root.GetFileByPath(fileBrowser1.SelectedPath);
			if (file == null) return;
			saveFileDialog1.Filter = "Binary File (*.bin)|*.bin|All Files (*.*)|*.*";//System.IO.Path.GetExtension(fileBrowser1.SelectedPath).Replace(".", "").ToUpper() + " Files (*" + System.IO.Path.GetExtension(fileBrowser1.SelectedPath).ToLower() + ")|*" + System.IO.Path.GetExtension(fileBrowser1.SelectedPath).ToLower() + "|All Files (*.*)|*.*";
			saveFileDialog1.FileName = System.IO.Path.GetFileName(fileBrowser1.SelectedPath);
			if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& saveFileDialog1.FileName.Length > 0)
			{
				System.IO.File.Create(saveFileDialog1.FileName).Close();
				System.IO.File.WriteAllBytes(saveFileDialog1.FileName, file.Data);
			}
		}

		private void fileBrowser1_OnSelectionChanged(object sender, EventArgs e)
		{
			if (Root.GetDirectoryByPath(fileBrowser1.SelectedPath) == null) menuExport.Enabled = menuReplace.Enabled = true;
			else menuExport.Enabled = menuReplace.Enabled = false;
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

		private void menuExportDir_Click(object sender, EventArgs e)
		{
			var dir = Root.GetDirectoryByPath(fileBrowser1.SelectedFolderPath);
			if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& folderBrowserDialog1.SelectedPath.Length > 0)
			{
				dir.Export(folderBrowserDialog1.SelectedPath);
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
