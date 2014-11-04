using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NDS.NitroSystem.G3D;

namespace NDS.UI
{
	public partial class NSBTXViewer : Form
	{
		NSBTX Textures;
		public NSBTXViewer(NSBTX Textures)
		{
			this.Textures = Textures;
			InitializeComponent();
			Controls.Add(new TEX0Viewer(Textures.TexPlttSet) { Dock = DockStyle.Fill });
		}

		private void NSBTXViewer_Load(object sender, EventArgs e)
		{

		}
	}
}
