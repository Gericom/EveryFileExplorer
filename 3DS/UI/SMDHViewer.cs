using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using LibEveryFileExplorer.GFX;

namespace _3DS.UI
{
	public partial class SMDHViewer : Form
	{
		private string[] Languages =
		{
			"JPN",
			"ENG",
			"FRE",
			"GER",
			"ITA",
			"SPA",
			"SCHI",
			"KOR",
			"DUT",
			"POR",
			"RUS",
			"TCHI",
			"UNK",
			"UNK",
			"UNK",
			"UNK"
		};

		public SMDH icn;
		public SMDHViewer(SMDH Icon)
		{
			this.icn = Icon;
			InitializeComponent();
			typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null,
				dataGridView1, new object[] { true });
			typeof(TabControl).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null,
				tabControl1, new object[] { true });
			typeof(TabPage).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null,
				tabPage1, new object[] { true });
			typeof(TabPage).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null,
				tabPage2, new object[] { true });
			typeof(TabPage).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null,
				tabPage3, new object[] { true });
		}

		private void SMDHViewer_Load(object sender, EventArgs e)
		{
			dataGridView1.SuspendLayout();
			int i = 0;
			foreach (var v in icn.AppTitles)
			{
				dataGridView1.Rows.Add(Languages[i++], v.ShortDescription, v.LongDescription, v.Publisher);
			}
			dataGridView1.ResumeLayout();
			pictureBox1.Image = icn.GetSmallIcon();
			pictureBox2.Image = icn.GetLargeIcon();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			saveFileDialog1.Title = "Export Small Icon";
			if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& saveFileDialog1.FileName.Length > 0)
			{
				File.Create(saveFileDialog1.FileName).Close();
				icn.GetSmallIcon().Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			saveFileDialog1.Title = "Export Large Icon";
			if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& saveFileDialog1.FileName.Length > 0)
			{
				File.Create(saveFileDialog1.FileName).Close();
				icn.GetLargeIcon().Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
			}
		}

		private void button3_Click(object sender, EventArgs e)
		{
			openFileDialog1.Title = "Import Small Icon";
			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& openFileDialog1.FileName.Length > 0)
			{
				Bitmap b = GFXUtil.Resize(new Bitmap(new MemoryStream(File.ReadAllBytes(openFileDialog1.FileName))), 24, 24);
				icn.SmallIcon = GPU.Textures.FromBitmap(b, GPU.Textures.ImageFormat.RGB565, true);
				b.Dispose();
				pictureBox1.Image = icn.GetSmallIcon();
			}
		}

		private void button4_Click(object sender, EventArgs e)
		{
			openFileDialog1.Title = "Import Large Icon";
			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& openFileDialog1.FileName.Length > 0)
			{
				Bitmap b = GFXUtil.Resize(new Bitmap(new MemoryStream(File.ReadAllBytes(openFileDialog1.FileName))), 48, 48);
				icn.LargeIcon = GPU.Textures.FromBitmap(b, GPU.Textures.ImageFormat.RGB565, true);
				b.Dispose();
				pictureBox2.Image = icn.GetLargeIcon();
			}
		}

		private void button5_Click(object sender, EventArgs e)
		{
			openFileDialog1.Title = "Import Both Icons";
			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& openFileDialog1.FileName.Length > 0)
			{
				Bitmap b = new Bitmap(new MemoryStream(File.ReadAllBytes(openFileDialog1.FileName)));
				Bitmap Small = GFXUtil.Resize(b, 24, 24);
				Bitmap Big = GFXUtil.Resize(b, 48, 48);
				icn.SmallIcon = GPU.Textures.FromBitmap(Small, GPU.Textures.ImageFormat.RGB565, true);
				icn.LargeIcon = GPU.Textures.FromBitmap(Big, GPU.Textures.ImageFormat.RGB565, true);
				b.Dispose();
				Small.Dispose();
				Big.Dispose();
				pictureBox1.Image = icn.GetSmallIcon();
				pictureBox2.Image = icn.GetLargeIcon();
			}
		}
	}
}
