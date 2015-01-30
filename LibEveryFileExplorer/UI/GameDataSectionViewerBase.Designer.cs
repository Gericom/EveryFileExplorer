namespace LibEveryFileExplorer.UI
{
	partial class GameDataSectionViewerBase
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameDataSectionViewerBase));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.buttonAdd = new System.Windows.Forms.ToolStripButton();
			this.buttonRemove = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonUp = new System.Windows.Forms.ToolStripButton();
			this.buttonDown = new System.Windows.Forms.ToolStripButton();
			this.listViewNF1 = new LibEveryFileExplorer.UI.ListViewNF();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAdd,
            this.buttonRemove,
            this.toolStripSeparator1,
            this.buttonUp,
            this.buttonDown});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(535, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
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
			// listViewNF1
			// 
			this.listViewNF1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewNF1.FullRowSelect = true;
			this.listViewNF1.GridLines = true;
			this.listViewNF1.HideSelection = false;
			this.listViewNF1.Location = new System.Drawing.Point(0, 25);
			this.listViewNF1.Name = "listViewNF1";
			this.listViewNF1.Size = new System.Drawing.Size(535, 352);
			this.listViewNF1.TabIndex = 1;
			this.listViewNF1.UseCompatibleStateImageBehavior = false;
			this.listViewNF1.View = System.Windows.Forms.View.Details;
			// 
			// GameDataSectionViewerBase
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.listViewNF1);
			this.Controls.Add(this.toolStrip1);
			this.Name = "GameDataSectionViewerBase";
			this.Size = new System.Drawing.Size(535, 377);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.ToolStrip toolStrip1;
		internal System.Windows.Forms.ToolStripButton buttonAdd;
		internal System.Windows.Forms.ToolStripButton buttonRemove;
		internal System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		internal System.Windows.Forms.ToolStripButton buttonUp;
		internal System.Windows.Forms.ToolStripButton buttonDown;
		internal ListViewNF listViewNF1;

	}
}
