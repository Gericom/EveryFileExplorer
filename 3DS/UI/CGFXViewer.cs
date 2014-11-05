using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tao.OpenGl;
using LibEveryFileExplorer._3D;
using System.Drawing.Imaging;
using _3DS.NintendoWare.GFX;
using LibEveryFileExplorer.Collections;

namespace _3DS.UI
{
	public partial class CGFXViewer : Form
	{
		bool init = false;
		CGFX cgfx;
		public CGFXViewer(CGFX cgfx)
		{
			this.cgfx = cgfx;
			InitializeComponent();
			simpleOpenGlControl1.MouseWheel += new MouseEventHandler(simpleOpenGlControl1_MouseWheel);
		}
		void simpleOpenGlControl1_MouseWheel(object sender, MouseEventArgs e)
		{
			if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) { dist += ((float)e.Delta / 10); }
			else { dist += ((float)e.Delta / 100); }
			Render();
		}

		String glversion = null;

		float X = 0.0f;
		float Y = 0.0f;
		float ang = 0.0f;
		float dist = 0.0f;
		float elev = 0.0f;
		private void CGFX_Load(object sender, EventArgs e)
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

			if (cgfx.Data.Textures != null && cgfx.Data.Models != null)
			{
				//int i = 1;
				/*foreach (_3DS.CGFX.DATA.TXOB t in cgfx.Data.Textures)
				{
					//int S = (int)((t.Unknown7 >> 8) & 0x3);
					//int T = (int)((t.Unknown7 >> 12) & 0x3);
					//if (S == 0) S = Gl.GL_REPEAT;
					//else S = Gl.GL_MIRRORED_REPEAT;
					//if (T == 0) T = Gl.GL_REPEAT;
					//else T = Gl.GL_MIRRORED_REPEAT;
					int S = Gl.GL_REPEAT;
					int T = Gl.GL_REPEAT;
					/*if ((t.Unknown4 & 0x7) == 0x3)
					{
						S = Gl.GL_MIRRORED_REPEAT;
						T = Gl.GL_MIRRORED_REPEAT;
					}*/

				//if ((t.Unknown4 & 0x1) == 1) S = Gl.GL_MIRRORED_REPEAT;
				//if (((t.Unknown4 >> 1) & 0x1) == 1) T = Gl.GL_MIRRORED_REPEAT;
				/*uint _s = (t.Unknown8 >> 16) & 0xFF;
				uint _t = (t.Unknown8 >> 24) & 0xFF;
				if (_s == 2) S = Gl.GL_REPEAT;
				else if (_s == 3) S = Gl.GL_MIRRORED_REPEAT;
				else S = Gl.GL_CLAMP_TO_EDGE;
				if (_t == 2) T = Gl.GL_REPEAT;
				else if (_t == 3) T = Gl.GL_MIRRORED_REPEAT;
				else T = Gl.GL_CLAMP_TO_EDGE;/
				//GlNitro.glNitroTexImage2D(t.GetBitmap(), i, S, T, Gl.GL_LINEAR, Gl.GL_LINEAR);
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, i);
				Gl.glColor3f(1, 1, 1);
				Bitmap b = t.GetBitmap();
				BitmapData d = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA8, b.Width, b.Height, 0, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, d.Scan0);
				b.UnlockBits(d);
				Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
				Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, S);
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, T);
				i++;
			}*/
				int i = 0;
				foreach (var v in cgfx.Data.Models[0].Materials)
				{
					if (v.Tex0 != null) UploadTex(v.Tex0, i * 4 + 0 + 1);
					if (v.Tex1 != null) UploadTex(v.Tex1, i * 4 + 1 + 1);
					if (v.Tex2 != null) UploadTex(v.Tex2, i * 4 + 2 + 1);
					//if (v.Tex3 != null) UploadTex(v.Tex3, i * 4 + 3 + 1);
					i++;
				}
			}
			if (cgfx.Data.Models != null) Shaders = new CGFXShader[cgfx.Data.Models[0].Materials.Length];
			//GlNitro.glNitroBindTextures(file, 1);

			glversion = Gl.glGetString(Gl.GL_VERSION);

