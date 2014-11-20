namespace _3DS.UI
{
	partial class CSTMViewer
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.wavePlayer1 = new CommonFiles.UI.WavePlayer();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.AutoSize = true;
			this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox1.Controls.Add(this.wavePlayer1);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 6);
			this.groupBox1.Size = new System.Drawing.Size(397, 48);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Preview";
			// 
			// wavePlayer1
			// 
			this.wavePlayer1.Dock = System.Windows.Forms.DockStyle.Top;
			this.wavePlayer1.Location = new System.Drawing.Point(3, 16);
			this.wavePlayer1.Name = "wavePlayer1";
			this.wavePlayer1.Size = new System.Drawing.Size(391, 26);
			this.wavePlayer1.TabIndex = 0;
			// 
			// CSTMViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(397, 241);
			this.Controls.Add(this.groupBox1);
			this.Name = "CSTMViewer";
			this.Text = "CSTMViewer";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CSTMViewer_FormClosed);
			this.Load += new System.EventHandler(this.CSTMViewer_Load);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CommonFiles.UI.WavePlayer wavePlayer1;
		private System.Windows.Forms.GroupBox groupBox1;
	}
}