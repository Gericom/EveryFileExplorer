using System;
using System.Windows.Forms;
using WiiU.NintendoWare.LYT2;
using System.IO;
using System.Drawing.Imaging;
using static WiiU.NintendoWare.LYT2.FLIM;


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

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {


        }
    }
}
