namespace GCNWii.UI
{
	partial class RARCViewer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RARCViewer));
			this.fileBrowser1 = new LibEveryFileExplorer.UI.FileBrowser();
			this.mainMenu1 = new LibEveryFileExplorer.UI.MainMenu(this.components);
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuExport = new System.Windows.Forms.MenuItem();
			this.menuItem13 = new System.Windows.Forms.MenuItem();
			this.menuReplace = new System.Windows.Forms.MenuItem();
			this.menuRename = new System.Windows.Forms.MenuItem();
			this.menuDelete = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuItem8 = new System.Windows.Forms.MenuItem();
			this.menuItem9 = new System.Windows.Forms.MenuItem();
			this.menuItem14 = new System.Windows.Forms.MenuItem();
			this.menuItem10 = new System.Windows.Forms.MenuItem();
			this.menuItem11 = new System.Windows.Forms.MenuItem();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.SuspendLayout();
			// 
			// fileBrowser1
			// 
			this.fileBrowser1.DeleteEnabled = false;
			this.fileBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fileBrowser1.Location = new System.Drawing.Point(0, 0);
			this.fileBrowser1.Name = "fileBrowser1";
			this.fileBrowser1.RenameEnabled = false;
			this.fileBrowser1.ShowAddDirectoryButton = false;
			this.fileBrowser1.ShowAddFileButton = false;
			this.fileBrowser1.ShowDeleteButton = false;
			this.fileBrowser1.ShowRenameButton = false;
			this.fileBrowser1.Size = new System.Drawing.Size(652, 338);
			this.fileBrowser1.TabIndex = 0;
			this.fileBrowser1.OnDirectoryChanged += new LibEveryFileExplorer.UI.FileBrowser.OnDirectoryChangedEventHandler(this.fileBrowser1_OnDirectoryChanged);
			this.fileBrowser1.OnFileActivated += new LibEveryFileExplorer.UI.FileBrowser.OnFileActivatedEventHandler(this.fileBrowser1_OnFileActivated);
			this.fileBrowser1.OnAddDirectory += new System.EventHandler(this.fileBrowser1_OnAddDirectory);
			this.fileBrowser1.OnAddFile += new System.EventHandler(this.fileBrowser1_OnAddFile);
			this.fileBrowser1.OnRemove += new System.EventHandler(this.fileBrowser1_OnRemove);
			this.fileBrowser1.OnRename += new System.EventHandler(this.fileBrowser1_OnRename);
			this.fileBrowser1.OnSelectionChanged += new System.EventHandler(this.fileBrowser1_OnSelectionChanged);
			this.fileBrowser1.OnRightClick += new LibEveryFileExplorer.UI.FileBrowser.OnRightClickEventHandler(this.fileBrowser1_OnRightClick);
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem5});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2,
            this.menuItem3,
            this.menuItem4,
            this.menuExport,
            this.menuItem13,
            this.menuReplace,
            this.menuRename,
            this.menuDelete});
			this.menuItem1.MergeOrder = 1;
			this.menuItem1.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
			this.menuItem1.Text = "Edit";
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 0;
			this.menuItem2.Text = "Add Folder...";
			this.menuItem2.Visible = false;
			this.menuItem2.Click += new System.EventHandler(this.fileBrowser1_OnAddDirectory);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 1;
			this.menuItem3.Text = "Add File...";
			this.menuItem3.Visible = false;
			this.menuItem3.Click += new System.EventHandler(this.fileBrowser1_OnAddFile);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 2;
			this.menuItem4.Text = "-";
			this.menuItem4.Visible = false;
			// 
			// menuExport
			// 
			this.menuExport.Enabled = false;
			this.menuExport.Index = 3;
			this.menuExport.Text = "Export...";
			this.menuExport.Click += new System.EventHandler(this.menuExport_Click);
			// 
			// menuItem13
			// 
			this.menuItem13.Index = 4;
			this.menuItem13.Text = "-";
			this.menuItem13.Visible = false;
			// 
			// menuReplace
			// 
			this.menuReplace.Enabled = false;
			this.menuReplace.Index = 5;
			this.menuReplace.Text = "Replace...";
			this.menuReplace.Visible = false;
			this.menuReplace.Click += new System.EventHandler(this.menuReplace_Click);
			// 
			// menuRename
			// 
			this.menuRename.Enabled = false;
			this.menuRename.Index = 6;
			this.menuRename.Text = "Rename...";
			this.menuRename.Visible = false;
			this.menuRename.Click += new System.EventHandler(this.fileBrowser1_OnRename);
			// 
			// menuDelete
			// 
			this.menuDelete.Enabled = false;
			this.menuDelete.Index = 7;
			this.menuDelete.Text = "Delete";
			this.menuDelete.Visible = false;
			this.menuDelete.Click += new System.EventHandler(this.fileBrowser1_OnRemove);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 1;
			this.menuItem5.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem6,
            this.menuItem7});
			this.menuItem5.MergeOrder = 3;
			this.menuItem5.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
			this.menuItem5.Text = "Tools";
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 0;
			this.menuItem6.Text = "-";
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 1;
			this.menuItem7.Text = "Export Directory Content...";
			this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			this.openFileDialog1.Filter = "All Files (*.*)|*.*";
			this.openFileDialog1.Title = "Import File";
			// 
			// folderBrowserDialog1
			// 
			this.folderBrowserDialog1.Description = "Select the directory to export the content of current directory to.";
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem8,
            this.menuItem9,
            this.menuItem14,
            this.menuItem10,
            this.menuItem11});
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 0;
			this.menuItem8.Text = "Export...";
			this.menuItem8.Click += new System.EventHandler(this.menuExport_Click);
			// 
			// menuItem9
			// 
			this.menuItem9.Index = 1;
			this.menuItem9.Text = "-";
			this.menuItem9.Visible = false;
			// 
			// menuItem14
			// 
			this.menuItem14.Index = 2;
			this.menuItem14.Text = "Replace...";
			this.menuItem14.Visible = false;
			this.menuItem14.Click += new System.EventHandler(this.menuReplace_Click);
			// 
			// menuItem10
			// 
			this.menuItem10.Index = 3;
			this.menuItem10.Text = "Rename...";
			this.menuItem10.Visible = false;
			this.menuItem10.Click += new System.EventHandler(this.fileBrowser1_OnRename);
			// 
			// menuItem11
			// 
			this.menuItem11.Index = 4;
			this.menuItem11.Text = "Delete";
			this.menuItem11.Visible = false;
			this.menuItem11.Click += new System.EventHandler(this.fileBrowser1_OnRemove);
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.Title = "Export...";
			// 
			// NARCViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(652, 338);
			this.Controls.Add(this.fileBrowser1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu1;
			this.Name = "NARCViewer";
			this.Text = "NARC Viewer";
			this.Load += new System.EventHandler(this.NARCViewer_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private LibEveryFileExplorer.UI.FileBrowser fileBrowser1;
		private LibEveryFileExplorer.UI.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuRename;
		private System.Windows.Forms.MenuItem menuDelete;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.Windows.Forms.MenuItem menuItem10;
		private System.Windows.Forms.MenuItem menuItem11;
		private System.Windows.Forms.MenuItem menuExport;
		private System.Windows.Forms.MenuItem menuItem13;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.MenuItem menuReplace;
		private System.Windows.Forms.MenuItem menuItem14;
	}
}