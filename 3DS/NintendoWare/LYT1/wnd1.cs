using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using LibEveryFileExplorer.Collections;
using Tao.OpenGl;
using LibEveryFileExplorer.IO;

namespace _3DS.NintendoWare.LYT1
{
	public class wnd1 : pan1
	{
		public enum WindowKind
		{
			Around = 0,
			Horizontal = 1,
			HorizontalNoContent = 2
		}
		public wnd1(EndianBinaryReader er)
			: base(er)
		{
			long basepos = er.BaseStream.Position - 0x4C;
			InflationLeft = er.ReadUInt16() / 16f;
			InflationRight = er.ReadUInt16() / 16f;
			InflationTop = er.ReadUInt16() / 16f;
			InflationBottom = er.ReadUInt16() / 16f;
			FrameSizeLeft = er.ReadUInt16();
			FrameSizeRight = er.ReadUInt16();
			FrameSizeTop = er.ReadUInt16();
			FrameSizeBottom = er.ReadUInt16();
			NrFrames = er.ReadByte();
			byte tmp = er.ReadByte();
			UseLTMaterial = (tmp & 1) == 1;
			UseVtxColorForAllWindow = (tmp & 2) == 2;
			Kind = (WindowKind)((tmp >> 2) & 3);
			DontDrawContent = (tmp & 8) == 16;
			Padding = er.ReadUInt16();
			ContentOffset = er.ReadUInt32();
			FrameOffsetTableOffset = er.ReadUInt32();
			er.BaseStream.Position = basepos + ContentOffset;
			Content = new WindowContent(er);
			er.BaseStream.Position = basepos + FrameOffsetTableOffset;
			WindowFrameOffsets = er.ReadUInt32s(NrFrames);
			WindowFrames = new WindowFrame[NrFrames];
			for (int i = 0; i < NrFrames; i++)
			{
				er.BaseStream.Position = basepos + WindowFrameOffsets[i];
				WindowFrames[i] = new WindowFrame(er);
			}
			er.BaseStream.Position = basepos + SectionSize;
		}
		public float InflationLeft;
		public float InflationRight;
		public float InflationTop;
		public float InflationBottom;

		public UInt16 FrameSizeLeft;
		public UInt16 FrameSizeRight;
		public UInt16 FrameSizeTop;
		public UInt16 FrameSizeBottom;

		public Byte NrFrames;
		//Flags:
		//0:
		public Boolean UseLTMaterial;
		//1:
		public Boolean UseVtxColorForAllWindow;
		//2-3:
		public WindowKind Kind;
		//4:
		public Boolean DontDrawContent;
		//End Flags;
		public UInt16 Padding;
		public UInt32 ContentOffset;
		public UInt32 FrameOffsetTableOffset;
		public WindowContent Content;
		public class WindowContent
		{
			public WindowContent(EndianBinaryReader er)
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
		}
		public UInt32[] WindowFrameOffsets;
		public WindowFrame[] WindowFrames;
		public class WindowFrame
		{
			public enum TexFlip
			{
				None = 0,
				FlipH = 1,
				FlipV = 2,
				Rotate90 = 3,
				Rotate180 = 4,
				Rotate270 = 5
			}
			public WindowFrame(EndianBinaryReader er)
			{
				MaterialId = er.ReadUInt16();
				TextureFlip = (TexFlip)er.ReadByte();
				Padding = er.ReadByte();
			}
			public UInt16 MaterialId;
			public TexFlip TextureFlip;
			public Byte Padding;
		}

