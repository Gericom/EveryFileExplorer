using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarioKart.MKDS.NKM;
using LibEveryFileExplorer.Collections;
using System.Drawing;
using Tao.OpenGl;

namespace MarioKart.UI.MapViewer
{
	public class MKDSRouteLineRenderGroup : RenderGroup
	{
		PATH Paths;
		POIT Points;

		public MKDSRouteLineRenderGroup(PATH Paths, POIT Points, Color LineColor)
		{
			this.Paths = Paths;
			this.Points = Points;
			this.LineColor = LineColor;
		}

		public Color LineColor { get; private set; }

		public override bool Interactable { get { return false; } }

		public override void Render(bool Picking, int PickingId)
		{
			if (Picking) return;
			Gl.glLineWidth(1.5f);
			Gl.glColor3f(LineColor.R / 255f, LineColor.G / 255f, LineColor.B / 255f);
			int idx = 0;
			foreach (var o in Paths.Entries)
			{
				if (Points.NrEntries < o.NrPoit + idx) break;
				Gl.glBegin(Gl.GL_LINE_STRIP);
				for (int i = 0; i < o.NrPoit; i++)
				{
					Gl.glVertex2f(Points[idx + i].Position.X, Points[idx + i].Position.Z);
					if (!(i + 1 < o.NrPoit) && o.Loop)
					{
						Gl.glVertex2f(Points[idx].Position.X, Points[idx].Position.Z);
					}
				}
				Gl.glEnd();
				idx += o.NrPoit;
			}
		}
	}
}
