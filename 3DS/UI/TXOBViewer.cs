using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using _3DS.NintendoWare.GFX;
using System.Drawing.Imaging;

namespace _3DS.UI
{
	public partial class TXOBViewer : UserControl
	{
		public ImageTextureCtr Texture;
		public TXOBViewer(ImageTextureCtr Texture)
		{
			this.Texture = Texture;
			InitializeComponent();
		}

		private void TXOBViewer_Load(object sender, EventArgs e)
		{
			for (int i = 0; i < Texture.NrLevels; i++)
			{
				toolStripComboBox1.Items.Add(i.ToString());
			}
			toolStripComboBox1.SelectedIndex = 0;
		}

		private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			pictureBox1.Image = Texture.GetBitmap(toolStripComboBox1.SelectedIndex);
		}

		private void toolStripButton2_Click(object sender, EventArgs e)
		{
			saveFileDialog1.FileName = Texture.Name + ".png";
			if (saveFileDialog1.ShowDialog() == DialogResult.OK
				&& saveFileDialog1.FileName.Length > 0)
			{
				pictureBox1.Image.Save(saveFileDialog1.FileName, ImageFormat.Png);
			}
		}
	}
}
