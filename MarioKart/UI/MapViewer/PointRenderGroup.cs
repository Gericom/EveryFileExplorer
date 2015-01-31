using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using LibEveryFileExplorer.GameData;
using System.Drawing;
using Tao.OpenGl;
using LibEveryFileExplorer.Collections;

namespace MarioKart.UI.MapViewer
{
	public class PointRenderGroup<T> : RenderGroup where T : GameDataSectionEntry
	{
		private MemberInfo PositionMember;
		private GameDataSection<T> GameDataSection;

		public PointRenderGroup(Color PointColor, GameDataSection<T> GameDataSection, MemberInfo PositionMember)
		{
			this.PointColor = PointColor;
			this.GameDataSection = GameDataSection;
			this.PositionMember = PositionMember;
		}

		public Color PointColor { get; private set; }

		public override bool Interactable { get { return true; } }

		public override void Render(object[] Selection, bool Picking, int PickingId)
		{
			Gl.glPointSize((Picking ? 6f : 5));

			Gl.glBegin(Gl.GL_POINTS);
			if (!Picking) Gl.glColor3f(PointColor.R / 255f, PointColor.G / 255f, PointColor.B / 255f);
			int objidx = 1;
			foreach (var o in GameDataSection.Entries)
			{
				if (Picking)
				{
					Color c = Color.FromArgb(objidx | PickingId);
					Gl.glColor4f(c.R / 255f, c.G / 255f, c.B / 255f, 1);
					objidx++;
				}
				Vector3 Position = GetPointPosition(o);
				Gl.glVertex2f(Position.X, Position.Z);

				if (!Picking && Selection != null && Selection.Contains(o))
				{
					Gl.glEnd();
					Gl.glPointSize(2f);
					Gl.glBegin(Gl.GL_POINTS);
					Gl.glColor3f(1, 1, 1);
					Gl.glVertex2f(Position.X, Position.Z);
					Gl.glEnd();
					Gl.glPointSize((Picking ? 6f : 5));
					Gl.glBegin(Gl.GL_POINTS);
					Gl.glColor3f(PointColor.R / 255f, PointColor.G / 255f, PointColor.B / 255f);
				}

			}
			Gl.glEnd();
		}

		private Vector3 GetPointPosition(T Entry)
		{
			if (PositionMember is PropertyInfo) return (Vector3)((PropertyInfo)PositionMember).GetValue(Entry, null);
			else if (PositionMember is FieldInfo) return (Vector3)((FieldInfo)PositionMember).GetValue(Entry);
			throw new Exception("PositionMember is no Property and no Field!");
		}

		private void SetPointPosition(T Entry, Vector3 Position)
		{
			if (PositionMember is PropertyInfo) ((PropertyInfo)PositionMember).SetValue(Entry, Position, null);
			else if (PositionMember is FieldInfo) ((FieldInfo)PositionMember).SetValue(Entry, Position);
			else throw new Exception("PositionMember is no Property and no Field!");
		}

		public override Vector3 GetPosition(int Index)
		{
			return GetPointPosition(GameDataSection[Index]);
		}

		public override void SetPosition(int Index, Vector3 Position, bool ValidY = false)
		{
			Vector3 NewPos = GetPointPosition(GameDataSection[Index]);
			NewPos.X = Position.X;
			if (ValidY) NewPos.Y = Position.Y;
			NewPos.Z = Position.Z;
			SetPointPosition(GameDataSection[Index], NewPos);
		}

		public override object GetEntry(int Index)
		{
			return GameDataSection[Index];
		}
	}
}
