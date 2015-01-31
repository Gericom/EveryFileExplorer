using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarioKart.MKDS.NKM;
using System.Drawing;
using Tao.OpenGl;

namespace MarioKart.UI.MapViewer
{
	public class MKDSItemPointLineRenderGroup : RenderGroup
	{
		IPOI ItemPoints;
		IPAT ItemPointPaths;

		public MKDSItemPointLineRenderGroup(IPOI ItemPoints, IPAT ItemPointPaths, Color LineColor)
		{
			this.ItemPoints = ItemPoints;
			this.ItemPointPaths = ItemPointPaths;
			this.LineColor = LineColor;
		}

		public Color LineColor { get; private set; }

		public override bool Interactable { get { return false; } }

		public override void Render(object[] Selection, bool Picking, int PickingId)
		{
			if (Picking) return;
			Gl.glLineWidth(1.5f);
			Gl.glBegin(Gl.GL_LINES);
			Gl.glColor3f(LineColor.R / 255f, LineColor.G / 255f, LineColor.B / 255f);
			for (int j = 0; j < ItemPointPaths.Entries.Count; j++)
			{
				if (ItemPoints.Entries.Count < ItemPointPaths[j].StartIndex + ItemPointPaths[j].Length) break;
				for (int i = ItemPointPaths[j].StartIndex; i < ItemPointPaths.Entries[j].StartIndex + ItemPointPaths[j].Length - 1; i++)
				{
					Gl.glVertex2f(ItemPoints[i].Position.X, ItemPoints[i].Position.Z);
					Gl.glVertex2f(ItemPoints[i + 1].Position.X, ItemPoints[i + 1].Position.Z);
				}

				for (int i = 0; i < 3; i++)
				{
					if (ItemPointPaths[j].GoesTo[i] == 0xFF || ItemPointPaths[j].GoesTo[i] >= ItemPointPaths.Entries.Count || ItemPoints.Entries.Count <= ItemPointPaths[j].StartIndex + ItemPointPaths[j].Length - 1 || ItemPoints.Entries.Count <= ItemPointPaths[ItemPointPaths[j].GoesTo[i]].StartIndex) continue;
					Gl.glVertex2f(ItemPoints[ItemPointPaths[j].StartIndex + ItemPointPaths[j].Length - 1].Position.X, ItemPoints[ItemPointPaths[j].StartIndex + ItemPointPaths[j].Length - 1].Position.Z);
					Gl.glVertex2f(ItemPoints[ItemPointPaths[ItemPointPaths[j].GoesTo[i]].StartIndex].Position.X, ItemPoints[ItemPointPaths[ItemPointPaths[j].GoesTo[i]].StartIndex].Position.Z);
				}

				for (int i = 0; i < 3; i++)
				{
					if (ItemPointPaths[j].ComesFrom[i] == 0xFF || ItemPointPaths[j].ComesFrom[i] >= ItemPointPaths.Entries.Count || ItemPoints.Entries.Count <= ItemPointPaths[j].StartIndex || ItemPoints.Entries.Count <= ItemPointPaths[ItemPointPaths[j].ComesFrom[i]].StartIndex + ItemPointPaths[ItemPointPaths[j].ComesFrom[i]].Length - 1) continue;
					Gl.glVertex2f(ItemPoints[ItemPointPaths[j].StartIndex].Position.X, ItemPoints[ItemPointPaths[j].StartIndex].Position.Z);
					Gl.glVertex2f(ItemPoints[ItemPointPaths[ItemPointPaths[j].ComesFrom[i]].StartIndex + ItemPointPaths[ItemPointPaths[j].ComesFrom[i]].Length - 1].Position.X, ItemPoints[ItemPointPaths[ItemPointPaths[j].ComesFrom[i]].StartIndex + ItemPointPaths[ItemPointPaths[j].ComesFrom[i]].Length - 1].Position.Z);
				}
			}
			Gl.glEnd();
		}
	}
}