			init = true;
			Render();
		}

		private void UploadTex(CMDL.MTOB.TexInfo TextureMapper, int Id)
		{
			if (!(TextureMapper.TextureObject is ReferenceTexture))
				return;
			var tex = cgfx.Data.Textures[cgfx.Data.Dictionaries[1].IndexOf(((ReferenceTexture)TextureMapper.TextureObject).LinkedTextureName)] as ImageTextureCtr;
			if (tex == null)
				return;
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, Id);
			Gl.glColor3f(1, 1, 1);
			Bitmap b = tex.GetBitmap();
			b.RotateFlip(RotateFlipType.RotateNoneFlipY);
			BitmapData d = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA8, b.Width, b.Height, 0, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, d.Scan0);
			b.UnlockBits(d);

			if (((TextureMapper.Unknown12 >> 1) & 1) == 1) Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
			else Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
			//if (((TextureMapper.Unknown12 >> 2) & 1) == 1) Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
			//else Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
			//A bit confusing, so using this for now:
			Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);

			switch ((TextureMapper.Unknown12 >> 12) & 0xF)
			{
				case 0:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
					break;
				case 1:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_BORDER);
					break;
				case 2:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
					break;
				case 3:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_MIRRORED_REPEAT);
					break;
				default:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
					break;
			}

			switch ((TextureMapper.Unknown12 >> 8) & 0xF)
			{
				case 0:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
					break;
				case 1:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_BORDER);
					break;
				case 2:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
					break;
				case 3:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_MIRRORED_REPEAT);
					break;
				default:
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
					break;
			}
		}

		CGFXShader[] Shaders;
		public void Render()
		{
			if (!init) return;
			if (cgfx.Data.Models == null) return;
			//G3D_Binary_File_Format.Shaders.Shader s = new G3D_Binary_File_Format.Shaders.Shader();
			//s.Compile();
			//s.Enable();
			//Gl.glViewport(0, 0, simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);
			float aspect = (float)simpleOpenGlControl1.Width / (float)simpleOpenGlControl1.Height;
			//float fov = 60.0f; // in degrees
			//float znear = 0.02f;
			//float zfar = 1000.0f * file.modelSet.models[SelMdl].info.posScale;
			//Gl.glMatrixMode(Gl.GL_PROJECTION);
			//Gl.glLoadMatrixf(BuildPerspProjMat(fov, aspect, znear, zfar));
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glLoadIdentity();
			Gl.glViewport(0, 0, simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);
			//Gl.glOrtho(-simpleOpenGlControl1.Width / 2.0f, simpleOpenGlControl1.Width / 2.0f, -simpleOpenGlControl1.Height / 2.0f, simpleOpenGlControl1.Height / 2.0f, 0.02f, 1000f * file.modelSet.models[SelMdl].info.posScale);
			//Gl.glMatrixMode(Gl.GL_PROJECTION);
			//Gl.glLoadIdentity();
			//Gl.glFrustum(-simpleOpenGlControl1.Width / 2f, simpleOpenGlControl1.Width / 2f, -simpleOpenGlControl1.Height / 2f, simpleOpenGlControl1.Height / 2f, 1000, -1000);
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
			CMDL m = cgfx.Data.Models[0];
			//foreach (_3DS.CGFX.DATA.CMDL m in cgfx.Data.Models)
			//{
			//int i = 0;
			foreach (var mm in m.Meshes)
			{
				var vv = m.Shapes[mm.ShapeIndex];
				Polygon p = vv.GetVertexData(m);

				var mat = m.Materials[mm.MaterialIndex];
				mat.FragShader.AlphaTest.GlApply();
				mat.FragmentOperation.BlendOperation.GlApply();

				Gl.glMatrixMode(Gl.GL_TEXTURE);
				if (mat.Tex0 != null)
				{
					Gl.glActiveTexture(Gl.GL_TEXTURE0);
					Gl.glBindTexture(Gl.GL_TEXTURE_2D, mm.MaterialIndex * 4 + 0 + 1);
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					Gl.glLoadIdentity();
					float[] mtx = new float[16];
					Array.Copy(mat.TextureCoordiators[0].Matrix, mtx, 4 * 3);
					mtx[15] = 1;
					mtx[12] = mtx[3];
					mtx[13] = mtx[7];
					mtx[14] = mtx[11];
					mtx[3] = 0;
					mtx[7] = 0;
					mtx[11] = 0;
					Gl.glLoadMatrixf(mtx);
					//if (mat.TextureCoordiators[0].MappingMethod == 2)
					//{
					//	Gl.glTexGeni(Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP);
					//	Gl.glTexGeni(Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP);
					//}
					//Gl.glTranslatef(mat.TextureCoordiators[0].Scale.X / mat.TextureCoordiators[0].Translate.X, mat.TextureCoordiators[0].Translate.Y * mat.TextureCoordiators[0].Scale.Y, 0);
					//Gl.glRotatef(mat.TextureCoordiators[0].Rotate, 0, 1, 0);
					//Gl.glScalef(mat.TextureCoordiators[0].Scale.X, mat.TextureCoordiators[0].Scale.Y, 1);
				}
				if (mat.Tex1 != null)
				{
					Gl.glActiveTexture(Gl.GL_TEXTURE1);
					Gl.glBindTexture(Gl.GL_TEXTURE_2D, mm.MaterialIndex * 4 + 1 + 1);
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					Gl.glLoadIdentity();
					float[] mtx = new float[16];
					Array.Copy(mat.TextureCoordiators[1].Matrix, mtx, 4 * 3);
					mtx[15] = 1;
					mtx[12] = mtx[3];
					mtx[13] = mtx[7];
					mtx[14] = mtx[11];
					mtx[3] = 0;
					mtx[7] = 0;
					mtx[11] = 0;
					Gl.glLoadMatrixf(mtx);
					//if (mat.TextureCoordiators[1].MappingMethod == 2)
					//{
					//	Gl.glTexGeni(Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP);
					//	Gl.glTexGeni(Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP);
					//}
				}
				if (mat.Tex2 != null)
				{
					Gl.glActiveTexture(Gl.GL_TEXTURE2);
					Gl.glBindTexture(Gl.GL_TEXTURE_2D, mm.MaterialIndex * 4 + 2 + 1);
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					Gl.glLoadIdentity();
					float[] mtx = new float[16];
					Array.Copy(mat.TextureCoordiators[2].Matrix, mtx, 4 * 3);
					mtx[15] = 1;
					mtx[12] = mtx[3];
					mtx[13] = mtx[7];
					mtx[14] = mtx[11];
					mtx[3] = 0;
					mtx[7] = 0;
					mtx[11] = 0;
					Gl.glLoadMatrixf(mtx);
					//if (mat.TextureCoordiators[2].MappingMethod == 2)
					//{
					//	Gl.glTexGeni(Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP);
					//	Gl.glTexGeni(Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP);
					//}
				}
				/*if (mat.Tex3 != null)
				{
					MessageBox.Show("Tex3!!!!!!!!!!!");
					Gl.glActiveTexture(Gl.GL_TEXTURE3);
					Gl.glBindTexture(Gl.GL_TEXTURE_2D, mm.MaterialIndex * 4 + 3 + 1);
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					Gl.glLoadIdentity();
				}*/
				Gl.glMatrixMode(Gl.GL_MODELVIEW);

				if (glversion.StartsWith("2.") && Shaders[mm.MaterialIndex] == null)
				{
					List<int> tex = new List<int>();
					if (mat.Tex0 != null) tex.Add((int)mm.MaterialIndex * 4 + 0 + 1);
					else tex.Add(0);
					if (mat.Tex1 != null) tex.Add((int)mm.MaterialIndex * 4 + 1 + 1);
					else tex.Add(0);
					if (mat.Tex2 != null) tex.Add((int)mm.MaterialIndex * 4 + 2 + 1);
					else tex.Add(0);
					//if (mat.Tex3 != null) tex.Add((int)mm.MaterialIndex * 4 + 3 + 1);
					/*else */
					tex.Add(0);

					Shaders[mm.MaterialIndex] = new CGFXShader(mat, tex.ToArray());
					Shaders[mm.MaterialIndex].Compile();
				}
				if (glversion.StartsWith("2.")) Shaders[mm.MaterialIndex].Enable();

				//Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, new float[] { mat.Emission_2.R / 255f, mat.Emission_2.G / 255f, mat.Emission_2.B / 255f, mat.Emission_2.A / 255f });
				//Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_AMBIENT, new float[] { mat.Ambient_1.R / 255f, mat.Ambient_1.G / 255f, mat.Ambient_1.B / 255f, mat.Ambient_1.A / 255f });
				//Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_DIFFUSE, new float[] { mat.Diffuse_2.R / 255f, mat.Diffuse_2.G / 255f, mat.Diffuse_2.B / 255f, mat.Diffuse_2.A / 255f });
				//Gl.glColorMaterial(Gl.GL_FRONT_AND_BACK, Gl.GL_DIFFUSE);
				//Gl.glEnable(Gl.GL_COLOR_MATERIAL);

				foreach (var q in vv.PrimitiveSets[0].Primitives[0].IndexStreams)
				{
					Vector3[] defs = q.GetFaceData();

					Gl.glBegin(Gl.GL_TRIANGLES);
					foreach (Vector3 d in defs)
					{
						if (p.Normals != null) Gl.glNormal3f(p.Normals[(int)d.X].X, p.Normals[(int)d.X].Y, p.Normals[(int)d.X].Z);
						if (p.Colors != null) Gl.glColor4f(p.Colors[(int)d.X].R / 255f, p.Colors[(int)d.X].G / 255f, p.Colors[(int)d.X].B / 255f, p.Colors[(int)d.X].A / 255f);
						else Gl.glColor4f(1, 1, 1, 1);
						if (p.TexCoords != null) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, p.TexCoords[(int)d.X].X, p.TexCoords[(int)d.X].Y);
						if (p.TexCoords2 != null) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE1, p.TexCoords2[(int)d.X].X, p.TexCoords2[(int)d.X].Y);
						if (p.TexCoords3 != null) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE2, p.TexCoords3[(int)d.X].X, p.TexCoords3[(int)d.X].Y);
						Gl.glVertex3f(p.Vertex[(int)d.X].X, p.Vertex[(int)d.X].Y, p.Vertex[(int)d.X].Z);

						if (p.Normals != null) Gl.glNormal3f(p.Normals[(int)d.Y].X, p.Normals[(int)d.Y].Y, p.Normals[(int)d.Y].Z);
						if (p.Colors != null) Gl.glColor4f(p.Colors[(int)d.Y].R / 255f, p.Colors[(int)d.Y].G / 255f, p.Colors[(int)d.Y].B / 255f, p.Colors[(int)d.Y].A / 255f);
						else Gl.glColor4f(1, 1, 1, 1);
						if (p.TexCoords != null) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, p.TexCoords[(int)d.Y].X, p.TexCoords[(int)d.Y].Y);
						if (p.TexCoords2 != null) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE1, p.TexCoords2[(int)d.Y].X, p.TexCoords2[(int)d.Y].Y);
						if (p.TexCoords3 != null) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE2, p.TexCoords3[(int)d.Y].X, p.TexCoords3[(int)d.Y].Y);
						Gl.glVertex3f(p.Vertex[(int)d.Y].X, p.Vertex[(int)d.Y].Y, p.Vertex[(int)d.Y].Z);

						if (p.Normals != null) Gl.glNormal3f(p.Normals[(int)d.Z].X, p.Normals[(int)d.Z].Y, p.Normals[(int)d.Z].Z);
						if (p.Colors != null) Gl.glColor4f(p.Colors[(int)d.Z].R / 255f, p.Colors[(int)d.Z].G / 255f, p.Colors[(int)d.Z].B / 255f, p.Colors[(int)d.Z].A / 255f);
						else Gl.glColor4f(1, 1, 1, 1);
						if (p.TexCoords != null) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, p.TexCoords[(int)d.Z].X, p.TexCoords[(int)d.Z].Y);
						if (p.TexCoords2 != null) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE1, p.TexCoords2[(int)d.Z].X, p.TexCoords2[(int)d.Z].Y);
						if (p.TexCoords3 != null) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE2, p.TexCoords3[(int)d.Z].X, p.TexCoords3[(int)d.Z].Y);
						Gl.glVertex3f(p.Vertex[(int)d.Z].X, p.Vertex[(int)d.Z].Y, p.Vertex[(int)d.Z].Z);
					}
					Gl.glEnd();
				}
			}
		}

		bool wire = false;
		bool licht = true;
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
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
					X -= 5f;
					Render();
					return true;
				case Keys.X:
					X += 5f;
					Render();
					return true;
				case Keys.A:
					Y -= 5f;
					Render();
					return true;
				case Keys.S:
					Y += 5f;
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
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) dist += 12f;
					else dist += 1.2f;
					Render();
					return true;
				case Keys.Oemplus:
				case Keys.Oemplus | Keys.Shift:
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) dist -= 12f;
					else dist -= 1.2f;
					Render();
					return true;
				default:
					return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		private void CGFX_Resize(object sender, EventArgs e)
		{
			Render();
			Render();
		}

		private void CGFX_Layout(object sender, LayoutEventArgs e)
		{
			Render();
		}

		private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
		{
			Render();
		}


	}
}
