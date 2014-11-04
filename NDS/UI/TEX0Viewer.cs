using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NDS.NitroSystem.G3D;

namespace NDS.UI
{
	public partial class TEX0Viewer : UserControl
	{
		TEX0 Textures;
		public TEX0Viewer(TEX0 Textures)
		{
			this.Textures = Textures;
			InitializeComponent();
		}

		private void TEX0Viewer_Load(object sender, EventArgs e)
		{
			listBox1.BeginUpdate();
			listBox1.Items.Clear();
			for (int i = 0; i < Textures.dictTex.numEntry; i++)
			{
				listBox1.Items.Add(Textures.dictTex[i].Key);
			}
			listBox1.EndUpdate();
			listBox2.BeginUpdate();
			listBox2.Items.Clear();
			for (int i = 0; i < Textures.dictPltt.numEntry; i++)
			{
				listBox2.Items.Add(Textures.dictPltt[i].Key);
			}
			listBox2.EndUpdate();
			if (Textures.dictTex.numEntry != 0) listBox1.SelectedIndex = 0;
		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			var tex = Textures.dictTex[listBox1.SelectedIndex].Value;
			if (tex.Fmt != GPU.Textures.ImageFormat.DIRECT && listBox2.SelectedIndices.Count == 0)
			{
				if (Textures.dictPltt.numEntry != 0) listBox2.SelectedIndex = 0;
				return;
			}
			Bitmap b = null;
			try
			{
				b = tex.ToBitmap(Textures.dictPltt[listBox2.SelectedIndex].Value);
			}
			catch { }
			finally { pictureBox1.Image = b; }
		}
	}
}
