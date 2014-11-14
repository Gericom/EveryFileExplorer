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
            this.bottomTextureImage = new System.Windows.Forms.PictureBox();
            this.topTextureImage = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.bottomTextureImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.topTextureImage)).BeginInit();
            this.SuspendLayout();
            // 
            // bottomTextureImage
            // 
            this.bottomTextureImage.Location = new System.Drawing.Point(126, 198);
            this.bottomTextureImage.Name = "bottomTextureImage";
            this.bottomTextureImage.Size = new System.Drawing.Size(601, 197);
            this.bottomTextureImage.TabIndex = 0;
            this.bottomTextureImage.TabStop = false;
            // 
            // topTextureImage
            // 
            this.topTextureImage.Location = new System.Drawing.Point(126, -5);
            this.topTextureImage.Name = "topTextureImage";
            this.topTextureImage.Size = new System.Drawing.Size(601, 197);
            this.topTextureImage.TabIndex = 1;
            this.topTextureImage.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(85, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(22, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "top";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(85, 287);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "bottom";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // ThemeViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(959, 407);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.topTextureImage);
            this.Controls.Add(this.bottomTextureImage);
            this.Name = "ThemeViewer";
            this.Text = "ThemeViewer";
            this.Load += new System.EventHandler(this.ThemeViewer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.bottomTextureImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.topTextureImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox bottomTextureImage;
        private System.Windows.Forms.PictureBox topTextureImage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}