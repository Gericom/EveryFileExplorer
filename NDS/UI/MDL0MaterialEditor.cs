using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NDS.NitroSystem.G3D;
using LibEveryFileExplorer.GFX;

namespace NDS.UI
{
	public partial class MDL0MaterialEditor : UserControl
	{
		MDL0.Model.MaterialSet.Material Material;
		public MDL0MaterialEditor(MDL0.Model.MaterialSet.Material Material)
		{
			this.Material = Material;
			InitializeComponent();
		}

		private void MDL0MaterialEditor_Load(object sender, EventArgs e)
		{
			checkBox3.Checked = (Material.polyAttr & 1) != 0;
			checkBox4.Checked = (Material.polyAttr & 2) != 0;
			checkBox5.Checked = (Material.polyAttr & 4) != 0;
			checkBox6.Checked = (Material.polyAttr & 8) != 0;
			comboBox1.SelectedIndex = (int)((Material.polyAttr >> 4) & 0x3);
			comboBox2.SelectedIndex = (int)((Material.polyAttr >> 6) & 0x3);
			checkBox7.Checked = ((Material.polyAttr >> 11) & 0x1) == 1;
			checkBox8.Checked = ((Material.polyAttr >> 12) & 0x1) == 1;
			checkBox9.Checked = ((Material.polyAttr >> 13) & 0x1) == 1;
			comboBox3.SelectedIndex = (int)((Material.polyAttr >> 14) & 0x1);
			checkBox10.Checked = ((Material.polyAttr >> 15) & 0x1) == 1;
			trackBar1.Value = (int)((Material.polyAttr >> 16) & 31);
			trackBar2.Value = (int)((Material.polyAttr >> 24) & 63);

			checkBox1.Checked = (Material.diffAmb & 0x8000) != 0;
			button1.BackColor = System.Drawing.Color.FromArgb((int)GFXUtil.XBGR1555ToArgb((ushort)(Material.diffAmb & 0x7FFF)));
			button2.BackColor = System.Drawing.Color.FromArgb((int)GFXUtil.XBGR1555ToArgb((ushort)((Material.diffAmb >> 16) & 0x7FFF)));

			checkBox2.Checked = (Material.specEmi & 0x8000) != 0;
			button3.BackColor = System.Drawing.Color.FromArgb((int)GFXUtil.XBGR1555ToArgb((ushort)(Material.specEmi & 0x7FFF)));
			button4.BackColor = System.Drawing.Color.FromArgb((int)GFXUtil.XBGR1555ToArgb((ushort)((Material.specEmi >> 16) & 0x7FFF)));

			uint wrapS = (Material.texImageParam >> 16) & 1;
			if (wrapS != 0) wrapS += (Material.texImageParam >> 18) & 1;
			comboBox4.SelectedIndex = (int)wrapS;
			uint wrapT = (Material.texImageParam >> 17) & 1;
			if (wrapT != 0) wrapT += (Material.texImageParam >> 19) & 1;
			comboBox5.SelectedIndex = (int)wrapT;
		}
	}
}
