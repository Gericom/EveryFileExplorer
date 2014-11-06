using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections;

namespace LibEveryFileExplorer.UI
{
	public partial class FileBrowser : UserControl
	{
		ImageList images;
		ImageList images2;

		public FileBrowser()
		{
			InitializeComponent();
			Win32Util.SetWindowTheme(treeView1.Handle, "explorer", null);
			Win32Util.SetWindowTheme(listView1.Handle, "explorer", null);
		}

		private void FileBrowser_Load(object sender, EventArgs e)
		{
			if (!DesignMode)
			{
				images = new ImageList();
				images.ColorDepth = ColorDepth.Depth32Bit;
				images.ImageSize = new System.Drawing.Size(32, 32);
				var g = EveryFileExplorerUtil.GetFileCategories();
				foreach (String s in g)
				{
					listView1.Groups.Add(s, s);
				}
				var g2 = EveryFileExplorerUtil.GetFileIcons();
				foreach (var q in g2.Keys)
				{
					images.Images.Add(q, g2[q]);
				}
				images.Images.Add("Folder", Resource.folder);
				listView1.LargeImageList = images;
				images2 = new ImageList();
				images2.ColorDepth = ColorDepth.Depth32Bit;
				images2.ImageSize = new System.Drawing.Size(16, 16);
				images2.Images.Add(Resource.folder_open);
				treeView1.ImageList = images2;
				treeView1.ImageIndex = 0;
			}
		}

		public delegate void OnDirectoryChangedEventHandler(String Path);
		public event OnDirectoryChangedEventHandler OnDirectoryChanged;

		public delegate void OnFileActivatedEventHandler(String Path);
		public event OnFileActivatedEventHandler OnFileActivated;

		[Browsable(false)]
		public String SelectedFolderPath
		{
			get
			{
				return treeView1.SelectedNode.FullPath;
			}
		}

		[Browsable(false)]
		public String SelectedPath
		{
			get
			{
				String basep = treeView1.SelectedNode.FullPath;
				if (listView1.SelectedItems.Count != 0) basep += "/" + listView1.SelectedItems[0].Text;
				return basep;
			}
		}

		public bool ShowAddDirectoryButton
		{
			get { return buttonDirAdd.Visible; }
			set
			{
				buttonDirAdd.Visible = value;
				FixSeparators();
			}
		}

		public bool ShowAddFileButton
		{
			get { return buttonFileAdd.Visible; }
			set
			{
				buttonFileAdd.Visible = value;
				FixSeparators();
			}
		}

		public bool ShowDeleteButton
		{
			get { return buttonDelete.Visible; }
			set
			{
				buttonDelete.Visible = value;
				FixSeparators();
			}
		}

		public bool ShowRenameButton
		{
			get { return buttonRename.Visible; }
			set
			{
				buttonRename.Visible = value;
				FixSeparators();
			}
		}

		public bool DeleteEnabled
		{
			get { return buttonDelete.Enabled; }
			set { buttonDelete.Enabled = value; }
		}

		public bool RenameEnabled
		{
			get { return buttonRename.Enabled; }
			set { buttonRename.Enabled = value; }
		}

		private void FixSeparators()
		{
			toolStripSeparator1.Visible = ((ShowAddDirectoryButton || ShowAddFileButton) && ShowDeleteButton) || ((ShowAddDirectoryButton || ShowAddFileButton) && ShowRenameButton);
			toolStripSeparator2.Visible = ShowDeleteButton && ShowRenameButton;
		}

		public void UpdateDirectories(TreeNode Root)
		{
			UpdateDirectories(Root, false, null, null);
		}

		public void UpdateDirectories(TreeNode Root, bool TryKeepCurrentState)
		{
			UpdateDirectories(Root, TryKeepCurrentState, null, null);
		}

