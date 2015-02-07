namespace MarioKart.UI
{
	partial class MKDSRouteViewer
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MKDSRouteViewer));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.listViewNF1 = new LibEveryFileExplorer.UI.ListViewNF();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStrip2 = new System.Windows.Forms.ToolStrip();
			this.listViewNF2 = new LibEveryFileExplorer.UI.ListViewNF();
			this.buttonAdd = new System.Windows.Forms.ToolStripButton();
			this.buttonRemove = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonUp = new System.Windows.Forms.ToolStripButton();
			this.buttonDown = new System.Windows.Forms.ToolStripButton();
			this.buttonRouteAdd = new System.Windows.Forms.ToolStripButton();
			this.buttonRouteRemove = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonRouteUp = new System.Windows.Forms.ToolStripButton();
			this.buttonRouteDown = new System.Windows.Forms.ToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.toolStrip2.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.listViewNF2);
			this.splitContainer1.Panel1.Controls.Add(this.toolStrip1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.listViewNF1);
			this.splitContainer1.Panel2.Controls.Add(this.toolStrip2);
			this.splitContainer1.Size = new System.Drawing.Size(474, 369);
			this.splitContainer1.SplitterDistance = 173;
			this.splitContainer1.TabIndex = 0;
			// 
			// listViewNF1
			// 
			this.listViewNF1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewNF1.FullRowSelect = true;
			this.listViewNF1.GridLines = true;
			this.listViewNF1.HideSelection = false;
			this.listViewNF1.Location = new System.Drawing.Point(0, 25);
			this.listViewNF1.Name = "listViewNF1";
			this.listViewNF1.Size = new System.Drawing.Size(297, 344);
			this.listViewNF1.TabIndex = 0;
			this.listViewNF1.UseCompatibleStateImageBehavior = false;
			this.listViewNF1.View = System.Windows.Forms.View.Details;
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonRouteAdd,
            this.buttonRouteRemove,
            this.toolStripSeparator2,
            this.buttonRouteUp,
            this.buttonRouteDown});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(173, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStrip2
			// 
			this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAdd,
            this.buttonRemove,
            this.toolStripSeparator1,
            this.buttonUp,
            this.buttonDown});
			this.toolStrip2.Location = new System.Drawing.Point(0, 0);
			this.toolStrip2.Name = "toolStrip2";
			this.toolStrip2.Size = new System.Drawing.Size(297, 25);
			this.toolStrip2.TabIndex = 1;
			this.toolStrip2.Text = "toolStrip2";
			// 
			// listViewNF2
			// 
			this.listViewNF2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewNF2.FullRowSelect = true;
			this.listViewNF2.GridLines = true;
			this.listViewNF2.HideSelection = false;
			this.listViewNF2.Location = new System.Drawing.Point(0, 25);
			this.listViewNF2.MultiSelect = false;
			this.listViewNF2.Name = "listViewNF2";
			this.listViewNF2.Size = new System.Drawing.Size(173, 344);
			this.listViewNF2.TabIndex = 1;
			this.listViewNF2.UseCompatibleStateImageBehavior = false;
			this.listViewNF2.View = System.Windows.Forms.View.Details;
			this.listViewNF2.SelectedIndexChanged += new System.EventHandler(this.listViewNF2_SelectedIndexChanged);
			// 
			// buttonAdd
			// 
			this.buttonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonAdd.Image = ((System.Drawing.Image)(resources.GetObject("buttonAdd.Image")));
			this.buttonAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.Size = new System.Drawing.Size(23, 22);
			this.buttonAdd.Text = "Add";
			// 
			// buttonRemove
			// 
			this.buttonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRemove.Image = ((System.Drawing.Image)(resources.GetObject("buttonRemove.Image")));
			this.buttonRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRemove.Name = "buttonRemove";
			this.buttonRemove.Size = new System.Drawing.Size(23, 22);
			this.buttonRemove.Text = "Remove";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonUp
			// 
			this.buttonUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonUp.Image = ((System.Drawing.Image)(resources.GetObject("buttonUp.Image")));
			this.buttonUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonUp.Name = "buttonUp";
			this.buttonUp.Size = new System.Drawing.Size(23, 22);
			this.buttonUp.Text = "Up";
			// 
			// buttonDown
			// 
			this.buttonDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonDown.Image = ((System.Drawing.Image)(resources.GetObject("buttonDown.Image")));
			this.buttonDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonDown.Name = "buttonDown";
			this.buttonDown.Size = new System.Drawing.Size(23, 22);
			this.buttonDown.Text = "Down";
			// 
			// buttonRouteAdd
			// 
			this.buttonRouteAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRouteAdd.Image = ((System.Drawing.Image)(resources.GetObject("buttonRouteAdd.Image")));
			this.buttonRouteAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRouteAdd.Name = "buttonRouteAdd";
			this.buttonRouteAdd.Size = new System.Drawing.Size(23, 22);
			this.buttonRouteAdd.Text = "Add";
			// 
			// buttonRouteRemove
			// 
			this.buttonRouteRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRouteRemove.Image = ((System.Drawing.Image)(resources.GetObject("buttonRouteRemove.Image")));
			this.buttonRouteRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRouteRemove.Name = "buttonRouteRemove";
			this.buttonRouteRemove.Size = new System.Drawing.Size(23, 22);
			this.buttonRouteRemove.Text = "Remove";
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonRouteUp
			// 
			this.buttonRouteUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRouteUp.Image = ((System.Drawing.Image)(resources.GetObject("buttonRouteUp.Image")));
			this.buttonRouteUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRouteUp.Name = "buttonRouteUp";
			this.buttonRouteUp.Size = new System.Drawing.Size(23, 22);
			this.buttonRouteUp.Text = "Up";
			// 
			// buttonRouteDown
			// 
			this.buttonRouteDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRouteDown.Image = ((System.Drawing.Image)(resources.GetObject("buttonRouteDown.Image")));
			this.buttonRouteDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRouteDown.Name = "buttonRouteDown";
			this.buttonRouteDown.Size = new System.Drawing.Size(23, 22);
			this.buttonRouteDown.Text = "Down";
			// 
			// MKDSRouteViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "MKDSRouteViewer";
			this.Size = new System.Drawing.Size(474, 369);
			this.Load += new System.EventHandler(this.MKDSRouteViewer_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.toolStrip2.ResumeLayout(false);
			this.toolStrip2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private LibEveryFileExplorer.UI.ListViewNF listViewNF2;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private LibEveryFileExplorer.UI.ListViewNF listViewNF1;
		private System.Windows.Forms.ToolStrip toolStrip2;
		internal System.Windows.Forms.ToolStripButton buttonRouteAdd;
		internal System.Windows.Forms.ToolStripButton buttonRouteRemove;
		internal System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		internal System.Windows.Forms.ToolStripButton buttonRouteUp;
		internal System.Windows.Forms.ToolStripButton buttonRouteDown;
		internal System.Windows.Forms.ToolStripButton buttonAdd;
		internal System.Windows.Forms.ToolStripButton buttonRemove;
		internal System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		internal System.Windows.Forms.ToolStripButton buttonUp;
		internal System.Windows.Forms.ToolStripButton buttonDown;
	}
}
