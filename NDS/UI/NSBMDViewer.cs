using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tao.OpenGl;
using LibEveryFileExplorer._3D;
using System.Drawing.Imaging;
using NDS.NitroSystem.G3D;
using NDS.GPU;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer;
using System.Runtime.InteropServices;

namespace NDS.UI
{
	public partial class NSBMDViewer : Form, IUseOtherFiles
	{
		ImageList ImageL;
		MDL0Viewer ModViewer = null;

		NSBMD mod;
		NSBTX tex = null;

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		private static extern int SetWindowTheme(IntPtr hWnd, string appName, string partList);

		public NSBMDViewer(NSBMD mod)
		{
			this.mod = mod;
			InitializeComponent();
			SetWindowTheme(treeView1.Handle, "explorer", null);
			ImageL = new ImageList();
			ImageL.ColorDepth = ColorDepth.Depth32Bit;
			ImageL.ImageSize = new Size(16, 16);
			ImageL.Images.Add(Resource.jar);
			ImageL.Images.Add(Resource.point);
			ImageL.Images.Add(Resource.fruit);
			ImageL.Images.Add(Resource.molecule);
			ImageL.Images.Add(Resource.image_sunset16);
			ImageL.Images.Add(Resource.color_swatch);
			ImageL.Images.Add(Resource.images_stack);
			ImageL.Images.Add(Resource.fruit_grape);
			ImageL.Images.Add(Resource.node);
			ImageL.Images.Add(Resource.color_swatches);
			ImageL.Images.Add(Resource.images_stack);
			treeView1.ImageList = ImageL;
		}


		private void CGFX_Load(object sender, EventArgs e)
		{
			treeView1.BeginUpdate();
			treeView1.Nodes.Clear();
			if (mod.ModelSet != null)
			{
				TreeNode mdl0 = new TreeNode("MDL0", 0, 0);
				mdl0.Expand();
				treeView1.Nodes.Add(mdl0);
				for (int i = 0; i < mod.ModelSet.models.Length; i++)
				{
					TreeNode mdl = new TreeNode(mod.ModelSet.dict[i].Key, 1, 1) { Tag = mod.ModelSet.models[i] };
					mdl0.Nodes.Add(mdl);
					if (i == 0) treeView1.SelectedNode = mdl;
					var v = mod.ModelSet.models[i];
					TreeNode n = new TreeNode("Nodes", 8, 8);
					mdl.Nodes.Add(n);
					for (int j = 0; j < v.nodes.data.Length; j++)
					{
						n.Nodes.Add(new TreeNode(v.nodes.dict[j].Key, 3, 3) { Tag = v.nodes.data[j] });
					}
					n = new TreeNode("Materials", 7, 7);
					mdl.Nodes.Add(n);
					for (int j = 0; j < v.materials.materials.Length; j++)
					{
						n.Nodes.Add(new TreeNode(v.materials.dict[j].Key, 2, 2) { Tag = v.materials.materials[j] });
					}
					n = new TreeNode("Textures", 10, 10);
					mdl.Nodes.Add(n);
					for (int j = 0; j < v.materials.dictTexToMatList.numEntry; j++)
					{
						n.Nodes.Add(new TreeNode(v.materials.dictTexToMatList[j].Key, 4, 4) { Tag = v.materials.dictTexToMatList[j].Value });
					}
					n = new TreeNode("Palettes", 9, 9);
					mdl.Nodes.Add(n);
					for (int j = 0; j < v.materials.dictPlttToMatList.numEntry; j++)
					{
						n.Nodes.Add(new TreeNode(v.materials.dictPlttToMatList[j].Key, 5, 5) { Tag = v.materials.dictPlttToMatList[j].Value });
					}
				}
			}
			if (mod.TexPlttSet != null)
			{
				treeView1.Nodes.Add(new TreeNode("TEX0", 6, 6));
			}
			treeView1.EndUpdate();
			//LoadTex();
		}

		private void LoadTex()
		{
			TEX0 tt;
			if (mod.TexPlttSet != null) tt = mod.TexPlttSet;
			else if (tex != null) tt = tex.TexPlttSet;
			else return;
			for (int i = 0; i < mod.ModelSet.models.Length; i++)
			{
				for (int j = 0; j < mod.ModelSet.models[i].materials.materials.Length; j++)
				{
					TEX0.DictTexData t = null;
					for (int k = 0; k < mod.ModelSet.models[i].materials.dictTexToMatList.numEntry; k++)
					{
						if (mod.ModelSet.models[i].materials.dictTexToMatList[k].Value.Materials.Contains((byte)j))
						{
							int texid = k;
							for (int l = 0; l < tt.dictTex.numEntry; l++)
							{
								if (tt.dictTex[l].Key == mod.ModelSet.models[i].materials.dictTexToMatList[k].Key) { texid = l; break; }
							}
							t = tt.dictTex[texid].Value;
							break;
						}
					}
					if (t == null)
						continue;
					mod.ModelSet.models[i].materials.materials[j].Fmt = t.Fmt;
					mod.ModelSet.models[i].materials.materials[j].origHeight = t.T;
					mod.ModelSet.models[i].materials.materials[j].origWidth = t.S;
					TEX0.DictPlttData p = null;
					if (t.Fmt != Textures.ImageFormat.DIRECT)
					{
						for (int k = 0; k < mod.ModelSet.models[i].materials.dictPlttToMatList.numEntry; k++)
						{
							if (mod.ModelSet.models[i].materials.dictPlttToMatList[k].Value.Materials.Contains((byte)j))
							{
								int palid = k;
								for (int l = 0; l < tt.dictPltt.numEntry; l++)
								{
									if (tt.dictPltt[l].Key == mod.ModelSet.models[i].materials.dictPlttToMatList[k].Key) { palid = l; break; }
								}
								p = tt.dictPltt[palid].Value;
								break;
							}
						}
					}
					UploadTex(t.ToBitmap(p), mod.ModelSet.models[i].materials.materials[j], j + 1);//+ offset);
				}
			}
		}

