namespace _3DS.UI
{
    partial class ThemeViewer
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.topBackgroundImage = new System.Windows.Forms.PictureBox();
            this.bottomBackgroundImage = new System.Windows.Forms.PictureBox();
            this.folderOpenImage = new System.Windows.Forms.PictureBox();
            this.folderClosedImage = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.iconBorder24pxImage = new System.Windows.Forms.PictureBox();
            this.iconBorder48pxImage = new System.Windows.Forms.PictureBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topBackgroundImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomBackgroundImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.folderOpenImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.folderClosedImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iconBorder24pxImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iconBorder48pxImage)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1397, 518);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.bottomBackgroundImage);
            this.tabPage1.Controls.Add(this.topBackgroundImage);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1389, 492);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Background";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.iconBorder24pxImage);
            this.tabPage2.Controls.Add(this.iconBorder48pxImage);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.folderClosedImage);
            this.tabPage2.Controls.Add(this.folderOpenImage);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1389, 492);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Icons";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // topBackgroundImage
            // 
            this.topBackgroundImage.Location = new System.Drawing.Point(4, 7);
            this.topBackgroundImage.Name = "topBackgroundImage";
            this.topBackgroundImage.Size = new System.Drawing.Size(1366, 212);
            this.topBackgroundImage.TabIndex = 0;
            this.topBackgroundImage.TabStop = false;
            // 
            // bottomBackgroundImage
            // 
            this.bottomBackgroundImage.Location = new System.Drawing.Point(4, 225);
            this.bottomBackgroundImage.Name = "bottomBackgroundImage";
            this.bottomBackgroundImage.Size = new System.Drawing.Size(1366, 212);
            this.bottomBackgroundImage.TabIndex = 1;
            this.bottomBackgroundImage.TabStop = false;
            // 
            // folderOpenImage
            // 
            this.folderOpenImage.Location = new System.Drawing.Point(8, 24);
            this.folderOpenImage.Name = "folderOpenImage";
            this.folderOpenImage.Size = new System.Drawing.Size(78, 61);
            this.folderOpenImage.TabIndex = 0;
            this.folderOpenImage.TabStop = false;
            // 
            // folderClosedImage
            // 
            this.folderClosedImage.Location = new System.Drawing.Point(92, 24);
            this.folderClosedImage.Name = "folderClosedImage";
            this.folderClosedImage.Size = new System.Drawing.Size(78, 61);
            this.folderClosedImage.TabIndex = 1;
            this.folderClosedImage.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "open folder";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(89, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "closed folder";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(257, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "24x24 border";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(171, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "48x48 border";
            // 
            // iconBorder24pxImage
            // 
            this.iconBorder24pxImage.Location = new System.Drawing.Point(260, 24);
            this.iconBorder24pxImage.Name = "iconBorder24pxImage";
            this.iconBorder24pxImage.Size = new System.Drawing.Size(78, 61);
            this.iconBorder24pxImage.TabIndex = 5;
            this.iconBorder24pxImage.TabStop = false;
            // 
            // iconBorder48pxImage
            // 
            this.iconBorder48pxImage.Location = new System.Drawing.Point(176, 24);
            this.iconBorder48pxImage.Name = "iconBorder48pxImage";
            this.iconBorder48pxImage.Size = new System.Drawing.Size(78, 61);
            this.iconBorder48pxImage.TabIndex = 4;
            this.iconBorder48pxImage.TabStop = false;
            // 
            // ThemeViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1397, 575);
            this.Controls.Add(this.tabControl1);
            this.Name = "ThemeViewer";
            this.Text = "ThemeViewer";
            this.Load += new System.EventHandler(this.ThemeViewer_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topBackgroundImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomBackgroundImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.folderOpenImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.folderClosedImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iconBorder24pxImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iconBorder48pxImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.PictureBox bottomBackgroundImage;
        private System.Windows.Forms.PictureBox topBackgroundImage;
        private System.Windows.Forms.PictureBox folderClosedImage;
        private System.Windows.Forms.PictureBox folderOpenImage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox iconBorder24pxImage;
        private System.Windows.Forms.PictureBox iconBorder48pxImage;

    }
}