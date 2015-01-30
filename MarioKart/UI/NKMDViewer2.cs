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
using MarioKart.MKDS;
using MarioKart.MKDS.NKM;
using LibEveryFileExplorer.Collections;
using MarioKart.UI.MapViewer;

namespace MarioKart.UI
{
	public partial class NKMDViewer2 : Form, IUseOtherFiles
	{
		List<IGameDataSectionViewer> SectionViewers = new List<IGameDataSectionViewer>();

		NKMD NKMD;
		KCL KCL = null;
		public NKMDViewer2(NKMD NKMD)
		{
			this.NKMD = NKMD;
			InitializeComponent();
		}

		private void NKMDViewer_Load(object sender, EventArgs e)
		{
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

			if (NKMD.ObjectInformation != null) AddTab<MKDS.NKM.OBJI.OBJIEntry>("OBJI", NKMD.ObjectInformation);
			if (NKMD.Path != null) AddTab<PATH.PATHEntry>("PATH", NKMD.Path);
			if (NKMD.Point != null) AddTab<POIT.POITEntry>("POIT", NKMD.Point);
			if (NKMD.KartPointStart != null) AddTab<KTPS.KTPSEntry>("KTPS", NKMD.KartPointStart);
			if (NKMD.KartPointJugem != null) AddTab<KTPJ.KTPJEntry>("KTPJ", NKMD.KartPointJugem);
			if (NKMD.KartPointSecond != null) AddTab<KTP2.KTP2Entry>("KTP2", NKMD.KartPointSecond);
			if (NKMD.KartPointCannon != null) AddTab<KTPC.KTPCEntry>("KTPC", NKMD.KartPointCannon);
			if (NKMD.KartPointMission != null) AddTab<KTPM.KTPMEntry>("KTPM", NKMD.KartPointMission);
			if (NKMD.CheckPoint != null) AddTab<CPOI.CPOIEntry>("CPOI", NKMD.CheckPoint);
			if (NKMD.CheckPointPath != null) AddTab<CPAT.CPATEntry>("CPAT", NKMD.CheckPointPath);
			if (NKMD.ItemPoint != null) AddTab<IPOI.IPOIEntry>("IPOI", NKMD.ItemPoint);
			if (NKMD.ItemPath != null) AddTab<IPAT.IPATEntry>("IPAT", NKMD.ItemPath);
			if (NKMD.EnemyPoint != null) AddTab<EPOI.EPOIEntry>("EPOI", NKMD.EnemyPoint);
			if (NKMD.EnemyPath != null) AddTab<EPAT.EPATEntry>("EPAT", NKMD.EnemyPath);
			if (NKMD.MiniGameEnemyPoint != null) AddTab<MEPO.MEPOEntry>("MEPO", NKMD.MiniGameEnemyPoint);
			if (NKMD.MiniGameEnemyPath != null) AddTab<MEPA.MEPAEntry>("MEPA", NKMD.MiniGameEnemyPath);
			if (NKMD.Area != null) AddTab<AREA.AREAEntry>("AREA", NKMD.Area);
			if (NKMD.Camera != null) AddTab<CAME.CAMEEntry>("CAME", NKMD.Camera);

			if (NKMD.Area != null) mapViewer1.Groups.Add(new MKDSAreaRenderGroup(NKMD.Area, Color.FromArgb(64, Color.CornflowerBlue)));
			
			if (NKMD.Point != null && NKMD.Path != null) mapViewer1.Groups.Add(new MKDSRouteLineRenderGroup(NKMD.Path, NKMD.Point, Color.FromArgb(0, 0, 128)));
			if (NKMD.CheckPoint != null && NKMD.CheckPointPath != null) mapViewer1.Groups.Add(new MKDSCheckPointLineRenderGroup(NKMD.CheckPoint, NKMD.CheckPointPath, Color.FromArgb(0, 170, 0), Color.FromArgb(170, 0, 0)));
			if (NKMD.ItemPoint != null && NKMD.ItemPath != null) mapViewer1.Groups.Add(new MKDSItemPointLineRenderGroup(NKMD.ItemPoint, NKMD.ItemPath, Color.FromArgb(/*255, 230*/204, 153, 0)));
			if (NKMD.EnemyPoint != null && NKMD.EnemyPath != null) mapViewer1.Groups.Add(new MKDSEnemyPointLineRenderGroup(NKMD.EnemyPoint, NKMD.EnemyPath, Color.FromArgb(0, 204, 0)));
			if (NKMD.MiniGameEnemyPoint != null && NKMD.MiniGameEnemyPath != null) mapViewer1.Groups.Add(new MKDSMiniGameEnemyPointLineRenderGroup(NKMD.MiniGameEnemyPoint, NKMD.MiniGameEnemyPath, Color.FromArgb(0, 204, 0)));

			if (NKMD.Point != null) mapViewer1.Groups.Add(new PointRenderGroup<POIT.POITEntry>(Color.FromArgb(0, 0, 128), NKMD.Point, typeof(POIT.POITEntry).GetProperty("Position")));
			if (NKMD.ObjectInformation != null) mapViewer1.Groups.Add(new MKDSObjectRenderGroup(NKMD.ObjectInformation, Color.Red));
			if (NKMD.KartPointStart != null) mapViewer1.Groups.Add(new PointRenderGroup<KTPS.KTPSEntry>(Color.Black, NKMD.KartPointStart, typeof(KTPS.KTPSEntry).GetProperty("Position")));
			if (NKMD.KartPointJugem != null) mapViewer1.Groups.Add(new PointRenderGroup<KTPJ.KTPJEntry>(Color.Orange, NKMD.KartPointJugem, typeof(KTPJ.KTPJEntry).GetProperty("Position")));
			if (NKMD.KartPointSecond != null) mapViewer1.Groups.Add(new PointRenderGroup<KTP2.KTP2Entry>(Color.FromArgb(0, 230, 255), NKMD.KartPointSecond, typeof(KTP2.KTP2Entry).GetProperty("Position")));
			if (NKMD.KartPointCannon != null) mapViewer1.Groups.Add(new PointRenderGroup<KTPC.KTPCEntry>(Color.FromArgb(255, 0, 128), NKMD.KartPointCannon, typeof(KTPC.KTPCEntry).GetProperty("Position")));
			if (NKMD.KartPointMission != null) mapViewer1.Groups.Add(new PointRenderGroup<KTPM.KTPMEntry>(Color.MediumPurple, NKMD.KartPointMission, typeof(KTPM.KTPMEntry).GetProperty("Position")));
			if (NKMD.CheckPoint != null) mapViewer1.Groups.Add(new MKDSCheckPointPoint1RenderGroup(NKMD.CheckPoint, Color.FromArgb(0, 170, 0)));
			if (NKMD.CheckPoint != null) mapViewer1.Groups.Add(new MKDSCheckPointPoint2RenderGroup(NKMD.CheckPoint, Color.FromArgb(170, 0, 0)));
			if (NKMD.Area != null) mapViewer1.Groups.Add(new PointRenderGroup<AREA.AREAEntry>(Color.CornflowerBlue, NKMD.Area, typeof(AREA.AREAEntry).GetProperty("Position")));
			if (NKMD.ItemPoint != null) mapViewer1.Groups.Add(new PointRenderGroup<IPOI.IPOIEntry>(Color.FromArgb(/*255, 230*/204, 153, 0), NKMD.ItemPoint, typeof(IPOI.IPOIEntry).GetProperty("Position")));
			if (NKMD.EnemyPoint != null) mapViewer1.Groups.Add(new PointRenderGroup<EPOI.EPOIEntry>(Color.FromArgb(0, 204, 0), NKMD.EnemyPoint, typeof(EPOI.EPOIEntry).GetProperty("Position")));
			if (NKMD.MiniGameEnemyPoint != null) mapViewer1.Groups.Add(new PointRenderGroup<MEPO.MEPOEntry>(Color.FromArgb(0, 204, 0), NKMD.MiniGameEnemyPoint, typeof(MEPO.MEPOEntry).GetProperty("Position")));
			if (NKMD.Camera != null) mapViewer1.Groups.Add(new PointRenderGroup<CAME.CAMEEntry>(Color.BurlyWood, NKMD.Camera, typeof(CAME.CAMEEntry).GetProperty("Position")));
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

		void GameDataSectionViewer_OnSelected(IGameDataSectionViewer Viewer, object[] Entry)
		{
			propertyGrid1.SelectedObjects = Entry;
			propertyGrid1.ExpandAllGridItems();
			foreach (var v in SectionViewers)
			{
				if (v != Viewer) v.RemoveSelection();
			}
		}

		private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPageIndex == 0)
			{
				mapViewer1.Render();
				mapViewer1.Focus();
				mapViewer1.Select();
			}
		}

