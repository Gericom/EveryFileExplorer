namespace _3DS.UI
{
	partial class SARCViewer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SARCViewer));
			this.mainMenu1 = new LibEveryFileExplorer.UI.MainMenu(this.components);
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuExport = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuReplace = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuExportDir = new System.Windows.Forms.MenuItem();
			this.fileBrowser1 = new LibEveryFileExplorer.UI.FileBrowser();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuItem8 = new System.Windows.Forms.MenuItem();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem2});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuExport,
            this.menuItem5,
            this.menuReplace});
			this.menuItem1.MergeOrder = 1;
			this.menuItem1.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
			this.menuItem1.Text = "Edit";
			// 
			// menuExport
			// 
			this.menuExport.Enabled = false;
			this.menuExport.Index = 0;
			this.menuExport.Text = "Export...";
			this.menuExport.Click += new System.EventHandler(this.OnExport);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 1;
			this.menuItem5.Text = "-";
			// 
			// menuReplace
			// 
			this.menuReplace.Enabled = false;
			this.menuReplace.Index = 2;
			this.menuReplace.Text = "Replace...";
			this.menuReplace.Click += new System.EventHandler(this.menuReplace_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 1;
			this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem3,
            this.menuExportDir});
			this.menuItem2.MergeOrder = 2;
			this.menuItem2.MergeType = System.Windows.Forms.MenuMerge.MergeItems;
			this.menuItem2.Text = "Tools";
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 0;
			this.menuItem3.Text = "-";
			// 
			// menuExportDir
			// 
			this.menuExportDir.Index = 1;
			this.menuExportDir.Text = "Export Directory Content...";
			this.menuExportDir.Click += new System.EventHandler(this.menuExportDir_Click);
			// 
			// fileBrowser1
			// 
			this.fileBrowser1.DeleteEnabled = true;
			this.fileBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fileBrowser1.Location = new System.Drawing.Point(0, 0);
			this.fileBrowser1.Name = "fileBrowser1";
			this.fileBrowser1.RenameEnabled = true;
			this.fileBrowser1.ShowAddDirectoryButton = false;
			this.fileBrowser1.ShowAddFileButton = false;
			this.fileBrowser1.ShowDeleteButton = false;
			this.fileBrowser1.ShowRenameButton = false;
			this.fileBrowser1.Size = new System.Drawing.Size(652, 338);
			this.fileBrowser1.TabIndex = 0;
			this.fileBrowser1.OnDirectoryChanged += new LibEveryFileExplorer.UI.FileBrowser.OnDirectoryChangedEventHandler(this.fileBrowser1_OnDirectoryChanged);
			this.fileBrowser1.OnFileActivated += new LibEveryFileExplorer.UI.FileBrowser.OnFileActivatedEventHandler(this.fileBrowser1_OnFileActivated);
			this.fileBrowser1.OnSelectionChanged += new System.EventHandler(this.fileBrowser1_OnSelectionChanged);
			this.fileBrowser1.OnRightClick += new LibEveryFileExplorer.UI.FileBrowser.OnRightClickEventHandler(this.fileBrowser1_OnRightClick);
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem4,
            this.menuItem7,
            this.menuItem8});
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 0;
			this.menuItem4.Text = "Export...";
			this.menuItem4.Click += new System.EventHandler(this.OnExport);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 1;
			this.menuItem7.Text = "-";
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 2;
			this.menuItem8.Text = "Replace...";
			this.menuItem8.Click += new System.EventHandler(this.menuReplace_Click);
			// 
			// folderBrowserDialog1
			// 
			this.folderBrowserDialog1.Description = "Select the directory to export the content of current directory to.";
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// SARCViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(652, 338);
			this.Controls.Add(this.fileBrowser1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu1;
			this.Name = "SARCViewer";
			this.Text = "SARC Viewer";
			this.Load += new System.EventHandler(this.SARCViewer_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private LibEveryFileExplorer.UI.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private LibEveryFileExplorer.UI.FileBrowser fileBrowser1;
		private System.Windows.Forms.MenuItem menuExport;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuExportDir;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem menuReplace;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
	}
}