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
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer;
using System.Runtime.InteropServices;
using _3DS.NintendoWare.GFX;

namespace _3DS.UI
{
	public partial class CGFXViewer : Form
	{
		ImageList ImageL;
		CMDLViewer ModViewer = null;

		CGFX mod;

		public CGFXViewer(CGFX mod)
		{
			this.mod = mod;
			InitializeComponent();
			Win32Util.SetWindowTheme(treeView1.Handle, "explorer", null);
			ImageL = new ImageList();
			ImageL.ColorDepth = ColorDepth.Depth32Bit;
			ImageL.ImageSize = new Size(16, 16);
			ImageL.Images.Add(Resource.jar);
			ImageL.Images.Add(Resource.point);
			ImageL.Images.Add(Resource.images_stack);
			ImageL.Images.Add(Resource.image_sunset);
			ImageL.Images.Add(Resource.films);
			ImageL.Images.Add(Resource.film);
			ImageL.Images.Add(Resource.tables_stacks);
			ImageL.Images.Add(Resource.table);
			ImageL.Images.Add(Resource.weather_clouds);
			ImageL.Images.Add(Resource.weather_cloud);
			ImageL.Images.Add(Resource.lighthouse_shine);
			ImageL.Images.Add(Resource.light_bulb);
			treeView1.ImageList = ImageL;
		}


		private void CGFX_Load(object sender, EventArgs e)
		{
			bool sel = false;
			treeView1.BeginUpdate();
			treeView1.Nodes.Clear();
			if (mod.Data.Models != null)
			{
				TreeNode section = new TreeNode("Models", 0, 0);
				treeView1.Nodes.Add(section);
				for (int i = 0; i < mod.Data.Models.Length; i++)
				{
					TreeNode entry = new TreeNode(mod.Data.Models[i].Name, 1, 1) { Tag = mod.Data.Models[i] };
					section.Nodes.Add(entry);
					if (i == 0 && !sel)
					{
						section.Expand();
						treeView1.SelectedNode = entry;
						sel = true;
					}
					//TODO: Content of CMDL
				}
			}
			if (mod.Data.Textures != null)
			{
				TreeNode section = new TreeNode("Textures", 2, 2);
				treeView1.Nodes.Add(section);
				for (int i = 0; i < mod.Data.Textures.Length; i++)
				{
					TreeNode entry = new TreeNode(mod.Data.Textures[i].Name, 3, 3) { Tag = mod.Data.Textures[i] };
					section.Nodes.Add(entry);
					if (i == 0 && !sel)
					{
						section.Expand();
						treeView1.SelectedNode = entry;
						sel = true;
					}
				}
			}
			if (mod.Data.Dictionaries[2] != null)
			{
				TreeNode section = new TreeNode("Lookup Tables", 6, 6);
				treeView1.Nodes.Add(section);
				for (int i = 0; i < mod.Data.Dictionaries[2].Count; i++)
				{
					TreeNode entry = new TreeNode(mod.Data.Dictionaries[2][i].Name, 7, 7);// { Tag = mod.Data.Textures[i] };
					section.Nodes.Add(entry);
				}
			}
			if (mod.Data.Dictionaries[6] != null)
			{
				TreeNode section = new TreeNode("Lights", 10, 10);
				treeView1.Nodes.Add(section);
				for (int i = 0; i < mod.Data.Dictionaries[6].Count; i++)
				{
					TreeNode entry = new TreeNode(mod.Data.Dictionaries[6][i].Name, 11, 11);// { Tag = mod.Data.Textures[i] };
					section.Nodes.Add(entry);
					if (i == 0 && !sel)
					{
						section.Expand();
						treeView1.SelectedNode = entry;
						sel = true;
					}
				}
			}
			if (mod.Data.Dictionaries[7] != null)
			{
				TreeNode section = new TreeNode("Fog", 8, 8);
				treeView1.Nodes.Add(section);
				for (int i = 0; i < mod.Data.Dictionaries[7].Count; i++)
				{
					TreeNode entry = new TreeNode(mod.Data.Dictionaries[7][i].Name, 9, 9);// { Tag = mod.Data.Textures[i] };
					section.Nodes.Add(entry);
					if (i == 0 && !sel)
					{
						section.Expand();
						treeView1.SelectedNode = entry;
						sel = true;
					}
				}
			}
			if (mod.Data.SkeletonAnimations != null)
			{
				TreeNode section = new TreeNode("Skeleton Animations", 4, 4);
				treeView1.Nodes.Add(section);
				for (int i = 0; i < mod.Data.SkeletonAnimations.Length; i++)
				{
					TreeNode entry = new TreeNode(mod.Data.SkeletonAnimations[i].Name, 5, 5) { Tag = mod.Data.SkeletonAnimations[i] };
					section.Nodes.Add(entry);
					if (i == 0 && !sel)
					{
						section.Expand();
						treeView1.SelectedNode = entry;
						sel = true;
					}
				}
			}
			if (mod.Data.MaterialAnimations != null)
			{
				TreeNode section = new TreeNode("Material Animations", 4, 4);
				treeView1.Nodes.Add(section);
				for (int i = 0; i < mod.Data.MaterialAnimations.Length; i++)
				{
					TreeNode entry = new TreeNode(mod.Data.MaterialAnimations[i].Name, 5, 5) { Tag = mod.Data.MaterialAnimations[i] };
					section.Nodes.Add(entry);
					if (i == 0 && !sel)
					{
						section.Expand();
						treeView1.SelectedNode = entry;
						sel = true;
					}
				}
			}
			if (mod.Data.VisibilityAnimations != null)
			{
				TreeNode section = new TreeNode("Visibility Animations", 4, 4);
				treeView1.Nodes.Add(section);
				for (int i = 0; i < mod.Data.VisibilityAnimations.Length; i++)
				{
					TreeNode entry = new TreeNode(mod.Data.VisibilityAnimations[i].Name, 5, 5) { Tag = mod.Data.VisibilityAnimations[i] };
					section.Nodes.Add(entry);
					if (i == 0 && !sel)
					{
						section.Expand();
						treeView1.SelectedNode = entry;
						sel = true;
					}
				}
			}
			treeView1.EndUpdate();
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

		private void NSBMDViewer_Activated(object sender, EventArgs e)
		{
			//render it multiple times to avoid glitches!
			if (ModViewer != null)
			{
				for (int i = 0; i < 8; i++) ModViewer.Render();
			}
		}

		private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			switch (e.Node.ImageIndex)
			{
				case 1:
					ModViewer = new CMDLViewer(mod, (CMDL)e.Node.Tag) { Dock = DockStyle.Fill };
					ModViewer.Width = panel1.Width;
					ModViewer.Height = panel1.Height;
					panel1.SuspendLayout();
					panel1.Controls.Add(ModViewer);
					if (panel1.Controls.Count > 1) panel1.Controls.RemoveAt(0);
					panel1.Invalidate();
					panel1.ResumeLayout();
					ModViewer.Invalidate();
					ModViewer.Render();
					ModViewer.Render();
					break;
				case 3:
					ModViewer = null;
					panel1.SuspendLayout();
					panel1.Controls.Add(new TXOBViewer((ImageTextureCtr)e.Node.Tag) { Dock = DockStyle.Fill, Width = panel1.Width, Height = panel1.Height });
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

		private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (ModViewer != null)
			{
				ModViewer.Render();
			}
		}
	}
}
