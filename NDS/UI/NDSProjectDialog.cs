using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace NDS.UI
{
	public partial class NDSProjectDialog : Form
	{
		public String ProjectName { get; private set; }
		public String ProjectDir { get; private set; }
		public String NDSPath { get; private set; }

		public NDSProjectDialog()
		{
			InitializeComponent();
			openFileDialog1.Filter = Nitro.NDS.Identifier.GetFileFilter();
		}

		private void NDSProjectDialog_Load(object sender, EventArgs e)
		{

		}

		private void button3_Click(object sender, EventArgs e)
		{
			if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& folderBrowserDialog1.SelectedPath.Length > 0)
			{
				if (new DirectoryInfo(folderBrowserDialog1.SelectedPath).GetFileSystemInfos().Length > 0)
				{
					MessageBox.Show("Please select an empty directory!");
					return;
				}
				ProjectDir = folderBrowserDialog1.SelectedPath;
				UpdatePaths();
			}
		}

		private void button4_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& openFileDialog1.FileName.Length > 0)
			{
				NDSPath = openFileDialog1.FileName;
				UpdatePaths();
			}
		}

		void UpdatePaths()
		{
			textBox1.Text = ProjectDir;
			textBox2.Text = NDSPath;
			if (ProjectDir != null && ProjectDir.Length > 0 && NDSPath != null && NDSPath.Length > 0 && ProjectName != null && ProjectName.Length > 0)
				button2.Enabled = true;
			else button2.Enabled = false;
		}

		private void textBox3_TextChanged(object sender, EventArgs e)
		{
			ProjectName = textBox3.Text;
			UpdatePaths();
		}
	}
}
