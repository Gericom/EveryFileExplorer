using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tao.OpenGl;
using NDS.NitroSystem.G3D;

namespace NDS.UI
{
	public partial class MDL0Viewer : UserControl
	{
		MDL0.Model Model;
		public MDL0Viewer(MDL0.Model Model)
		{
			this.Model = Model;
			InitializeComponent();
			simpleOpenGlControl1.MouseWheel += new MouseEventHandler(simpleOpenGlControl1_MouseWheel);
		}
		void simpleOpenGlControl1_MouseWheel(object sender, MouseEventArgs e)
		{
			if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) { dist += ((float)e.Delta / 10) * Model.info.posScale / 8f; }
			else { dist += ((float)e.Delta / 100) * Model.info.posScale / 8f; }
			Render();
		}

		bool init = false;

		float X = 0.0f;
		float Y = 0.0f;
		float ang = 0.0f;
		float dist = 0.0f;
		float elev = 0.0f;
		private void MDL0Viewer_Load(object sender, EventArgs e)
		{
			simpleOpenGlControl1.InitializeContexts();
			Gl.ReloadFunctions();
			//Gl.glEnable(Gl.GL_LIGHTING);
			Gl.glEnable(Gl.GL_RESCALE_NORMAL);
			Gl.glEnable(Gl.GL_COLOR_MATERIAL);
			Gl.glEnable(Gl.GL_DEPTH_TEST);
			Gl.glEnable(Gl.GL_NORMALIZE);
			Gl.glDisable(Gl.GL_CULL_FACE);
			Gl.glFrontFace(Gl.GL_CCW);
			Gl.glEnable(Gl.GL_TEXTURE_2D);
			Gl.glClearDepth(1);
			Gl.glEnable(Gl.GL_ALPHA_TEST);
			Gl.glAlphaFunc(Gl.GL_GREATER, 0f);
			Gl.glEnable(Gl.GL_BLEND);
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

			//Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_BLEND);

			Gl.glShadeModel(Gl.GL_SMOOTH);

			Gl.glDepthFunc(Gl.GL_LEQUAL);


			Gl.glClearColor(51f / 255f, 51f / 255f, 51f / 255f, 0f);

			init = true;
			Render();
		}

		public void Render()
		{
			if (!init) return;
			float aspect = (float)simpleOpenGlControl1.Width / (float)simpleOpenGlControl1.Height;
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glLoadIdentity();
			Gl.glViewport(0, 0, simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);
			Glu.gluPerspective(30, aspect, 0.1f, 20480000f);//0.02f, 32.0f);

			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glLoadIdentity();
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
			Gl.glColor3f(1.0f, 1.0f, 1.0f);
			//Gl.glEnable(Gl.GL_DEPTH_TEST);
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

			/*Gl.glRotatef(elev, 1, 0, 0);
			Gl.glFogfv(Gl.GL_FOG_COLOR, new float[] { 0, 0, 0, 1 });
			Gl.glFogf(Gl.GL_FOG_DENSITY, 1);
			Gl.glRotatef(-elev, 1, 0, 0);*/

			Gl.glTranslatef(X, Y, -dist);
			Gl.glRotatef(elev, 1, 0, 0);
			Gl.glRotatef(ang, 0, 1, 0);

			Gl.glPushMatrix();
			RenderModel();
			Gl.glPopMatrix();
			simpleOpenGlControl1.Refresh();
		}

		void RenderModel()
		{
			Model.ProcessSbc();
		}

		bool wire = false;
		bool licht = true;
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData & Keys.KeyCode)
			{
				case Keys.Right:
					ang += 1;
					Render();
					return true;
				case Keys.Left:
					ang -= 1;
					Render();
					return true;
				case Keys.Up:
					elev += 1;
					Render();
					return true;
				case Keys.Down:
					elev -= 1;
					Render();
					return true;
				case Keys.Z:
					X -= 0.05f * Model.info.posScale / 2;//(((Control.ModifierKeys & Keys.Shift) != 0) ? 100f : 1f);
					Render();
					return true;
				case Keys.X:
					X += 0.05f * Model.info.posScale / 2;//* (((Control.ModifierKeys & Keys.Shift) != 0) ? 100f : 1f);
					Render();
					return true;
				case Keys.A:
					Y -= 0.05f * Model.info.posScale / 2;//* (((Control.ModifierKeys & Keys.Shift) != 0) ? 100f : 1f);
					Render();
					return true;
				case Keys.S:
					Y += 0.05f * Model.info.posScale / 2;//* (((Control.ModifierKeys & Keys.Shift) != 0) ? 100f : 1f);
					Render();
					return true;
				case Keys.W:
					wire = !wire;
					if (wire) { Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE); Render(); }
					else { Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL); Render(); }
					return true;
				case Keys.Escape:
					X = 0.0f;
					Y = 0.0f;
					ang = 0.0f;
					dist = 0.0f;
					elev = 0.0f;
					Render();
					return true;
				case Keys.T:
					elev = 90;
					ang = 0;
					Render();
					return true;
				case Keys.L:
					licht = !licht;
					Render();
					return true;
				case Keys.OemMinus:
				case Keys.OemMinus | Keys.Shift:
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) dist += 12f * Model.info.posScale / 8f;
					else dist += 1.2f * Model.info.posScale / 8f;
					Render();
					return true;
				case Keys.Oemplus:
				case Keys.Oemplus | Keys.Shift:
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) dist -= 12f * Model.info.posScale / 8f;
					else dist -= 1.2f * Model.info.posScale / 8f;
					Render();
					return true;
				default:
					return base.ProcessCmdKey(ref msg, keyData);
			}
		}
	}
}