		private void UploadTex(Bitmap b, MDL0.Model.MaterialSet.Material m, int Id)
		{
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, Id);
			Gl.glColor3f(1, 1, 1);
			BitmapData d = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA8, b.Width, b.Height, 0, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, d.Scan0);
			b.UnlockBits(d);
			Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
			Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
			bool repeatS = (m.texImageParam >> 16 & 0x1) == 1;
			bool repeatT = (m.texImageParam >> 17 & 0x1) == 1;
			bool flipS = (m.texImageParam >> 18 & 0x1) == 1;
			bool flipT = (m.texImageParam >> 19 & 0x1) == 1;
			int S;
			if (repeatS && flipS)
			{
				S = Gl.GL_MIRRORED_REPEAT;
			}
			else if (repeatS)
			{
				S = Gl.GL_REPEAT;
			}
			else
			{
				S = Gl.GL_CLAMP;
			}
			int T;
			if (repeatT && flipT)
			{
				T = Gl.GL_MIRRORED_REPEAT;
			}
			else if (repeatT)
			{
				T = Gl.GL_REPEAT;
			}
			else
			{
				T = Gl.GL_CLAMP;
			}
			Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, S);
			Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, T);
		}

		private void CGFX_Resize(object sender, EventArgs e)
		{
			if (ModViewer != null)
			{
				ModViewer.Render();
				ModViewer.Render();
			}
		}

		private void CGFX_Layout(object sender, LayoutEventArgs e)
		{
			if (ModViewer != null)
			{
				ModViewer.Render();
			}
		}

		private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (ModViewer != null)
			{
				ModViewer.Render();
			}
		}

		public void FileOpened(ViewableFile File)
		{
			ViewableFile[] v = EveryFileExplorerUtil.GetOpenFilesOfType(typeof(NSBTX));
			menuItem1.MenuItems.Clear();
			bool curavab = false;
			foreach (var vv in v)
			{
				var m = menuItem1.MenuItems.Add(vv.File.Name);
				if (vv.FileFormat == tex)
				{
					curavab = true;
					m.Checked = true;
				}
			}
			if (!curavab && v.Length != 0)
			{
				menuItem1.MenuItems[0].Checked = true;
				tex = v[0].FileFormat;
			}
			LoadTex();
			if (ModViewer != null)
			{
				ModViewer.Render();
				ModViewer.Render();
			}
		}

		public void FileClosed(ViewableFile File)
		{
			if (File.FileFormat is NSBTX && File.FileFormat == tex) tex = null;
			ViewableFile[] v = EveryFileExplorerUtil.GetOpenFilesOfType(typeof(NSBTX));
			menuItem1.MenuItems.Clear();
			foreach (var vv in v)
			{
				menuItem1.MenuItems.Add(vv.File.Name);
			}
			if (v.Length != 0)
			{
				menuItem1.MenuItems[0].Checked = true;
				tex = v[0].FileFormat;
			}
			LoadTex();
			if (ModViewer != null)
			{
				ModViewer.Render();
				ModViewer.Render();
			}
		}

		private void NSBMDViewer_Activated(object sender, EventArgs e)
		{
			//otherwise the nsbtx menu is not refreshed!
			mainMenu1.MenuItems.Clear();
			mainMenu1.MenuItems.Add(menuItem1);
		}

		private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			switch (e.Node.ImageIndex)
			{
				case 1:
					ModViewer = new MDL0Viewer((MDL0.Model)e.Node.Tag) { Dock = DockStyle.Fill };
					ModViewer.Width = panel1.Width;
					ModViewer.Height = panel1.Height;
					panel1.SuspendLayout();
					panel1.Controls.Add(ModViewer);
					if (panel1.Controls.Count > 1) panel1.Controls.RemoveAt(0);
					panel1.Invalidate();
					panel1.ResumeLayout();
					ModViewer.Invalidate();
					LoadTex();
					ModViewer.Render();
					ModViewer.Render();
					break;
				case 2:
					ModViewer = null;
					panel1.SuspendLayout();
					panel1.Controls.Add(new MDL0MaterialEditor((MDL0.Model.MaterialSet.Material)e.Node.Tag) { Dock = DockStyle.Fill, Width = panel1.Width, Height = panel1.Height });
					if (panel1.Controls.Count > 1) panel1.Controls.RemoveAt(0);
					panel1.Invalidate();
					panel1.ResumeLayout();
					break;
				case 6:
					ModViewer = null;
					panel1.SuspendLayout();
					panel1.Controls.Add(new TEX0Viewer(mod.TexPlttSet) { Dock = DockStyle.Fill, Width = panel1.Width, Height = panel1.Height });
					if (panel1.Controls.Count > 1) panel1.Controls.RemoveAt(0);
					panel1.Invalidate();
					panel1.ResumeLayout();
					break;
				default:
					ModViewer = null;
					panel1.SuspendLayout();
					panel1.Controls.Clear();
					panel1.Invalidate();
					panel1.ResumeLayout();
					break;
			}
		}
	}
}
