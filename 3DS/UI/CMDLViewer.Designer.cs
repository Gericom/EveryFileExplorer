namespace _3DS.UI
{
	partial class CMDLViewer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CMDLViewer));
			this.simpleOpenGlControl1 = new Tao.Platform.Windows.SimpleOpenGlControl();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// simpleOpenGlControl1
			// 
			this.simpleOpenGlControl1.AccumBits = ((byte)(0));
			this.simpleOpenGlControl1.AutoCheckErrors = false;
			this.simpleOpenGlControl1.AutoFinish = false;
			this.simpleOpenGlControl1.AutoMakeCurrent = true;
			this.simpleOpenGlControl1.AutoSwapBuffers = true;
			this.simpleOpenGlControl1.BackColor = System.Drawing.Color.Black;
			this.simpleOpenGlControl1.ColorBits = ((byte)(32));
			this.simpleOpenGlControl1.DepthBits = ((byte)(32));
			this.simpleOpenGlControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.simpleOpenGlControl1.Location = new System.Drawing.Point(0, 25);
			this.simpleOpenGlControl1.Name = "simpleOpenGlControl1";
			this.simpleOpenGlControl1.Size = new System.Drawing.Size(462, 332);
			this.simpleOpenGlControl1.StencilBits = ((byte)(8));
			this.simpleOpenGlControl1.TabIndex = 1;
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(462, 25);
			this.toolStrip1.TabIndex = 2;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton1.Text = "Export";
			this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.Filter = "COLLADA DAE File (*.dae)|*.dae|Wavefront OBJ File (*.obj)|*.obj|Maya ASCII (*.ma)" +
    "|*.ma";
			// 
			// CMDLViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.simpleOpenGlControl1);
			this.Controls.Add(this.toolStrip1);
			this.DoubleBuffered = true;
			this.Name = "CMDLViewer";
			this.Size = new System.Drawing.Size(462, 357);
			this.Load += new System.EventHandler(this.MDL0Viewer_Load);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Tao.Platform.Windows.SimpleOpenGlControl simpleOpenGlControl1;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton toolStripButton1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
	}
}