		public void UpdateDirectories(TreeNode Root, bool TryKeepCurrentState, String OldNamePath, String NewNamePath)
		{
			if (TryKeepCurrentState)
			{
				String curpath = SelectedFolderPath;
				TreeNode[] old = new TreeNode[treeView1.Nodes.Count];
				treeView1.Nodes.CopyTo(old, 0);
				if (OldNamePath != null && NewNamePath != null)
				{
					if (OldNamePath.StartsWith("//")) OldNamePath = " " + OldNamePath.Substring(1);
					if(NewNamePath.StartsWith("//")) NewNamePath = " " + NewNamePath.Substring(1);
					string oldpath = OldNamePath;
					string newpath = NewNamePath;

					string[] parts = oldpath.Split('/');
					string[] parts2 = newpath.Split('/');
					if (parts.Length == parts2.Length)
					{
						TreeNode CurNode = old[0];
						int partidx = 1;
						while (partidx < parts.Length)
						{
							foreach (TreeNode t in CurNode.Nodes)
							{
								if (t.Text == parts[partidx])
								{
									CurNode = t;
									partidx++;
									break;
								}
							}
						}
						if (CurNode.Text == parts[partidx - 1]) CurNode.Text = parts2[partidx - 1];
					}
				}
				treeView1.BeginUpdate();
				treeView1.Nodes.Clear();
				treeView1.Nodes.Add(Root);
				SetExpandState(treeView1.Nodes, (IList)new List<TreeNode>(old));
				{
					string path = " " + curpath.Substring(1);
					string[] parts = path.Split('/');
					TreeNode CurNode = treeView1.Nodes[0];
					int partidx = 1;
					while (partidx < parts.Length)
					{
						foreach (TreeNode t in CurNode.Nodes)
						{
							if (t.Text == parts[partidx])
							{
								CurNode = t;
								partidx++;
								break;
							}
						}
					}

					treeView1.SelectedNode = CurNode;
				}
				treeView1.EndUpdate();
			}
			else
			{
				treeView1.BeginUpdate();
				treeView1.Nodes.Clear();
				treeView1.Nodes.Add(Root);
				Root.Expand();
				treeView1.SelectedNode = Root;
				treeView1.EndUpdate();
			}
		}

		public void SetExpandState(IList Nodes, IList ExpandState)
		{
			foreach (TreeNode t in Nodes)
			{
				foreach (TreeNode t2 in ExpandState)
				{
					if (t2.Text == t.Text)
					{
						if (t2.IsExpanded) t.Expand();
						else t.Collapse();
						SetExpandState(t.Nodes, t2.Nodes);
						break;
					}
				}
			}
		}

		public void UpdateContent(ListViewItem[] Items)
		{
			foreach (ListViewItem i in Items)
			{
				if (i.Tag != null && i.Tag is String)
				{
					String cat = (String)i.Tag;
					i.Group = listView1.Groups[cat];
				}
			}
			listView1.BeginUpdate();
			listView1.Items.Clear();
			listView1.Items.AddRange(Items);
			listView1.EndUpdate();
			if (OnSelectionChanged != null) OnSelectionChanged.Invoke(this, null);
		}

		private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (OnDirectoryChanged != null) OnDirectoryChanged.Invoke(e.Node.FullPath);
		}

		private void listView1_ItemActivate(object sender, EventArgs e)
		{
			if (listView1.SelectedItems[0].Group != null && listView1.SelectedItems[0].Group.Header == "Folders")
			{
				foreach (TreeNode n in treeView1.SelectedNode.Nodes)
				{
					if (n.Text == listView1.SelectedItems[0].Text)
					{
						treeView1.SelectedNode = n;
						return;
					}
				}
			}
			else
			{
				if (OnFileActivated != null) OnFileActivated.Invoke(treeView1.SelectedNode.FullPath + "/" + listView1.SelectedItems[0].Text);
			}
		}

		public event EventHandler OnAddDirectory;
		public event EventHandler OnAddFile;
		public event EventHandler OnRemove;
		public event EventHandler OnRename;

		private void buttonDirAdd_Click(object sender, EventArgs e)
		{
			if (OnAddDirectory != null) OnAddDirectory.Invoke(sender, e);
		}

		private void buttonFileAdd_Click(object sender, EventArgs e)
		{
			if (OnAddFile != null) OnAddFile.Invoke(sender, e);
		}

		private void buttonDelete_Click(object sender, EventArgs e)
		{
			if (OnRemove != null) OnRemove.Invoke(sender, e);
		}

		private void buttonRename_Click(object sender, EventArgs e)
		{
			if (OnRename != null) OnRename.Invoke(sender, e);
		}

		public event EventHandler OnSelectionChanged;

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (OnSelectionChanged != null) OnSelectionChanged.Invoke(sender, e);
		}

		public delegate void OnRightClickEventHandler(Point Location);
		public event OnRightClickEventHandler OnRightClick;

		private void listView1_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				ListViewItem i = listView1.GetItemAt(e.X, e.Y);
				if (i != null && OnRightClick != null) OnRightClick.Invoke(PointToClient(listView1.PointToScreen(e.Location)));
			}
		}
	}
}
