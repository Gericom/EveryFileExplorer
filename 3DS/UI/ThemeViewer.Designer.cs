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
            this.widthBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.plus40widthbutton = new System.Windows.Forms.Button();
            this.plus40heighbutton = new System.Windows.Forms.Button();
            this.adjustheightbutton = new System.Windows.Forms.Button();
            this.heightBox = new System.Windows.Forms.TextBox();
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
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(85, 287);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "bottom";
            // 
            // widthBox
            // 
            this.widthBox.Location = new System.Drawing.Point(763, 198);
            this.widthBox.Name = "widthBox";
            this.widthBox.Size = new System.Drawing.Size(100, 20);
            this.widthBox.TabIndex = 4;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(763, 225);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "adjust width";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // plus40widthbutton
            // 
            this.plus40widthbutton.Location = new System.Drawing.Point(763, 255);
            this.plus40widthbutton.Name = "plus40widthbutton";
            this.plus40widthbutton.Size = new System.Drawing.Size(75, 23);
            this.plus40widthbutton.TabIndex = 6;
            this.plus40widthbutton.Text = "+40";
            this.plus40widthbutton.UseVisualStyleBackColor = true;
            this.plus40widthbutton.Click += new System.EventHandler(this.button2_Click);
            // 
            // plus40heighbutton
            // 
            this.plus40heighbutton.Location = new System.Drawing.Point(763, 149);
            this.plus40heighbutton.Name = "plus40heighbutton";
            this.plus40heighbutton.Size = new System.Drawing.Size(75, 23);
            this.plus40heighbutton.TabIndex = 9;
            this.plus40heighbutton.Text = "+40";
            this.plus40heighbutton.UseVisualStyleBackColor = true;
            this.plus40heighbutton.Click += new System.EventHandler(this.plus40heighbutton_Click);
            // 
            // adjustheightbutton
            // 
            this.adjustheightbutton.Location = new System.Drawing.Point(763, 119);
            this.adjustheightbutton.Name = "adjustheightbutton";
            this.adjustheightbutton.Size = new System.Drawing.Size(75, 23);
            this.adjustheightbutton.TabIndex = 8;
            this.adjustheightbutton.Text = "adjust height";
            this.adjustheightbutton.UseVisualStyleBackColor = true;
            this.adjustheightbutton.Click += new System.EventHandler(this.adjustHeightbutton);
            // 
            // heightBox
            // 
            this.heightBox.Location = new System.Drawing.Point(763, 92);
            this.heightBox.Name = "heightBox";
            this.heightBox.Size = new System.Drawing.Size(100, 20);
            this.heightBox.TabIndex = 7;
            // 
            // ThemeViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(959, 407);
            this.Controls.Add(this.plus40heighbutton);
            this.Controls.Add(this.adjustheightbutton);
            this.Controls.Add(this.heightBox);
            this.Controls.Add(this.plus40widthbutton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.widthBox);
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
        private System.Windows.Forms.TextBox widthBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button plus40widthbutton;
        private System.Windows.Forms.Button plus40heighbutton;
        private System.Windows.Forms.Button adjustheightbutton;
        private System.Windows.Forms.TextBox heightBox;
    }
}