		public void FileOpened(ViewableFile File)
		{
			ViewableFile[] v = EveryFileExplorerUtil.GetOpenFilesOfType(typeof(MKDS.KCL));
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
			mapViewer1.Render();
			mapViewer1.Render();
		}

		public void FileClosed(ViewableFile File)
		{
			if (File.FileFormat is MKDS.KCL && File.FileFormat == KCL) KCL = null;
			ViewableFile[] v = EveryFileExplorerUtil.GetOpenFilesOfType(typeof(MKDS.KCL));
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
			mapViewer1.Render();
			mapViewer1.Render();
		}

		private void NKMDViewer_Shown(object sender, EventArgs e)
		{
			mapViewer1.Render();
			mapViewer1.Render();
		}

		private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			foreach (var v in SectionViewers) v.UpdateListViewEntry(propertyGrid1.SelectedObjects);
			mapViewer1.Render();
			mapViewer1.Render();
		}

		private void mapViewer1_Init3D()
		{
			Bitmap b3 = OBJI.OBJ_2D01;//important!
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
			}
		}

		private void mapViewer1_RenderCollision(bool Picking)
		{
			if (KCL == null) return;

			int i = 0;
			foreach (var p in KCL.Planes)
			{
				//Vector3 PositionA, PositionB, PositionC, Normal;
				Triangle t = KCL.GetTriangle(p);

				Color c = MKDS.KCL.GetColor(p.CollisionType);
				if (Picking && c.A != 0) c = Color.FromArgb(i + 1 | 0xFF << 24);
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

		private void NKMDViewer2_Move(object sender, EventArgs e)
		{
			mapViewer1.Render();
			mapViewer1.Render();
		}

		private void mapViewer1_GroupMemberSelected(object Entry)
		{
			foreach (var v in SectionViewers)
			{
				v.Select(Entry);
			}
			propertyGrid1.SelectedObject = Entry;
			propertyGrid1.ExpandAllGridItems();
		}
	}
}
