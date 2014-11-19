using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using _3DS.NintendoWare.FONT;
using LibEveryFileExplorer.GFX;

namespace _3DS.UI
{
	public partial class CFNTViewer : Form
	{
		CFNT Font;
		BitmapFont f;
		public CFNTViewer(CFNT Font)
		{
			this.Font = Font;
			f = Font.GetBitmapFont();
			InitializeComponent();
			pictureBox1.Image = f.PrintToBitmap(textBox1.Text, new BitmapFont.FontRenderSettings());
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			pictureBox1.Image = f.PrintToBitmap(textBox1.Text, new BitmapFont.FontRenderSettings());
		}
	}
}
