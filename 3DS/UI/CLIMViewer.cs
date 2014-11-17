using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using _3DS.NintendoWare.LYT1;

namespace _3DS.UI
{
	public partial class CLIMViewer : Form
	{
		CLIM Image;
		public CLIMViewer(CLIM Image)
		{
			this.Image = Image;
			InitializeComponent();
		}

		private void CLIMViewer_Load(object sender, EventArgs e)
		{
			pictureBox1.Image = Image.ToBitmap();
		}
	}
}