		public override void Render(CLYT Layout, CLIM[] Textures, int InfluenceAlpha)
		{
			Gl.glPushMatrix();
			{
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
				Gl.glTranslatef(Translation.X, Translation.Y, Translation.Z);
				Gl.glRotatef(Rotation.X, 1, 0, 0);
				Gl.glRotatef(Rotation.Y, 0, 1, 0);
				Gl.glRotatef(Rotation.Z, 0, 0, 1);
				Gl.glScalef(Scale.X, Scale.Y, 1);
				Gl.glPushMatrix();
				{
					//Translate to origin
					Gl.glTranslatef(-0.5f * Size.X * (float)HAlignment, -0.5f * Size.Y * (-(float)VAlignment), 0);
					switch (Kind)
					{
						case WindowKind.Around:
							if (NrFrames == 1)//1 texture for all
							{
								mat1.MaterialEntry m = Layout.Materials.Materials[WindowFrames[0].MaterialId];
								if (m.TexMaps.Length == 0) RenderContent(Layout, InfluenceAlpha, Size.X, Size.Y);
								else
								{
									var t = Textures[m.TexMaps[0].TexIndex];
									if (t == null) break;
									Gl.glPushMatrix();
									{
										Gl.glTranslatef(t.Image.Width, -t.Image.Height, 0);
										RenderContent(Layout, InfluenceAlpha, Size.X - t.Image.Width * 2, Size.Y - t.Image.Height * 2);
									}
									Gl.glPopMatrix();
									// _________
									//|______|  |
									//|  |   |  |
									//|  |___|__|
									//|__|______|
									//Top Left
									SetupMaterial(Layout, WindowFrames[0].MaterialId);
									float[,] Vertex2 = SetupRect(Size.X - t.Image.Width, t.Image.Height);
									Gl.glBegin(Gl.GL_QUADS);
									Gl.glColor4f(1, 1, 1, (InfluencedAlpha ? (byte)(((float)Alpha * (float)InfluenceAlpha) / 255f) : this.Alpha) / 255f);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 0);
									Gl.glVertex3f(Vertex2[0, 0], Vertex2[0, 1], 0);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, (Size.X - t.Image.Width) / t.Image.Width, 0);
									Gl.glVertex3f(Vertex2[1, 0], Vertex2[1, 1], 0);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, (Size.X - t.Image.Width) / t.Image.Width, 1);
									Gl.glVertex3f(Vertex2[2, 0], Vertex2[2, 1], 0);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 1);
									Gl.glVertex3f(Vertex2[3, 0], Vertex2[3, 1], 0);
									Gl.glEnd();
									//Top Right
									Gl.glPushMatrix();
									{
										Gl.glTranslatef(Size.X - t.Image.Width, 0, 0);
										Vertex2 = SetupRect(t.Image.Width, Size.Y - t.Image.Height);
										Gl.glBegin(Gl.GL_QUADS);
										Gl.glColor4f(1, 1, 1, (InfluencedAlpha ? (byte)(((float)Alpha * (float)InfluenceAlpha) / 255f) : this.Alpha) / 255f);

										Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, 0);
										Gl.glVertex3f(Vertex2[0, 0], Vertex2[0, 1], 0);

										Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 0);
										Gl.glVertex3f(Vertex2[1, 0], Vertex2[1, 1], 0);

										Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, (Size.Y - t.Image.Height) / t.Image.Height);
										Gl.glVertex3f(Vertex2[2, 0], Vertex2[2, 1], 0);

										Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, (Size.Y - t.Image.Height) / t.Image.Height);
										Gl.glVertex3f(Vertex2[3, 0], Vertex2[3, 1], 0);
										Gl.glEnd();
									}
									Gl.glPopMatrix();
									//Bottom Right
									Gl.glPushMatrix();
									{
										Gl.glTranslatef(t.Image.Width, -(Size.Y - t.Image.Height), 0);
										Vertex2 = SetupRect(Size.X - t.Image.Width, t.Image.Height);
										Gl.glBegin(Gl.GL_QUADS);
										Gl.glColor4f(1, 1, 1, (InfluencedAlpha ? (byte)(((float)Alpha * (float)InfluenceAlpha) / 255f) : this.Alpha) / 255f);

										Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, (Size.X - t.Image.Width) / t.Image.Width, 1);
										Gl.glVertex3f(Vertex2[0, 0], Vertex2[0, 1], 0);

										Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 1);
										Gl.glVertex3f(Vertex2[1, 0], Vertex2[1, 1], 0);

										Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 0);
										Gl.glVertex3f(Vertex2[2, 0], Vertex2[2, 1], 0);

										Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, (Size.X - t.Image.Width) / t.Image.Width, 0);
										Gl.glVertex3f(Vertex2[3, 0], Vertex2[3, 1], 0);
										Gl.glEnd();
									}
									Gl.glPopMatrix();
									//Bottom Left
									Gl.glPushMatrix();
									{
										Gl.glTranslatef(0, -t.Image.Height, 0);
										Vertex2 = SetupRect(t.Image.Width, Size.Y - t.Image.Height);
										Gl.glBegin(Gl.GL_QUADS);
										Gl.glColor4f(1, 1, 1, (InfluencedAlpha ? (byte)(((float)Alpha * (float)InfluenceAlpha) / 255f) : this.Alpha) / 255f);

										Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, (Size.Y - t.Image.Height) / t.Image.Height);
										Gl.glVertex3f(Vertex2[0, 0], Vertex2[0, 1], 0);

										Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, (Size.Y - t.Image.Height) / t.Image.Height);
										Gl.glVertex3f(Vertex2[1, 0], Vertex2[1, 1], 0);

										Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, 0);
										Gl.glVertex3f(Vertex2[2, 0], Vertex2[2, 1], 0);

										Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 0);
										Gl.glVertex3f(Vertex2[3, 0], Vertex2[3, 1], 0);
										Gl.glEnd();
									}
									Gl.glPopMatrix();
								}
							}
							else if (NrFrames == 4)//Corners
							{
								var t1 = Textures[Layout.Materials.Materials[WindowFrames[0].MaterialId].TexMaps[0].TexIndex];
								var t2 = Textures[Layout.Materials.Materials[WindowFrames[1].MaterialId].TexMaps[0].TexIndex];
								var t3 = Textures[Layout.Materials.Materials[WindowFrames[2].MaterialId].TexMaps[0].TexIndex];
								var t4 = Textures[Layout.Materials.Materials[WindowFrames[3].MaterialId].TexMaps[0].TexIndex];
								if (t1 == null || t2 == null || t3 == null || t4 == null) break;
								Gl.glPushMatrix();
								{
									Gl.glTranslatef(t4.Image.Width, -t1.Image.Height, 0);
									RenderContent(Layout, InfluenceAlpha, Size.X - (t4.Image.Width + t2.Image.Width), Size.Y - (t1.Image.Height + t3.Image.Height));
								}
								Gl.glPopMatrix();
								// _________
								//|______|  |
								//|  |   |  |
								//|  |___|__|
								//|__|______|
								//Top Left
								SetupMaterial(Layout, WindowFrames[0].MaterialId);
								float[,] Vertex2 = SetupRect(Size.X - t2.Image.Width, t1.Image.Height);
								Gl.glBegin(Gl.GL_QUADS);
								Gl.glColor4f(1, 1, 1, (InfluencedAlpha ? (byte)(((float)Alpha * (float)InfluenceAlpha) / 255f) : this.Alpha) / 255f);

								Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 0);
								Gl.glVertex3f(Vertex2[0, 0], Vertex2[0, 1], 0);

								Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, (Size.X - t2.Image.Width) / t1.Image.Width, 0);
								Gl.glVertex3f(Vertex2[1, 0], Vertex2[1, 1], 0);

								Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, (Size.X - t2.Image.Width) / t1.Image.Width, 1);
								Gl.glVertex3f(Vertex2[2, 0], Vertex2[2, 1], 0);

								Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 1);
								Gl.glVertex3f(Vertex2[3, 0], Vertex2[3, 1], 0);
								Gl.glEnd();
								//Top Right
								Gl.glPushMatrix();
								{
									Gl.glTranslatef(Size.X - t2.Image.Width, 0, 0);
									SetupMaterial(Layout, WindowFrames[1].MaterialId);
									Vertex2 = SetupRect(t2.Image.Width, Size.Y - t3.Image.Height);
									Gl.glBegin(Gl.GL_QUADS);
									Gl.glColor4f(1, 1, 1, (InfluencedAlpha ? (byte)(((float)Alpha * (float)InfluenceAlpha) / 255f) : this.Alpha) / 255f);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 0);
									Gl.glVertex3f(Vertex2[0, 0], Vertex2[0, 1], 0);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, 0);
									Gl.glVertex3f(Vertex2[1, 0], Vertex2[1, 1], 0);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, (Size.Y - t3.Image.Height) / t2.Image.Height);
									Gl.glVertex3f(Vertex2[2, 0], Vertex2[2, 1], 0);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, (Size.Y - t3.Image.Height) / t2.Image.Height);
									Gl.glVertex3f(Vertex2[3, 0], Vertex2[3, 1], 0);
									Gl.glEnd();
								}
								Gl.glPopMatrix();
								//Bottom Right
								Gl.glPushMatrix();
								{
									Gl.glTranslatef(t4.Image.Width, -(Size.Y - t3.Image.Height), 0);
									SetupMaterial(Layout, WindowFrames[2].MaterialId);
									Vertex2 = SetupRect(Size.X - t4.Image.Width, t3.Image.Height);
									Gl.glBegin(Gl.GL_QUADS);
									Gl.glColor4f(1, 1, 1, (InfluencedAlpha ? (byte)(((float)Alpha * (float)InfluenceAlpha) / 255f) : this.Alpha) / 255f);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, (Size.X - t2.Image.Width) / t3.Image.Width, 0);
									Gl.glVertex3f(Vertex2[0, 0], Vertex2[0, 1], 0);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 0);
									Gl.glVertex3f(Vertex2[1, 0], Vertex2[1, 1], 0);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 1);
									Gl.glVertex3f(Vertex2[2, 0], Vertex2[2, 1], 0);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, (Size.X - t2.Image.Width) / t3.Image.Width, 1);
									Gl.glVertex3f(Vertex2[3, 0], Vertex2[3, 1], 0);
									Gl.glEnd();
								}
								Gl.glPopMatrix();
								//Bottom Left
								Gl.glPushMatrix();
								{
									Gl.glTranslatef(0, -t1.Image.Height, 0);
									SetupMaterial(Layout, WindowFrames[3].MaterialId);
									Vertex2 = SetupRect(t4.Image.Width, Size.Y - t1.Image.Height);
									Gl.glBegin(Gl.GL_QUADS);
									Gl.glColor4f(1, 1, 1, (InfluencedAlpha ? (byte)(((float)Alpha * (float)InfluenceAlpha) / 255f) : this.Alpha) / 255f);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, -(Size.Y - t1.Image.Height) / t4.Image.Height); 
									Gl.glVertex3f(Vertex2[0, 0], Vertex2[0, 1], 0);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, -(Size.Y - t1.Image.Height) / t4.Image.Height);
									Gl.glVertex3f(Vertex2[1, 0], Vertex2[1, 1], 0);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 1);
									Gl.glVertex3f(Vertex2[2, 0], Vertex2[2, 1], 0);

									Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, 1); 
									Gl.glVertex3f(Vertex2[3, 0], Vertex2[3, 1], 0);
									Gl.glEnd();
								}
								Gl.glPopMatrix();
							}
							else if (NrFrames == 8)//all
							{
								RenderContent(Layout, InfluenceAlpha, Size.X, Size.Y);
							}
							else
							{
								RenderContent(Layout, InfluenceAlpha, Size.X, Size.Y);
								//not possible?
							}
							break;
						default:
							break;
					}
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
				Content.VertexColorLT.R / 255f,
				Content.VertexColorLT.G / 255f,
				Content.VertexColorLT.B / 255f,
				MixColors(Content.VertexColorLT.A, (InfluencedAlpha ? (byte)(((float)Alpha  * (float)InfluenceAlpha) / 255f) : this.Alpha))
			};
			TR2 = new float[]
			{
				Content.VertexColorRT.R / 255f,
				Content.VertexColorRT.G / 255f,
				Content.VertexColorRT.B / 255f,
				MixColors(Content.VertexColorRT.A, (InfluencedAlpha ? (byte)(((float)Alpha  * (float)InfluenceAlpha) / 255f) : this.Alpha))
			};
			BR2 = new float[]
			{
				Content.VertexColorRB.R / 255f,
				Content.VertexColorRB.G / 255f,
				Content.VertexColorRB.B / 255f,
				MixColors(Content.VertexColorRB.A,(InfluencedAlpha ? (byte)(((float)Alpha  * (float)InfluenceAlpha) / 255f) : this.Alpha))
			};
			BL2 = new float[]
			{
				Content.VertexColorLB.R / 255f,
				Content.VertexColorLB.G / 255f,
				Content.VertexColorLB.B / 255f,
				MixColors(Content.VertexColorLB.A, (InfluencedAlpha ? (byte)(((float)Alpha  * (float)InfluenceAlpha) / 255f) : this.Alpha))
			};
			return new float[][] { TL2, TR2, BR2, BL2 };
		}

		private void RenderContent(CLYT Layout, int InfluenceAlpha, float Width, float Height)
		{
			SetupMaterial(Layout, Content.MaterialId);

			float[,] Vertex2 = SetupRect(Width, Height);
			float[][] VtxColor = SetupVtxColors(InfluenceAlpha);
			Gl.glBegin(Gl.GL_QUADS);

			for (int o = 0; o < Content.TexCoordEntries.Length; o++)
			{
				Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0 + o,
					Content.TexCoordEntries[o].TexCoordLT.X, Content.TexCoordEntries[o].TexCoordLT.Y);
			}
			if (Content.TexCoordEntries.Length == 0) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 0);
			Gl.glColor4f(VtxColor[0][0], VtxColor[0][1], VtxColor[0][2], VtxColor[0][3]);
			Gl.glVertex3f(Vertex2[0, 0], Vertex2[0, 1], 0);
			for (int o = 0; o < Content.TexCoordEntries.Length; o++)
			{
				Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0 + o,
					Content.TexCoordEntries[o].TexCoordRT.X, Content.TexCoordEntries[o].TexCoordRT.Y);
			}
			if (Content.TexCoordEntries.Length == 0) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, 0);
			Gl.glColor4f(VtxColor[1][0], VtxColor[1][1], VtxColor[1][2], VtxColor[1][3]);
			Gl.glVertex3f(Vertex2[1, 0], Vertex2[1, 1], 0);
			for (int o = 0; o < Content.TexCoordEntries.Length; o++)
			{
				Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0 + o,
					Content.TexCoordEntries[o].TexCoordRB.X, Content.TexCoordEntries[o].TexCoordRB.Y);
			}
			if (Content.TexCoordEntries.Length == 0) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 1, 1);
			Gl.glColor4f(VtxColor[2][0], VtxColor[2][1], VtxColor[2][2], VtxColor[2][3]);
			Gl.glVertex3f(Vertex2[2, 0], Vertex2[2, 1], 0);
			for (int o = 0; o < Content.TexCoordEntries.Length; o++)
			{
				Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0 + o,
					Content.TexCoordEntries[o].TexCoordLB.X, Content.TexCoordEntries[o].TexCoordLB.Y);
			}
			if (Content.TexCoordEntries.Length == 0) Gl.glMultiTexCoord2f(Gl.GL_TEXTURE0, 0, 1);
			Gl.glColor4f(VtxColor[3][0], VtxColor[3][1], VtxColor[3][2], VtxColor[3][3]);
			Gl.glVertex3f(Vertex2[3, 0], Vertex2[3, 1], 0);
			Gl.glEnd();
		}
	}
}
