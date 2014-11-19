using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tao.OpenGl;
using LibEveryFileExplorer.Files;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using LibEveryFileExplorer;
using _3DS.NintendoWare.LYT1;

namespace _3DS.UI
{
	public partial class CLYTViewer : Form
	{
		bool init = false;
		CLYT NWLayout;
		CLIM[] Textures;
		BasicShader BShader = new BasicShader();
		ImageList ImageL;

		public CLYTViewer(CLYT Layout)
		{
			this.NWLayout = Layout;
			InitializeComponent();
			Win32Util.SetWindowTheme(treeView1.Handle, "explorer", null);
			Win32Util.SetWindowTheme(treeView2.Handle, "explorer", null);
		}

		private void CLYTViewer_Load(object sender, EventArgs e)
		{
			simpleOpenGlControl1.InitializeContexts();
			simpleOpenGlControl1.Width = (int)NWLayout.Layout.LayoutSize.X;
			simpleOpenGlControl1.Height = (int)NWLayout.Layout.LayoutSize.Y;
			Gl.ReloadFunctions();
			Gl.glEnable(Gl.GL_COLOR_MATERIAL);
			Gl.glDisable(Gl.GL_DEPTH_TEST);
			//Gl.glDepthFunc(Gl.GL_ALWAYS);
			Gl.glEnable(Gl.GL_LOGIC_OP);
			Gl.glDisable(Gl.GL_CULL_FACE);
			Gl.glEnable(Gl.GL_TEXTURE_2D);

			//Gl.glEnable(Gl.GL_LINE_SMOOTH);
			Gl.glEnable(Gl.GL_BLEND);

			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

			if (NWLayout.TextureList != null)
			{
				int i = 0;
				Textures = new CLIM[NWLayout.TextureList.NrTextures];
				foreach (String s in NWLayout.TextureList.TextureNames)
				{
					byte[] data = data = ((ViewableFile)Tag).File.FindFileRelative("../timg/" + s);
					if (data == null) data = ((ViewableFile)Tag).File.FindFileRelative(s);
					if (data != null) Textures[i] = new CLIM(data);
					i++;
				}
			}
			if (NWLayout.Materials != null)
			{
				int i = 0;
				foreach (var v in NWLayout.Materials.Materials)
				{
					int j = 0;
					foreach (var t in v.TexMaps)
					{
						if (Textures[t.TexIndex] != null) UploadTex(t, Textures[t.TexIndex], i * 4 + j + 1);
						j++;
					}
					v.SetupShader();
					i++;
				}
			}
			BShader.Compile(false);

			ImageL = new ImageList();
			ImageL.ColorDepth = ColorDepth.Depth32Bit;
			ImageL.ImageSize = new System.Drawing.Size(16, 16);
			ImageL.Images.Add("pan1", Resource.zone16);
			ImageL.Images.Add("pic1", Resource.image16);
			ImageL.Images.Add("wnd1", Resource.slide);
			ImageL.Images.Add("txt1", Resource.edit);
			ImageL.Images.Add("grp1", Resource.zones_stack);
			treeView1.ImageList = ImageL;
			treeView2.ImageList = ImageL;

			treeView1.BeginUpdate();
			treeView1.Nodes.Clear();
			treeView1.Nodes.Add(NWLayout.RootPane.GetTreeNodes());
			treeView1.EndUpdate();

			treeView2.BeginUpdate();
			treeView2.Nodes.Clear();
			treeView2.Nodes.Add(NWLayout.RootGroup.GetTreeNodes());
			treeView2.EndUpdate();

			init = true;
			Render();
		}

