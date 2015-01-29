namespace MarioKart.UI
{
	partial class NKMDViewer2
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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.mapViewer1 = new MarioKart.UI.MapViewer.MapViewer();
			this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.mainMenu1 = new LibEveryFileExplorer.UI.MainMenu(this.components);
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(488, 330);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl1_Selecting);
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.mapViewer1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(480, 304);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Map";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// mapViewer1
			// 
			this.mapViewer1.BackColor = System.Drawing.Color.White;
			this.mapViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mapViewer1.Location = new System.Drawing.Point(3, 3);
			this.mapViewer1.MaximumViewport = 8192F;
			this.mapViewer1.MinimumViewport = 256F;
			this.mapViewer1.Name = "mapViewer1";
			this.mapViewer1.Size = new System.Drawing.Size(474, 298);
			this.mapViewer1.TabIndex = 0;
			this.mapViewer1.Viewport = 8192F;
			this.mapViewer1.Init3D += new MarioKart.UI.MapViewer.MapViewer.Init3DEventHandler(this.mapViewer1_Init3D);
			this.mapViewer1.RenderCollision += new MarioKart.UI.MapViewer.MapViewer.RenderCollisionEventHandler(this.mapViewer1_RenderCollision);
			this.mapViewer1.GroupMemberSelected += new MarioKart.UI.MapViewer.MapViewer.GroupMemberSelectedEventHandler(this.mapViewer1_GroupMemberSelected);
			// 
			// propertyGrid1
			// 
			this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
			this.propertyGrid1.Name = "propertyGrid1";
			this.propertyGrid1.Size = new System.Drawing.Size(177, 330);
			this.propertyGrid1.TabIndex = 1;
			this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.propertyGrid1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
			this.splitContainer1.Size = new System.Drawing.Size(669, 330);
			this.splitContainer1.SplitterDistance = 177;
			this.splitContainer1.TabIndex = 2;
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MergeOrder = 2;
			this.menuItem1.Text = "Collision";
			// 
			// NKMDViewer2
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(669, 330);
			this.Controls.Add(this.splitContainer1);
			this.Menu = this.mainMenu1;
			this.Name = "NKMDViewer2";
			this.Text = "NKMDViewer";
			this.Load += new System.EventHandler(this.NKMDViewer_Load);
			this.Shown += new System.EventHandler(this.NKMDViewer_Shown);
			this.Move += new System.EventHandler(this.NKMDViewer2_Move);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.PropertyGrid propertyGrid1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private LibEveryFileExplorer.UI.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private MapViewer.MapViewer mapViewer1;
	}
}