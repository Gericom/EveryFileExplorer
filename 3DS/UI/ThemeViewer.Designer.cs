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
            ((System.ComponentModel.ISupportInitialize)(this.bottomTextureImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.topTextureImage)).BeginInit();
            this.SuspendLayout();
            // 
            // bottomTextureImage
            // 
            this.bottomTextureImage.Location = new System.Drawing.Point(12, 283);
            this.bottomTextureImage.Name = "bottomTextureImage";
            this.bottomTextureImage.Size = new System.Drawing.Size(1244, 235);
            this.bottomTextureImage.TabIndex = 0;
            this.bottomTextureImage.TabStop = false;
            // 
            // topTextureImage
            // 
            this.topTextureImage.Location = new System.Drawing.Point(12, 12);
            this.topTextureImage.Name = "topTextureImage";
            this.topTextureImage.Size = new System.Drawing.Size(1244, 265);
            this.topTextureImage.TabIndex = 1;
            this.topTextureImage.TabStop = false;
            // 
            // ThemeViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1307, 530);
            this.Controls.Add(this.topTextureImage);
            this.Controls.Add(this.bottomTextureImage);
            this.Name = "ThemeViewer";
            this.Text = "ThemeViewer";
            this.Load += new System.EventHandler(this.ThemeViewer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.bottomTextureImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.topTextureImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox bottomTextureImage;
        private System.Windows.Forms.PictureBox topTextureImage;
    }
}