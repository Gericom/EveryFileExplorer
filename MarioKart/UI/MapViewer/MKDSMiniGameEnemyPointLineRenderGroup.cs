using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarioKart.MKDS.NKM;
using System.Drawing;
using Tao.OpenGl;

namespace MarioKart.UI.MapViewer
{
	public class MKDSMiniGameEnemyPointLineRenderGroup : RenderGroup
	{
		MEPO MiniGameEnemyPoints;
		MEPA MiniGameEnemyPointPaths;

		public MKDSMiniGameEnemyPointLineRenderGroup(MEPO MiniGameEnemyPoints, MEPA MiniGameEnemyPointPaths, Color LineColor)
		{
			this.MiniGameEnemyPoints = MiniGameEnemyPoints;
			this.MiniGameEnemyPointPaths = MiniGameEnemyPointPaths;
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
			for (int j = 0; j < MiniGameEnemyPointPaths.Entries.Count; j++)
			{
				if (MiniGameEnemyPoints.Entries.Count < MiniGameEnemyPointPaths[j].StartIndex + MiniGameEnemyPointPaths[j].Length) break;
				for (int i = MiniGameEnemyPointPaths[j].StartIndex; i < MiniGameEnemyPointPaths.Entries[j].StartIndex + MiniGameEnemyPointPaths[j].Length - 1; i++)
				{
					Gl.glVertex2f(MiniGameEnemyPoints[i].Position.X, MiniGameEnemyPoints[i].Position.Z);
					Gl.glVertex2f(MiniGameEnemyPoints[i + 1].Position.X, MiniGameEnemyPoints[i + 1].Position.Z);
				}

				for (int i = 0; i < 8; i++)
				{
					if (MiniGameEnemyPointPaths[j].GoesTo[i] == 0xFF || MiniGameEnemyPointPaths[j].GoesTo[i] >= MiniGameEnemyPoints.Entries.Count) continue;
					Gl.glVertex2f(MiniGameEnemyPoints[MiniGameEnemyPointPaths[j].StartIndex + MiniGameEnemyPointPaths[j].Length - 1].Position.X, MiniGameEnemyPoints[MiniGameEnemyPointPaths[j].StartIndex + MiniGameEnemyPointPaths[j].Length - 1].Position.Z);
					Gl.glVertex2f(MiniGameEnemyPoints[MiniGameEnemyPointPaths[j].GoesTo[i]].Position.X, MiniGameEnemyPoints[MiniGameEnemyPointPaths[j].GoesTo[i]].Position.Z);
				}

				for (int i = 0; i < 8; i++)
				{
					if (MiniGameEnemyPointPaths[j].ComesFrom[i] == 0xFF || MiniGameEnemyPointPaths[j].ComesFrom[i] >= MiniGameEnemyPoints.Entries.Count) continue;
					Gl.glVertex2f(MiniGameEnemyPoints[MiniGameEnemyPointPaths[j].StartIndex].Position.X, MiniGameEnemyPoints[MiniGameEnemyPointPaths[j].StartIndex].Position.Z);
					Gl.glVertex2f(MiniGameEnemyPoints[MiniGameEnemyPointPaths[j].ComesFrom[i]].Position.X, MiniGameEnemyPoints[MiniGameEnemyPointPaths[j].ComesFrom[i]].Position.Z);
				}
			}
			Gl.glEnd();
		}
	}
}
