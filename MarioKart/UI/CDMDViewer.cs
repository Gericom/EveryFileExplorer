using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using Tao.OpenGl;
using System.Drawing.Imaging;
using LibEveryFileExplorer._3D;
using LibEveryFileExplorer;
using LibEveryFileExplorer.UI;
using LibEveryFileExplorer.GameData;
using MarioKart.MK7;
using LibEveryFileExplorer.Collections;
using MarioKart.MK7.KMP;

namespace MarioKart.UI
{
	public partial class CDMDViewer : Form, IUseOtherFiles
	{
		List<IGameDataSectionViewer> SectionViewers = new List<IGameDataSectionViewer>();

		CDMD MapData;
		KCL KCL = null;
		public CDMDViewer(CDMD MapData)
		{
			this.MapData = MapData;
			InitializeComponent();
			simpleOpenGlControl1.InitializeContexts();
			simpleOpenGlControl1.MouseWheel += new MouseEventHandler(simpleOpenGlControl1_MouseWheel);
		}

		int scale = 1;
		bool first = true;
		void simpleOpenGlControl1_MouseWheel(object sender, MouseEventArgs e)
		{
			if (!(e.Delta < 0 && scale == 1) && !(e.Delta > 0 && scale == 32))
			{
				scale = (int)(scale * (e.Delta < 0 ? 1f / 2f : 2));
				vScrollBar1.Maximum = scale - 1;
				vScrollBar1.Minimum = -(scale - 1);
				hScrollBar1.Maximum = scale - 1;
				hScrollBar1.Minimum = -(scale - 1);
				if (scale == 1) { vScrollBar1.Enabled = false; hScrollBar1.Enabled = false; }
				else { vScrollBar1.Enabled = true; hScrollBar1.Enabled = true; }
				Render();
			}
		}
		bool init = false;
		private void NKMDViewer_Load(object sender, EventArgs e)
		{
			Gl.glEnable(Gl.GL_COLOR_MATERIAL);
			Gl.glEnable(Gl.GL_DEPTH_TEST);
			Gl.glDepthFunc(Gl.GL_ALWAYS);
			Gl.glEnable(Gl.GL_LOGIC_OP);
			Gl.glDisable(Gl.GL_CULL_FACE);
			Gl.glEnable(Gl.GL_TEXTURE_2D);
			Gl.glEnable(Gl.GL_LINE_SMOOTH);
			Gl.glEnable(Gl.GL_BLEND);

			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

			ViewableFile[] v = EveryFileExplorerUtil.GetOpenFilesOfType(typeof(KCL));
			menuItem1.MenuItems.Clear();
			foreach (var vv in v)
			{
				menuItem1.MenuItems.Add(vv.File.Name);
			}
			if (v.Length != 0)
			{
				menuItem1.MenuItems[0].Checked = true;
				KCL = v[0].FileFormat;
			}

			if (MapData.CheckPoint != null) AddTab<CKPT.CKPTEntry>("CKPT", MapData.CheckPoint);
			if (MapData.CheckPointPath != null) AddTab<CKPH.CKPHEntry>("CKPH", MapData.CheckPointPath);
			if (MapData.GlobalObject != null) AddTab<GOBJ.GOBJEntry>("GOBJ", MapData.GlobalObject);
			if (MapData.JugemPoint != null) AddTab<JGPT.JGPTEntry>("JGPT", MapData.JugemPoint);
			if (MapData.GliderPoint != null) AddTab<GLPT.GLPTEntry>("GLPT", MapData.GliderPoint);
			if (MapData.GliderPointPath != null) AddTab<GLPH.GLPHEntry>("GLPH", MapData.GliderPointPath);

			/*Bitmap b3 = OBJI.OBJ_2D01;
			System.Resources.ResourceSet s = OBJI.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, false, false);
			foreach (Object b in s)
			{
				Bitmap b2 = ((Bitmap)((System.Collections.DictionaryEntry)b).Value);
				BitmapData bd = b2.LockBits(new Rectangle(0, 0, b2.Width, b2.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);
				if ((String)((System.Collections.DictionaryEntry)b).Key != "start")
				{
					Gl.glBindTexture(Gl.GL_TEXTURE_2D, BitConverter.ToUInt16(BitConverter.GetBytes(ushort.Parse(((String)((System.Collections.DictionaryEntry)b).Key).Split('_')[1], System.Globalization.NumberStyles.HexNumber)).Reverse().ToArray(), 0));
				}
				else
				{
					Gl.glBindTexture(Gl.GL_TEXTURE_2D, -1);
				}
				Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA8, b2.Width, b2.Height, 0, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, bd.Scan0);
				b2.UnlockBits(bd);
				Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP);
				Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP);
				Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
				Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
			}*/
			init = true;
			Render();
			Render();
		}