		private void UploadTex(mat1.MaterialEntry.TexMap TexMap, CLIM Texture, int Id)
		{
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, Id);
			Gl.glColor3f(1, 1, 1);
			Bitmap b = Texture.ToBitmap();
			//b.RotateFlip(RotateFlipType.RotateNoneFlipY);
			BitmapData d = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA8, b.Width, b.Height, 0, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, d.Scan0);
			b.UnlockBits(d);

			Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, (TexMap.MagFilter == mat1.MaterialEntry.TexMap.FilterMode.Linear) ? Gl.GL_LINEAR : Gl.GL_NEAREST);
			Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, (TexMap.MinFilter == mat1.MaterialEntry.TexMap.FilterMode.Linear) ? Gl.GL_LINEAR : Gl.GL_NEAREST);

			switch (TexMap.WrapS)
			{
				case mat1.MaterialEntry.TexMap.WrapMode.Clamp:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
					break;
				case mat1.MaterialEntry.TexMap.WrapMode.Repeat:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
					break;
				case mat1.MaterialEntry.TexMap.WrapMode.Mirror:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_MIRRORED_REPEAT);
					break;
				default:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
					break;
			}

			switch (TexMap.WrapT)
			{
				case mat1.MaterialEntry.TexMap.WrapMode.Clamp:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
					break;
				case mat1.MaterialEntry.TexMap.WrapMode.Repeat:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
					break;
				case mat1.MaterialEntry.TexMap.WrapMode.Mirror:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_MIRRORED_REPEAT);
					break;
				default:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
					break;
			}
		}

		public void Render()
		{
			if (!init) return;
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glLoadIdentity();
			Gl.glViewport(0, 0, simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);

			Gl.glOrtho(-NWLayout.Layout.LayoutSize.X / 2.0f, NWLayout.Layout.LayoutSize.X / 2.0f, -NWLayout.Layout.LayoutSize.Y / 2.0f, NWLayout.Layout.LayoutSize.Y / 2.0f, -1000, 1000);
			//Glu.gluPerspective(90, aspect, 0.02f, 1000.0f);//0.02f, 32.0f);
			//Gl.glTranslatef(0, 0, -100);


			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glLoadIdentity();

			/*if (!picking)*/
			Gl.glClearColor(1, 1, 1, 1);//BGColor.R / 255f, BGColor.G / 255f, BGColor.B / 255f, 1f);
			//else Gl.glClearColor(0, 0, 0, 1);
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

			Gl.glColor4f(1, 1, 1, 1);
			Gl.glEnable(Gl.GL_TEXTURE_2D);
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
			Gl.glColor4f(1, 1, 1, 1);
			Gl.glDisable(Gl.GL_CULL_FACE);
			Gl.glEnable(Gl.GL_ALPHA_TEST);
			Gl.glEnable(Gl.GL_BLEND);
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

			Gl.glAlphaFunc(Gl.GL_ALWAYS, 0f);

			Gl.glLoadIdentity();


			//if (!picking)
			{
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
				BShader.Enable();
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
				Gl.glColor4f(204 / 255f, 204 / 255f, 204 / 255f, 1);
				int xbase = 0;
				for (int y = 0; y < simpleOpenGlControl1.Height; y += 8)
				{
					for (int x = xbase; x < simpleOpenGlControl1.Width; x += 16)
					{
						Gl.glRectf(x - simpleOpenGlControl1.Width / 2f, y - simpleOpenGlControl1.Height / 2f, x - simpleOpenGlControl1.Width / 2f + 8, y - simpleOpenGlControl1.Height / 2f + 8);
					}
					if (xbase == 0) xbase = 8;
					else xbase = 0;
				}
			}
			/*if (picking)
			{
				BasicShaderb.Enable();
				int idx = 0;
				Layout.PAN1.Render(Layout, ref idx, 255, picking);
				pic = new byte[4];
				Bitmap b = IO.Util.ScreenShot(simpleOpenGlControl1);
				Gl.glReadPixels(MousePoint.X, (int)simpleOpenGlControl1.Height - MousePoint.Y, 1, 1, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, pic);
				Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
				Render();
				//simpleOpenGlControl1.Refresh();
				//Render();
			}
			else
			{*/
			NWLayout.RootPane.Render(NWLayout, Textures, 255);
			simpleOpenGlControl1.Refresh();
			//}
		}

		private void CLYTViewer_Layout(object sender, LayoutEventArgs e)
		{
			Render();
		}

		private void CLYTViewer_Resize(object sender, EventArgs e)
		{
			Render();
			Render();
		}

		private void CLYTViewer_Activated(object sender, EventArgs e)
		{
			for (int i = 0; i < 8; i++) Render();
		}
	}
}
