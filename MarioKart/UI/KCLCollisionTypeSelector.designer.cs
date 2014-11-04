namespace MarioKart.UI
{
	partial class KCLCollisionTypeSelector
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
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Collide = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ItemTemplate = new Microsoft.VisualBasic.PowerPacks.DataRepeaterItem();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			this.dataGridView1.AllowUserToResizeRows = false;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.name,
            this.Collide,
            this.Type});
			this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView1.Location = new System.Drawing.Point(0, 0);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ShowEditingIcon = false;
			this.dataGridView1.Size = new System.Drawing.Size(497, 304);
			this.dataGridView1.TabIndex = 0;
			this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
			this.dataGridView1.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGridView1_CellValidating);
			// 
			// name
			// 
			this.name.HeaderText = "Name";
			this.name.Name = "name";
			this.name.ReadOnly = true;
			// 
			// Collide
			// 
			this.Collide.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Collide.HeaderText = "Collide";
			this.Collide.Name = "Collide";
			// 
			// Type
			// 
			this.Type.HeaderText = "Type";
			this.Type.MaxInputLength = 4;
			this.Type.Name = "Type";
			// 
			// ItemTemplate
			// 
			this.ItemTemplate.Size = new System.Drawing.Size(232, 100);
			// 
			// KCLCollisionTypeSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(497, 304);
			this.Controls.Add(this.dataGridView1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "KCLCollisionTypeSelector";
			this.Text = "Collision Type Selector";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.kclType_FormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.kclType_FormClosed);
			this.Load += new System.EventHandler(this.kclType_Load);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Collide;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
		private Microsoft.VisualBasic.PowerPacks.DataRepeaterItem ItemTemplate;
    }
}