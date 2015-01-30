using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarioKart.MKDS.NKM;
using System.Drawing;
using Tao.OpenGl;

namespace MarioKart.UI.MapViewer
{
	public class MKDSEnemyPointLineRenderGroup : RenderGroup
	{
		EPOI EnemyPoints;
		EPAT EnemyPointPaths;

		public MKDSEnemyPointLineRenderGroup(EPOI EnemyPoints, EPAT EnemyPointPaths, Color LineColor)
		{
			this.EnemyPoints = EnemyPoints;
			this.EnemyPointPaths = EnemyPointPaths;
			this.LineColor = LineColor;
		}

		public Color LineColor { get; private set; }

		public override bool Interactable { get { return false; } }

		public override void Render(bool Picking, int PickingId)
		{
			if (Picking) return;
			Gl.glLineWidth(1.5f);
			Gl.glBegin(Gl.GL_LINES);
			Gl.glColor3f(LineColor.R / 255f, LineColor.G / 255f, LineColor.B / 255f);
			for (int j = 0; j < EnemyPointPaths.Entries.Count; j++)
			{
				if (EnemyPoints.Entries.Count < EnemyPointPaths[j].StartIndex + EnemyPointPaths[j].Length) break;
				for (int i = EnemyPointPaths[j].StartIndex; i < EnemyPointPaths.Entries[j].StartIndex + EnemyPointPaths[j].Length - 1; i++)
				{
					Gl.glVertex2f(EnemyPoints[i].Position.X, EnemyPoints[i].Position.Z);
					Gl.glVertex2f(EnemyPoints[i + 1].Position.X, EnemyPoints[i + 1].Position.Z);
				}

				for (int i = 0; i < 3; i++)
				{
					if (EnemyPointPaths[j].GoesTo[i] == -1 || EnemyPointPaths[j].GoesTo[i] >= EnemyPointPaths.Entries.Count) continue;
					Gl.glVertex2f(EnemyPoints[EnemyPointPaths[j].StartIndex + EnemyPointPaths[j].Length - 1].Position.X, EnemyPoints[EnemyPointPaths[j].StartIndex + EnemyPointPaths[j].Length - 1].Position.Z);
					Gl.glVertex2f(EnemyPoints[EnemyPointPaths[EnemyPointPaths[j].GoesTo[i]].StartIndex].Position.X, EnemyPoints[EnemyPointPaths[EnemyPointPaths[j].GoesTo[i]].StartIndex].Position.Z);
				}

				for (int i = 0; i < 3; i++)
				{
					if (EnemyPointPaths[j].ComesFrom[i] == -1 || EnemyPointPaths[j].ComesFrom[i] >= EnemyPointPaths.Entries.Count) continue;
					Gl.glVertex2f(EnemyPoints[EnemyPointPaths[j].StartIndex].Position.X, EnemyPoints[EnemyPointPaths[j].StartIndex].Position.Z);
					Gl.glVertex2f(EnemyPoints[EnemyPointPaths[EnemyPointPaths[j].ComesFrom[i]].StartIndex + EnemyPointPaths[EnemyPointPaths[j].ComesFrom[i]].Length - 1].Position.X, EnemyPoints[EnemyPointPaths[EnemyPointPaths[j].ComesFrom[i]].StartIndex + EnemyPointPaths[EnemyPointPaths[j].ComesFrom[i]].Length - 1].Position.Z);
				}
			}
			Gl.glEnd();
		}
	}
}
