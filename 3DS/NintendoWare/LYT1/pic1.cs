using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.Collections;
using Tao.OpenGl;

namespace _3DS.NintendoWare.LYT1
{
	public class pic1 : pan1
	{
		public pic1(EndianBinaryReader er)
			: base(er)
		{
			VertexColorLT = er.ReadColor8();
			VertexColorRT = er.ReadColor8();
			VertexColorLB = er.ReadColor8();
			VertexColorRB = er.ReadColor8();
			MaterialId = er.ReadUInt16();
			NrTexCoordEntries = er.ReadUInt16();
			TexCoordEntries = new TexCoord[NrTexCoordEntries];
			for (int i = 0; i < NrTexCoordEntries; i++)
			{
				TexCoordEntries[i] = new TexCoord(er);
			}
		}
		public Color VertexColorLT;
		public Color VertexColorRT;
		public Color VertexColorLB;
		public Color VertexColorRB;
		public UInt16 MaterialId;
		public UInt16 NrTexCoordEntries;
		public TexCoord[] TexCoordEntries;
		public class TexCoord
		{
			public TexCoord(EndianBinaryReader er)
			{
				TexCoordLT = er.ReadVector2();
				TexCoordRT = er.ReadVector2();
				TexCoordLB = er.ReadVector2();
				TexCoordRB = er.ReadVector2();
			}
			public Vector2 TexCoordLT;
			public Vector2 TexCoordRT;
			public Vector2 TexCoordLB;
			public Vector2 TexCoordRB;
		}

		public override void Render(CLYT Layout, CLIM[] Textures, int InfluenceAlpha)
		{
			Gl.glPushMatrix();
			{
				SetupMaterial(Layout, MaterialId);
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
				Gl.glTranslatef(Translation.X, Translation.Y, Translation.Z);
				Gl.glRotatef(Rotation.X, 1, 0, 0);
				Gl.glRotatef(Rotation.Y, 0, 1, 0);
				Gl.glRotatef(Rotation.Z, 0, 0, 1);
				Gl.glScalef(Scale.X, Scale.Y, 1);
				Gl.glPushMatrix();
				{
					Gl.glTranslatef(-0.5f * Size.X * (float)HAlignment, -0.5f * Size.Y * (-(float)VAlignment), 0);
					float[,] Vertex2 = SetupRect();
					float[][] VtxColor = SetupVtxColors(InfluenceAlpha);
					Gl.glBegin(Gl.GL_QUADS);

					for (int o = 0; o < TexCoordEntries.Length; o++)
					{
						Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0 + o,
							TexCoordEntries[o].TexCoordLT.X, TexCoordEntries[o].TexCoordLT.Y);
					}
					if (TexCoordEntries.Length == 0) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 0);
					Gl.glColor4f(VtxColor[0][0], VtxColor[0][1], VtxColor[0][2], VtxColor[0][3]);
					Gl.glVertex3f(Vertex2[0, 0], Vertex2[0, 1], 0);
					for (int o = 0; o < TexCoordEntries.Length; o++)
					{
						Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0 + o,
							TexCoordEntries[o].TexCoordRT.X, TexCoordEntries[o].TexCoordRT.Y);
					}
					if (TexCoordEntries.Length == 0) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, 0);
					Gl.glColor4f(VtxColor[1][0], VtxColor[1][1], VtxColor[1][2], VtxColor[1][3]);
					Gl.glVertex3f(Vertex2[1, 0], Vertex2[1, 1], 0);
					for (int o = 0; o < TexCoordEntries.Length; o++)
					{
						Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0 + o,
							TexCoordEntries[o].TexCoordRB.X, TexCoordEntries[o].TexCoordRB.Y);
					}
					if (TexCoordEntries.Length == 0) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, 1);
					Gl.glColor4f(VtxColor[2][0], VtxColor[2][1], VtxColor[2][2], VtxColor[2][3]);
					Gl.glVertex3f(Vertex2[2, 0], Vertex2[2, 1], 0);
					for (int o = 0; o < TexCoordEntries.Length; o++)
					{
						Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0 + o,
							TexCoordEntries[o].TexCoordLB.X, TexCoordEntries[o].TexCoordLB.Y);
					}
					if (TexCoordEntries.Length == 0) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 1);
					Gl.glColor4f(VtxColor[3][0], VtxColor[3][1], VtxColor[3][2], VtxColor[3][3]);
					Gl.glVertex3f(Vertex2[3, 0], Vertex2[3, 1], 0);
					Gl.glEnd();
				}
				Gl.glPopMatrix();
				foreach (pan1 p in Children)
				{
					p.Render(Layout, Textures, InfluencedAlpha ? (int)((float)(Alpha * InfluenceAlpha) / 255f) : Alpha);
				}
			}
			Gl.glPopMatrix();
		}

		private float[][] SetupVtxColors(int InfluenceAlpha)
		{
			float[] TL2;
			float[] TR2;
			float[] BL2;
			float[] BR2;
			TL2 = new float[]
			{
				VertexColorLT.R / 255f,
				VertexColorLT.G / 255f,
				VertexColorLT.B / 255f,
				MixColors(VertexColorLT.A, (InfluencedAlpha ? (byte)(((float)Alpha  * (float)InfluenceAlpha) / 255f) : this.Alpha))
			};
			TR2 = new float[]
			{
				VertexColorRT.R / 255f,
				VertexColorRT.G / 255f,
				VertexColorRT.B / 255f,
				MixColors(VertexColorRT.A, (InfluencedAlpha ? (byte)(((float)Alpha  * (float)InfluenceAlpha) / 255f) : this.Alpha))
			};
			BR2 = new float[]
			{
				VertexColorRB.R / 255f,
				VertexColorRB.G / 255f,
				VertexColorRB.B / 255f,
				MixColors(VertexColorRB.A,(InfluencedAlpha ? (byte)(((float)Alpha  * (float)InfluenceAlpha) / 255f) : this.Alpha))
			};
			BL2 = new float[]
			{
				VertexColorLB.R / 255f,
				VertexColorLB.G / 255f,
				VertexColorLB.B / 255f,
				MixColors(VertexColorLB.A, (InfluencedAlpha ? (byte)(((float)Alpha  * (float)InfluenceAlpha) / 255f) : this.Alpha))
			};
			return new float[][] { TL2, TR2, BR2, BL2 };
		}
	}
}
