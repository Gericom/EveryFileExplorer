using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using LibEveryFileExplorer.Collections;
using System.IO;
using Tao.OpenGl;

namespace _3DS.NintendoWare.LYT1
{
	public class txt1 : pan1
	{
		public txt1(EndianBinaryReader er)
			: base(er)
		{
			long baseoffset = er.BaseStream.Position - 0x4C;
			NrCharacters = er.ReadUInt16();
			NrCharacters2 = er.ReadUInt16();
			MaterialId = er.ReadUInt16();
			FontId = er.ReadUInt16();
			PositionType = er.ReadByte();
			TextAlignment = er.ReadByte();
			TextFlags = er.ReadByte();
			Padding = er.ReadByte();
			StringOffset = er.ReadUInt32();
			TopColor = er.ReadColor8();
			BottomColor = er.ReadColor8();
			FontSize = er.ReadVector2();
			CharSize = er.ReadSingle();
			LineSize = er.ReadSingle();
			er.BaseStream.Position = baseoffset + StringOffset;
			Text = er.ReadStringNT(Encoding.Unicode);
			er.BaseStream.Position = baseoffset + SectionSize;
		}
		public UInt16 NrCharacters;
		public UInt16 NrCharacters2;//?
		public UInt16 MaterialId;
		public UInt16 FontId;
		public Byte PositionType;
		public Byte TextAlignment;
		public Byte TextFlags;
		public Byte Padding;
		public UInt32 StringOffset;
		public Color TopColor;
		public Color BottomColor;
		public Vector2 FontSize;
		public Single CharSize;
		public Single LineSize;

		public String Text;

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

					/*for (int o = 0; o < TexCoordEntries.Length; o++)
					{
						Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0 + o,
							TexCoordEntries[o].TexCoordLT.X, TexCoordEntries[o].TexCoordLT.Y);
					}
					if (TexCoordEntries.Length == 0)*/ Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 0);
					Gl.glColor4f(VtxColor[0][0], VtxColor[0][1], VtxColor[0][2], VtxColor[0][3]);
					Gl.glVertex3f(Vertex2[0, 0], Vertex2[0, 1], 0);
					/*for (int o = 0; o < TexCoordEntries.Length; o++)
					{
						Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0 + o,
							TexCoordEntries[o].TexCoordRT.X, TexCoordEntries[o].TexCoordRT.Y);
					}
					if (TexCoordEntries.Length == 0)*/ Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, 0);
					Gl.glColor4f(VtxColor[1][0], VtxColor[1][1], VtxColor[1][2], VtxColor[1][3]);
					Gl.glVertex3f(Vertex2[1, 0], Vertex2[1, 1], 0);
					/*for (int o = 0; o < TexCoordEntries.Length; o++)
					{
						Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0 + o,
							TexCoordEntries[o].TexCoordRB.X, TexCoordEntries[o].TexCoordRB.Y);
					}
					if (TexCoordEntries.Length == 0)*/ Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, 1);
					Gl.glColor4f(VtxColor[2][0], VtxColor[2][1], VtxColor[2][2], VtxColor[2][3]);
					Gl.glVertex3f(Vertex2[2, 0], Vertex2[2, 1], 0);
					/*for (int o = 0; o < TexCoordEntries.Length; o++)
					{
						Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0 + o,
							TexCoordEntries[o].TexCoordLB.X, TexCoordEntries[o].TexCoordLB.Y);
					}
					if (TexCoordEntries.Length == 0)*/Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 1);
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
				TopColor.R / 255f,
				TopColor.G / 255f,
				TopColor.B / 255f,
				MixColors(TopColor.A, (InfluencedAlpha ? (byte)(((float)Alpha  * (float)InfluenceAlpha) / 255f) : this.Alpha))
			};
			TR2 = new float[]
			{
				TopColor.R / 255f,
				TopColor.G / 255f,
				TopColor.B / 255f,
				MixColors(TopColor.A, (InfluencedAlpha ? (byte)(((float)Alpha  * (float)InfluenceAlpha) / 255f) : this.Alpha))
			};
			BR2 = new float[]
			{
				BottomColor.R / 255f,
				BottomColor.G / 255f,
				BottomColor.B / 255f,
				MixColors(BottomColor.A,(InfluencedAlpha ? (byte)(((float)Alpha  * (float)InfluenceAlpha) / 255f) : this.Alpha))
			};
			BL2 = new float[]
			{
				BottomColor.R / 255f,
				BottomColor.G / 255f,
				BottomColor.B / 255f,
				MixColors(BottomColor.A, (InfluencedAlpha ? (byte)(((float)Alpha  * (float)InfluenceAlpha) / 255f) : this.Alpha))
			};
			return new float[][] { TL2, TR2, BR2, BL2 };
		}
	}
}
