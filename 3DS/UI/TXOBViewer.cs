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
using System.IO;
using _3DS.GPU;

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

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (Texture.HWFormat != Textures.ImageFormat.L8 &&
                Texture.HWFormat != Textures.ImageFormat.A8 &&
                Texture.HWFormat != Textures.ImageFormat.LA8 &&
                Texture.HWFormat != Textures.ImageFormat.HILO8 &&
                Texture.HWFormat != Textures.ImageFormat.RGB565 &&
                Texture.HWFormat != Textures.ImageFormat.RGB8 &&
                Texture.HWFormat != Textures.ImageFormat.RGBA5551 &&
                Texture.HWFormat != Textures.ImageFormat.RGBA4 &&
                Texture.HWFormat != Textures.ImageFormat.RGBA8 &&
                Texture.HWFormat != Textures.ImageFormat.ETC1 &&
                Texture.HWFormat != Textures.ImageFormat.ETC1A4)
            {
                MessageBox.Show("This texture format is currently not supported for importing.");
                return;
            }
            if (openFileDialog1.ShowDialog() == DialogResult.OK
                && openFileDialog1.FileName.Length > 0)
            {
                Bitmap b = new Bitmap(new MemoryStream(File.ReadAllBytes(openFileDialog1.FileName)));
                if (b.Width != Texture.Width && b.Height != Texture.Height)
                {
                    MessageBox.Show("Make sure the texture is even big as the original texture! (" + Texture.Width + "x" + Texture.Height + ")");
                    return;
                }
                for (int i = 0; i < Texture.NrLevels; i++)
                {
                    int l = i;
                    uint w = Texture.Width;
                    uint h = Texture.Height;
                    int bpp = Textures.GetBpp(Texture.HWFormat);
                    int offset = 0;
                    while (l > 0)
                    {
                        offset += (int)(w * h * bpp / 8);
                        w /= 2;
                        h /= 2;
                        l--;
                    }
                    byte[] result = Textures.FromBitmap(new Bitmap(b, (int)w, (int)h), Texture.HWFormat);
                    Array.Copy(result, 0, Texture.TextureImage.Data, offset, result.Length);
                }
                pictureBox1.Image = Texture.GetBitmap(toolStripComboBox1.SelectedIndex);
            }
        }
	}
}
