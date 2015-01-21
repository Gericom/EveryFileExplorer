using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WiiU.NintendoWare.LYT2;

namespace WiiU.UI
{
	public partial class FLIMViewer : Form
	{
		FLIM Image;
		public FLIMViewer(FLIM Image)
		{
			this.Image = Image;
			InitializeComponent();
		}

		private void FLIMViewer_Load(object sender, EventArgs e)
		{
			pictureBox1.Image = Image.ToBitmap();
		}
	}
}
