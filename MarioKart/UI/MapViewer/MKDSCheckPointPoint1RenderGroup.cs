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
	public class MKDSCheckPointPoint1RenderGroup : RenderGroup
	{
		CPOI Checkpoints;

		public MKDSCheckPointPoint1RenderGroup(CPOI Checkpoints, Color PointColor)
		{
			this.Checkpoints = Checkpoints;
			this.PointColor = PointColor;
		}

		public Color PointColor { get; private set; }

		public override bool Interactable { get { return true; } }

		public override void Render(object[] Selection, bool Picking, int PickingId)
		{
			Gl.glPointSize((Picking ? 6f : 5));

			Gl.glBegin(Gl.GL_POINTS);
			if (!Picking) Gl.glColor3f(PointColor.R / 255f, PointColor.G / 255f, PointColor.B / 255f);
			int objidx = 1;
			foreach (var o in Checkpoints.Entries)
			{
				if (Picking)
				{
					Color c = Color.FromArgb(objidx | PickingId);
					Gl.glColor4f(c.R / 255f, c.G / 255f, c.B / 255f, 1);
					objidx++;
				}
				Gl.glVertex2f(o.Point1.X, o.Point1.Y);
			}
			Gl.glEnd();
		}

		public override object GetEntry(int Index)
		{
			return Checkpoints[Index];
		}

		public override Vector3 GetPosition(int Index)
		{
			return new Vector3(Checkpoints[Index].Point1.X, 0, Checkpoints[Index].Point1.Y);
		}

		public override void SetPosition(int Index, Vector3 Position, bool ValidY = false)
		{
			Checkpoints[Index].Point1 = new Vector2(Position.X, Position.Z);
		}
	}
}