		private void AddTab<T>(String Name, GameDataSection<T> Section) where T : GameDataSectionEntry, new()
		{
			TabPage p = new TabPage(Name);
			var v = new GameDataSectionViewer<T>(Section) { Dock = DockStyle.Fill };
			v.OnSelected += new SelectedEventHandler(GameDataSectionViewer_OnSelected);
			SectionViewers.Add(v);
			p.Controls.Add(v);
			tabControl1.TabPages.Add(p);
		}

		void GameDataSectionViewer_OnSelected(IGameDataSectionViewer Viewer, object Entry)
		{
			propertyGrid1.SelectedObject = Entry;
			propertyGrid1.ExpandAllGridItems();
			foreach (var v in SectionViewers)
			{
				if (v != Viewer) v.RemoveSelection();
			}
		}

		float min = -8192f;
		float max = 8192f;
		byte[] pic;
		float mult = 0;
		private void Render(bool pick = false, Point mousepoint = new Point(), bool kclpick = false)
		{
			if (!init) return;
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glLoadIdentity();
			Gl.glViewport(0, 0, simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);
			float x = (8192f / (float)scale) / simpleOpenGlControl1.Width;
			x *= 2;
			float y = (8192f / (float)scale) / simpleOpenGlControl1.Height;
			y *= 2;
			//Gl.glTranslatef(0, 0, 0);
			//Gl.glOrtho(-8192, 8192, 8192, -8192, -1000, 1000);
			if (x > y)
			{
				Gl.glOrtho((-(x * simpleOpenGlControl1.Width) / 2f) + (hScrollBar1.Value * (8192f / (float)scale)), (x * simpleOpenGlControl1.Width) / 2f + (hScrollBar1.Value * (8192f / (float)scale)), (x * simpleOpenGlControl1.Height) / 2f + (vScrollBar1.Value * (8192f / (float)scale)), (-(x * simpleOpenGlControl1.Height) / 2f) + (vScrollBar1.Value * (8192f / (float)scale)), -8192, 8192);
				mult = x;
			}
			else
			{
				Gl.glOrtho((-(y * simpleOpenGlControl1.Width) / 2f) + (hScrollBar1.Value * (8192f / (float)scale)), (y * simpleOpenGlControl1.Width) / 2f + (hScrollBar1.Value * (8192f / (float)scale)), (y * simpleOpenGlControl1.Height) / 2f + (vScrollBar1.Value * (8192f / (float)scale)), (-(y * simpleOpenGlControl1.Height) / 2f) + (vScrollBar1.Value * (8192f / (float)scale)), -8192, 8192);
				mult = y;
			}

			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glLoadIdentity();
			Gl.glClearColor(1, 1, 1, 1f);
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			Gl.glColor4f(1, 1, 1, 1);
			Gl.glEnable(Gl.GL_TEXTURE_2D);
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
			Gl.glColor4f(1, 1, 1, 1);
			Gl.glDisable(Gl.GL_CULL_FACE);
			Gl.glEnable(Gl.GL_ALPHA_TEST);
			Gl.glEnable(Gl.GL_BLEND);
			Gl.glEnable(Gl.GL_POINT_SMOOTH);
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

			Gl.glAlphaFunc(Gl.GL_ALWAYS, 0f);

			if (pick)
			{
				Gl.glLoadIdentity();
				Gl.glDisable(Gl.GL_POLYGON_SMOOTH);
				Gl.glDisable(Gl.GL_POINT_SMOOTH);
				if (!kclpick) RenderNKM(true);
				else
				{
					Gl.glDepthFunc(Gl.GL_LEQUAL);
					RenderKCL(true);
					Gl.glDepthFunc(Gl.GL_ALWAYS);
				}
				pic = new byte[4];
				Gl.glReadPixels(mousepoint.X, (int)simpleOpenGlControl1.Height - mousepoint.Y, 1, 1, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, pic);
				Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
				Render();
				return;
			}
			else
			{
				Gl.glLoadIdentity();
				RenderNKM();
			}

			simpleOpenGlControl1.Refresh();
		}
		private void RenderNKM(bool picking = false)
		{
			if (first && KCL != null)
			{
				first = false;
			}
			if (!picking)
			{
				Gl.glEnable(Gl.GL_POLYGON_SMOOTH);
			}
			if (!picking && KCL != null)
			{
				Gl.glDepthFunc(Gl.GL_LEQUAL);
				RenderKCL();
				Gl.glDepthFunc(Gl.GL_ALWAYS);
			}
			Gl.glPointSize((picking ? 6f : 5));

			int objidx = 1;
			if (!picking)
			{
				Gl.glColor4f(Color.CornflowerBlue.R / 255f, Color.CornflowerBlue.G / 255f, Color.CornflowerBlue.B / 255f, 0.25f);
			}
			Gl.glBegin(Gl.GL_QUADS);
			//if (aREAToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.Area.Entries)
				{
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (17 << 18)).R / 255f, Color.FromArgb(objidx | (17 << 18)).G / 255f, Color.FromArgb(objidx | (17 << 18)).B / 255f, 1);
						objidx++;
					}
					Vector3[] cube = o.GetCube();
					//We're interested in points 0, 1, 5 and 3 (ground plane)
					Vector3 Point1 = cube[3];
					Vector3 Point2 = cube[5];
					Vector3 Point3 = cube[1];
					Vector3 Point4 = cube[0];
					Gl.glVertex2f(Point1.X, Point1.Z);
					Gl.glVertex2f(Point2.X, Point2.Z);
					Gl.glVertex2f(Point3.X, Point3.Z);
					Gl.glVertex2f(Point4.X, Point4.Z);
				}
			}*/
			Gl.glEnd();

			Gl.glBegin(Gl.GL_POINTS);
			if (!picking)
			{
				Gl.glColor3f(0, 0, 0.5f);
			}
			objidx = 1;
			//if (pOITToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.Point.Entries)
				{
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (2 << 18)).R / 255f, Color.FromArgb(objidx | (2 << 18)).G / 255f, Color.FromArgb(objidx | (2 << 18)).B / 255f, 1);
						objidx++;
					}
					Gl.glVertex2f(o.Position.X, o.Position.Z);
				}
			}*/
			Gl.glEnd();
			Gl.glLineWidth(1.5f);
			int idx = 0;
			//if (pOITToolStripMenuItem.Checked)
			/*{
				if (!picking)
				{
					foreach (var o in NKMD.Path.Entries)
					{
						if (NKMD.Point.NrEntries < o.NrPoit + idx) break;
						Gl.glBegin(Gl.GL_LINE_STRIP);
						for (int i = 0; i < o.NrPoit; i++)
						{
							Gl.glVertex2f(NKMD.Point[idx + i].Position.X, NKMD.Point[idx + i].Position.Z);
							if (!(i + 1 < o.NrPoit) && o.Loop)
							{
								Gl.glVertex2f(NKMD.Point[idx].Position.X, NKMD.Point[idx].Position.Z);
							}
						}
						Gl.glEnd();
						idx += o.NrPoit;
					}
				}
			}*/
			Gl.glBegin(Gl.GL_POINTS);
			if (!picking)
			{
				Gl.glColor3f(1, 0, 0);
			}
			objidx = 1;
			//if (oBJIToolStripMenuItem.Checked)
			{
				foreach (var o in MapData.GlobalObject.Entries)
				{
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (0 << 18)).R / 255f, Color.FromArgb(objidx | (0 << 18)).G / 255f, Color.FromArgb(objidx | (0 << 18)).B / 255f, 1);
						objidx++;
					}
					//Bitmap b;
					//if ((b = (Bitmap)OBJI.ResourceManager.GetObject("OBJ_" + BitConverter.ToString(BitConverter.GetBytes(o.ObjectID), 0).Replace("-", ""))) == null)
					//{
						Gl.glVertex2f(o.Position.X, o.Position.Z);
					/*}
					else
					{
						Gl.glEnd();
						if (!picking)
						{
							Gl.glColor3f(1, 1, 1);
							Gl.glBindTexture(Gl.GL_TEXTURE_2D, o.ObjectID);
						}
						Gl.glPushMatrix();
						Gl.glTranslatef(o.Position.X, o.Position.Z, 0);

						Gl.glRotatef(o.Rotation.Y, 0, 0, 1);

						Gl.glScalef(mult, mult, 1);

						Gl.glBegin(Gl.GL_QUADS);
						Gl.glTexCoord2f(0, 0);
						Gl.glVertex2f(-b.Width / 2f, -b.Height / 2f);
						Gl.glTexCoord2f(1, 0);
						Gl.glVertex2f(b.Width / 2f, -b.Height / 2f);
						Gl.glTexCoord2f(1, 1);
						Gl.glVertex2f(b.Width / 2f, b.Height / 2f);
						Gl.glTexCoord2f(0, 1);
						Gl.glVertex2f(-b.Width / 2f, b.Height / 2f);
						Gl.glEnd();
						Gl.glPopMatrix();
						if (!picking)
						{
							Gl.glColor3f(1, 0, 0);
							Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
						}
						Gl.glBegin(Gl.GL_POINTS);
					}*/
				}
			}
			if (!picking)
			{
				Gl.glColor3f(0, 0, 0);
			}
			objidx = 1;
			//if (kTPSToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.KartPointStart.Entries)
				{
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (4 << 18)).R / 255f, Color.FromArgb(objidx | (4 << 18)).G / 255f, Color.FromArgb(objidx | (4 << 18)).B / 255f, 1);
						objidx++;
					}
					Bitmap b;
					if ((b = (Bitmap)OBJI.ResourceManager.GetObject("start")) == null)
					{
						Gl.glVertex2f(o.Position.X, o.Position.Z);
					}
					else
					{
						Gl.glEnd();
						if (!picking)
						{
							Gl.glColor3f(1, 1, 1);
							Gl.glBindTexture(Gl.GL_TEXTURE_2D, -1);
						}
						Gl.glPushMatrix();
						Gl.glTranslatef(o.Position.X, o.Position.Z, 0);

						Gl.glRotatef(o.Rotation.Y, 0, 0, 1);

						Gl.glScalef(mult, mult, 1);

						Gl.glBegin(Gl.GL_QUADS);
						Gl.glTexCoord2f(0, 0);
						Gl.glVertex2f(-b.Width / 2f, -b.Height / 2f);
						Gl.glTexCoord2f(1, 0);
						Gl.glVertex2f(b.Width / 2f, -b.Height / 2f);
						Gl.glTexCoord2f(1, 1);
						Gl.glVertex2f(b.Width / 2f, b.Height / 2f);
						Gl.glTexCoord2f(0, 1);
						Gl.glVertex2f(-b.Width / 2f, b.Height / 2f);
						Gl.glEnd();
						Gl.glPopMatrix();
						if (!picking)
						{
							Gl.glColor3f(1, 0, 0);
							Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
						}
						Gl.glBegin(Gl.GL_POINTS);
					}
				}
			}*/
			if (!picking)
			{
				Gl.glColor3f(1, 0, 0.5f);
			}
			objidx = 1;
			//if (kTPCToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.KartPointCannon.Entries)
				{
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (7 << 18)).R / 255f, Color.FromArgb(objidx | (7 << 18)).G / 255f, Color.FromArgb(objidx | (7 << 18)).B / 255f, 1);
						objidx++;
					}
					Gl.glVertex2f(o.Position.X, o.Position.Z);
				}
			}*/
			if (!picking)
			{
				Gl.glColor3f(0, 0.9f, 1);
			}
			objidx = 1;
			//if (kTP2ToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.KartPointSecond.Entries)
				{
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (6 << 18)).R / 255f, Color.FromArgb(objidx | (6 << 18)).G / 255f, Color.FromArgb(objidx | (6 << 18)).B / 255f, 1);
						objidx++;
					}
					Gl.glVertex2f(o.Position.X, o.Position.Z);
				}
			}*/
			if (!picking)
			{
				Gl.glColor3f(Color.MediumPurple.R / 255f, Color.MediumPurple.G / 255f, Color.MediumPurple.B / 255f);
			}
			objidx = 1;
			//if (kTPMToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.KartPointMission.Entries)
				{
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (8 << 18)).R / 255f, Color.FromArgb(objidx | (8 << 18)).G / 255f, Color.FromArgb(objidx | (8 << 18)).B / 255f, 1);
						objidx++;
					}
					Gl.glVertex2f(o.Position.X, o.Position.Z);
				}
			}*/
			if (!picking)
			{
				Gl.glColor3f(1, 0.6f, 0);
			}
			objidx = 1;
			//if (kTPJToolStripMenuItem.Checked)
			{
				foreach (var o in MapData.JugemPoint.Entries)
				{
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (5 << 18)).R / 255f, Color.FromArgb(objidx | (5 << 18)).G / 255f, Color.FromArgb(objidx | (5 << 18)).B / 255f, 1);
						objidx++;
					}
					Gl.glVertex2f(o.Position.X, o.Position.Z);
				}
			}
			if (!picking)
			{
				Gl.glColor3f(0, 0.8f, 0);
			}
			objidx = 1;
			//if (ePOIToolStripMenuItem.Checked)
			/*{
				if (NKMD.EnemyPoint != null)
				{
					foreach (var o in NKMD.EnemyPoint.Entries)
					{
						if (picking)
						{
							Gl.glColor4f(Color.FromArgb(objidx | (13 << 18)).R / 255f, Color.FromArgb(objidx | (13 << 18)).G / 255f, Color.FromArgb(objidx | (13 << 18)).B / 255f, 1);
							objidx++;
						}
						Gl.glVertex2f(o.Position.X, o.Position.Z);
					}
				}
				else
				{
					foreach (var o in NKMD.MiniGameEnemyPoint.Entries)
					{
						if (picking)
						{
							Gl.glColor4f(Color.FromArgb(objidx | (15 << 18)).R / 255f, Color.FromArgb(objidx | (15 << 18)).G / 255f, Color.FromArgb(objidx | (15 << 18)).B / 255f, 1);
							objidx++;
						}
						Gl.glVertex2f(o.Position.X, o.Position.Z);
					}
				}
			}*/

			if (!picking)
			{
				Gl.glColor3f(1, 0.9f, 0);
			}
			objidx = 1;
			//if (iPOIToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.ItemPoint.Entries)
				{
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (11 << 18)).R / 255f, Color.FromArgb(objidx | (11 << 18)).G / 255f, Color.FromArgb(objidx | (11 << 18)).B / 255f, 1);
						objidx++;
					}
					Gl.glVertex2f(o.Position.X, o.Position.Z);
				}
			}*/
			if (!picking)
			{
				Gl.glColor3f(Color.CornflowerBlue.R / 255f, Color.CornflowerBlue.G / 255f, Color.CornflowerBlue.B / 255f);
			}
			objidx = 1;
			//if (aREAToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.Area.Entries)
				{
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (17 << 18)).R / 255f, Color.FromArgb(objidx | (17 << 18)).G / 255f, Color.FromArgb(objidx | (17 << 18)).B / 255f, 1);
						objidx++;
					}
					Gl.glVertex2f(o.Position.X, o.Position.Z);
				}
			}*/
			if (!picking)
			{
				Gl.glColor3f(Color.BurlyWood.R / 255f, Color.BurlyWood.G / 255f, Color.BurlyWood.B / 255f);
			}
			objidx = 1;
			//if (cAMEToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.Camera.Entries)
				{
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (18 << 18)).R / 255f, Color.FromArgb(objidx | (18 << 18)).G / 255f, Color.FromArgb(objidx | (18 << 18)).B / 255f, 1);
					}
					Gl.glVertex2f(o.Position.X, o.Position.Z);
					if (o.CameraType == 3 || o.CameraType == 4)
					{
						if (picking)
						{
							Gl.glColor4f(Color.FromArgb(objidx | (19 << 18)).R / 255f, Color.FromArgb(objidx | (19 << 18)).G / 255f, Color.FromArgb(objidx | (19 << 18)).B / 255f, 1);
						}
						Gl.glVertex2f(o.Viewpoint1.X, o.Viewpoint1.Z);
						if (picking)
						{
							Gl.glColor4f(Color.FromArgb(objidx | (20 << 18)).R / 255f, Color.FromArgb(objidx | (20 << 18)).G / 255f, Color.FromArgb(objidx | (20 << 18)).B / 255f, 1);
							objidx++;
						}
						Gl.glVertex2f(o.Viewpoint2.X, o.Viewpoint2.Z);
					}
				}
			}*/



			Gl.glEnd();

			//if (cPOIToolStripMenuItem.Checked)
			{
				if (!picking)
				{
					Gl.glBegin(Gl.GL_LINES);
					foreach (var o in MapData.CheckPoint.Entries)
					{
						Gl.glColor3f(0, 170f / 255f, 0);
						//Gl.glColor3f(0.5f, 0.5f, 0.5f);
						Gl.glVertex2f(o.Point1.X, o.Point1.Y);
						Gl.glColor3f(170f / 255f, 0, 0);//181f / 255f, 230f / 255f, 29f / 255f);
						//Gl.glColor3f(1, 1, 1);
						Gl.glVertex2f(o.Point2.X, o.Point2.Y);
					}
					for (int j = 0; j < MapData.CheckPointPath.Entries.Count; j++)
					{
						if (MapData.CheckPoint.Entries.Count < MapData.CheckPointPath[j].Start + MapData.CheckPointPath[j].Length) break;
						for (int i = MapData.CheckPointPath[j].Start; i < MapData.CheckPointPath.Entries[j].Start + MapData.CheckPointPath[j].Length - 1; i++)
						{
							Gl.glColor3f(0, 170f / 255f, 0);
							Gl.glVertex2f(MapData.CheckPoint[i].Point1.X, MapData.CheckPoint[i].Point1.Y);
							Gl.glVertex2f(MapData.CheckPoint[i + 1].Point1.X, MapData.CheckPoint[i + 1].Point1.Y);
							Gl.glColor3f(170f / 255f, 0, 0);
							Gl.glVertex2f(MapData.CheckPoint[i].Point2.X, MapData.CheckPoint[i].Point2.Y);
							Gl.glVertex2f(MapData.CheckPoint[i + 1].Point2.X, MapData.CheckPoint[i + 1].Point2.Y);
						}

						/*for (int i = 0; i < 3; i++)
						{
							if (MapData.CheckPointPath[j].Next[i] == -1 || MapData.CheckPointPath[j].Next[i] >= MapData.CheckPointPath.Entries.Count) continue;
							Gl.glColor3f(0, 170f / 255f, 0);
							Gl.glVertex2f(MapData.CheckPoint[MapData.CheckPointPath[j].Start + MapData.CheckPointPath[j].Length - 1].Point1.X, MapData.CheckPoint[MapData.CheckPointPath[j].Start + MapData.CheckPointPath[j].Length - 1].Point1.Y);
							Gl.glVertex2f(MapData.CheckPoint[MapData.CheckPointPath[MapData.CheckPointPath[j].Next[i]].Start].Point1.X, MapData.CheckPoint[MapData.CheckPointPath[MapData.CheckPointPath[j].Next[i]].Start].Point1.Y);
							Gl.glColor3f(170f / 255f, 0, 0);
							Gl.glVertex2f(MapData.CheckPoint[MapData.CheckPointPath[j].Start + MapData.CheckPointPath[j].Length - 1].Point2.X, MapData.CheckPoint[MapData.CheckPointPath[j].Start + MapData.CheckPointPath[j].Length - 1].Point2.Y);
							Gl.glVertex2f(MapData.CheckPoint[MapData.CheckPointPath[MapData.CheckPointPath[j].Next[i]].Start].Point2.X, MapData.CheckPoint[MapData.CheckPointPath[MapData.CheckPointPath[j].Next[i]].Start].Point2.Y);
						}*/
					}
					Gl.glEnd();
				}
			}

			Gl.glBegin(Gl.GL_POINTS);
			objidx = 1;
			//if (cPOIToolStripMenuItem.Checked)
			{
				foreach (var o in MapData.CheckPoint.Entries)
				{
					if (!picking)
					{
						Gl.glColor3f(0, 170f / 255f, 0);
					}
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (21 << 18)).R / 255f, Color.FromArgb(objidx | (21 << 18)).G / 255f, Color.FromArgb(objidx | (21 << 18)).B / 255f, 1);
					}
					Gl.glVertex2f(o.Point1.X, o.Point1.Y);
					if (!picking)
					{
						Gl.glColor3f(170f / 255f, 0, 0);//181f / 255f, 230f / 255f, 29f / 255f);
					}
					if (picking)
					{
						Gl.glColor4f(Color.FromArgb(objidx | (22 << 18)).R / 255f, Color.FromArgb(objidx | (22 << 18)).G / 255f, Color.FromArgb(objidx | (22 << 18)).B / 255f, 1);
						objidx++;
					}
					Gl.glVertex2f(o.Point2.X, o.Point2.Y);
				}
			}
			Gl.glEnd();
			//Gl.glEnable(Gl.GL_LINE_SMOOTH);
		}

		public void RenderKCL(bool picking = false)
		{
			int i = 0;
			foreach (var p in KCL.Planes)
			{
				//Vector3 PositionA, PositionB, PositionC, Normal;
				Triangle t = KCL.GetTriangle(p);

				Color c = Color.Gray;//MKDS.KCL.GetColor(p.CollisionType);
				if (picking && c.A != 0) c = Color.FromArgb(i + 1 | 0xFF << 24);
				Gl.glColor4f(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
				Gl.glBegin(Gl.GL_TRIANGLES);
				//Gl.glNormal3f(t.Normal.X, t.Normal.Y, t.Normal.Z);
				Gl.glVertex3f(t.PointA.X, t.PointA.Z, t.PointA.Y);
				Gl.glVertex3f(t.PointB.X, t.PointB.Z, t.PointB.Y);
				Gl.glVertex3f(t.PointC.X, t.PointC.Z, t.PointC.Y);
				Gl.glEnd();
				i++;
			}
		}

		private void simpleOpenGlControl1_Resize(object sender, EventArgs e)
		{
			Render();
			Render();
		}

		private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPageIndex == 0)
			{
				Render();
				simpleOpenGlControl1.Focus();
				simpleOpenGlControl1.Select();
			}
		}

		private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
		{
			Render();
		}

		public void FileOpened(ViewableFile File)
		{
			ViewableFile[] v = EveryFileExplorerUtil.GetOpenFilesOfType(typeof(KCL));
			menuItem1.MenuItems.Clear();
			bool curavab = false;
			foreach (var vv in v)
			{
				var m = menuItem1.MenuItems.Add(vv.File.Name);
				if (vv.FileFormat == KCL)
				{
					curavab = true;
					m.Checked = true;
				}
			}
			if (!curavab && v.Length != 0)
			{
				menuItem1.MenuItems[0].Checked = true;
				KCL = v[0].FileFormat;
			}
			Render();
			Render();
		}

		public void FileClosed(ViewableFile File)
		{
			if (File.FileFormat is KCL && File.FileFormat == KCL) KCL = null;
			ViewableFile[] v = EveryFileExplorerUtil.GetOpenFilesOfType(typeof(KCL));
			menuItem1.MenuItems.Clear();
			foreach (var vv in v)
			{
				menuItem1.MenuItems.Add(vv.File.Name);
			}
			if (v.Length != 0)
			{
				menuItem1.MenuItems[0].Checked = true;
				KCL = v[0].FileFormat;
			}
			Render();
			Render();
		}

		private void NKMDViewer_Shown(object sender, EventArgs e)
		{
			Render();
			Render();
		}

		private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			foreach (var v in SectionViewers) v.UpdateListViewEntry(propertyGrid1.SelectedObject);
			Render();
			Render();
		}
	}
}
