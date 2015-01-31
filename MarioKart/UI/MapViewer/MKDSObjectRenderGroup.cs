using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using LibEveryFileExplorer.Collections;
using Tao.OpenGl;

namespace MarioKart.UI.MapViewer
{
	public class MKDSObjectRenderGroup : RenderGroup
	{
		MKDS.NKM.OBJI Objects;

		public MKDSObjectRenderGroup(MKDS.NKM.OBJI Objects, Color PointColor)
		{
			this.Objects = Objects;
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
			foreach (var o in Objects.Entries)
			{
				if (Picking)
				{
					Color c = Color.FromArgb(objidx | PickingId);
					Gl.glColor4f(c.R / 255f, c.G / 255f, c.B / 255f, 1);
					objidx++;
				}
				Bitmap b;
				if ((b = (Bitmap)OBJI.ResourceManager.GetObject("OBJ_" + BitConverter.ToString(BitConverter.GetBytes(o.ObjectID), 0).Replace("-", ""))) == null)
				{
					Gl.glVertex2f(o.Position.X, o.Position.Z);
				}
				else
				{
					Gl.glEnd();
					if (!Picking)
					{
						Gl.glColor3f(1, 1, 1);
						Gl.glBindTexture(Gl.GL_TEXTURE_2D, o.ObjectID);
					}
					Gl.glPushMatrix();
					Gl.glTranslatef(o.Position.X, o.Position.Z, 0);

					Gl.glRotatef(o.Rotation.Y, 0, 0, 1);

					int[] viewport = new int[4];
					float[] pm = new float[16];
					Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
					Gl.glGetFloatv(Gl.GL_PROJECTION_MATRIX, pm);

					float scale = 1f / pm[0] / viewport[2] * 2f;
					Gl.glScalef(scale, scale, 1);

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
					if (!Picking)
					{
						Gl.glColor3f(PointColor.R / 255f, PointColor.G / 255f, PointColor.B / 255f);
						Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
					}
					Gl.glBegin(Gl.GL_POINTS);
				}
			}
			Gl.glEnd();
		}

		public override object GetEntry(int Index)
		{
			return Objects[Index];
		}

		public override Vector3 GetPosition(int Index)
		{
			return Objects[Index].Position;
		}

		public override void SetPosition(int Index, Vector3 Position, bool ValidY = false)
		{
			if (!ValidY) Position.Y = Objects[Index].Position.Y;
			Objects[Index].Position = Position;
		}
	}
}
