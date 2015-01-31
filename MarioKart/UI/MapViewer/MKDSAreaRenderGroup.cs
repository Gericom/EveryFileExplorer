using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarioKart.MKDS.NKM;
using System.Drawing;
using Tao.OpenGl;
using LibEveryFileExplorer.Collections;

namespace MarioKart.UI.MapViewer
{
	public class MKDSAreaRenderGroup : RenderGroup
	{
		AREA Areas;

		public MKDSAreaRenderGroup(AREA Areas, Color AreaColor)
		{
			this.Areas = Areas;
			this.AreaColor = AreaColor;
		}

		public Color AreaColor { get; private set; }

		public override bool Interactable { get { return false; } }

		public override void Render(object[] Selection, bool Picking, int PickingId)
		{
			if (Picking) return;
			Gl.glColor4f(AreaColor.R / 255f, AreaColor.G / 255f, AreaColor.B / 255f, AreaColor.A / 255f);
			Gl.glBegin(Gl.GL_QUADS);
			foreach (var o in Areas.Entries)
			{
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
			Gl.glEnd();
		}
	}
}
