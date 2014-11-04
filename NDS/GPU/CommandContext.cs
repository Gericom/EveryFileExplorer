using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Tao.OpenGl;
using LibEveryFileExplorer.GFX;
using LibEveryFileExplorer.Collections;
using LibEveryFileExplorer.IO;

namespace NDS.GPU
{
	public class CommandContext
	{
		public CommandContext()
		{
			for (int i = 0; i < 31; i++)
			{
				PosMtxStack[i] = Matrix44.Identity;
				DirMtxStack[i] = Matrix44.Identity;
			}
		}

		private Color[] LightColors = new Color[]
		{
			System.Drawing.Color.White,
			System.Drawing.Color.White,
			System.Drawing.Color.White,
			System.Drawing.Color.White
		};

		private Vector3[] LightVectors = new Vector3[]
		{
			new Vector3( 0,			-1,			-1			),
			new Vector3( 0.998047f,	-1,			 0			),
			new Vector3( 0,			-1,			 0.998047f	),
			new Vector3(-1,			-1,			 0			)
		};

		private Byte[] SpecularReflectionTable = new byte[]
		{
			0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30,
			32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 60, 62, 
			64, 66, 68, 70, 72, 74, 76, 78, 80, 82, 84, 86, 88, 90, 92, 94,
			96, 98, 100, 102, 104, 106, 108, 110, 112, 114, 116, 118, 120, 122, 124, 126,
			129, 131, 133, 135, 137, 139, 141, 143, 145, 147, 149, 151, 153, 155, 157, 159,
			161, 163, 165, 167, 169, 171, 173, 175, 177, 179, 181, 183, 185, 187, 189, 191,
			193, 195, 197, 199, 201, 203, 205, 207, 209, 211, 213, 215, 217, 219, 221, 223,
			225, 227, 229, 231, 233, 235, 237, 239, 241, 243, 245, 247, 249, 251, 253, 255
		};
		private bool UsesSpecularReflectionTable = false;
		private bool[] LightEnabled = new bool[4];

		//private Matrix4[] PrjMtxStack = new Matrix4[1];
		private Matrix44[] PosMtxStack = new Matrix44[31];
		private Matrix44[] DirMtxStack = new Matrix44[31];
		//private Matrix4[] TexMtxStack = new Matrix4[1];
		private Matrix44 CurPosMtx = Matrix44.Identity;
		private Matrix44 CurDirMtx = Matrix44.Identity;
		private int StackPtr = 0;
		private NDSMatrixMode MtxMode = NDSMatrixMode.Position_Vector;

		private Vector3 LastSetVtx = new Vector3();

		private Color DiffuseColor = System.Drawing.Color.White;
		private Color AmbientColor;
		private Color SpecularColor;
		private Color EmissionColor;
		private int Alpha = 31;

		public void RunDL(byte[] DL)
		{
			int Offset = 0;
			Queue<byte> CommandQueue = new Queue<byte>();
			while (Offset < DL.Length)
			{
				if (CommandQueue.Count == 0)
				{
					CommandQueue.Enqueue(DL[Offset++]);
					CommandQueue.Enqueue(DL[Offset++]);
					CommandQueue.Enqueue(DL[Offset++]);
					CommandQueue.Enqueue(DL[Offset++]);
				}
				byte cmd = CommandQueue.Dequeue();
				switch (cmd)
				{
					case 0: Nop(); break;
					case 0x10: MatrixMode((NDSMatrixMode)IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x11: PushMatrix(); break;
					case 0x12: PopMatrix(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x13: StoreMatrix(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x14: RestoreMatrix(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x15: Identity(); break;
					case 0x16:
						{
							uint[] mtx = new uint[16];
							for (int i = 0; i < 16; i++) { mtx[i] = IOUtil.ReadU32LE(DL, Offset); Offset += 4; }
							LoadMatrix44(mtx);
							break;
						}
					case 0x17:
						{
							uint[] mtx = new uint[12];
							for (int i = 0; i < 12; i++) { mtx[i] = IOUtil.ReadU32LE(DL, Offset); Offset += 4; }
							LoadMatrix43(mtx);
							break;
						}
					case 0x18:
						{
							uint[] mtx = new uint[16];
							for (int i = 0; i < 16; i++) { mtx[i] = IOUtil.ReadU32LE(DL, Offset); Offset += 4; }
							MultMatrix44(mtx);
							break;
						}
					case 0x19:
						{
							uint[] mtx = new uint[12];
							for (int i = 0; i < 12; i++) { mtx[i] = IOUtil.ReadU32LE(DL, Offset); Offset += 4; }
							MultMatrix43(mtx);
							break;
						}
					case 0x1A:
						{
							uint[] mtx = new uint[9];
							for (int i = 0; i < 9; i++) { mtx[i] = IOUtil.ReadU32LE(DL, Offset); Offset += 4; }
							MultMatrix33(mtx);
							break;
						}
					case 0x1b:
						Scale(IOUtil.ReadU32LE(DL, Offset), IOUtil.ReadU32LE(DL, Offset + 4), IOUtil.ReadU32LE(DL, Offset + 8));
						Offset += 12;
						break;
					case 0x1c:
						Translate(IOUtil.ReadU32LE(DL, Offset), IOUtil.ReadU32LE(DL, Offset + 4), IOUtil.ReadU32LE(DL, Offset + 8));
						Offset += 12;
						break;
					case 0x20: Color(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x21: Normal(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x22: TexCoord(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x23: Vertex(IOUtil.ReadU32LE(DL, Offset), IOUtil.ReadU32LE(DL, Offset + 4)); Offset += 8; break;
					case 0x24: Vertex10(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x25: VertexXY(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x26: VertexXZ(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x27: VertexYZ(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x28: VertexDiff(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x29: PolygonAttr(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x2A: TexImageParam(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x2B: TexPlttBase(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x30: MaterialColor0(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x31: MaterialColor1(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x32: LightVector(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x33: LightColor(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x34:
						{
							uint[] shine = new uint[32];
							for (int i = 0; i < 32; i++) { shine[i] = IOUtil.ReadU32LE(DL, Offset); Offset += 4; }
							Shininess(shine);
							break;
						}
					case 0x40: Begin((NDSPrimitiveType)IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x41: End(); break;
					case 0x50: SwapBuffers(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x60: ViewPort(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					case 0x70: BoxTest(IOUtil.ReadU32LE(DL, Offset), IOUtil.ReadU32LE(DL, Offset + 4), IOUtil.ReadU32LE(DL, Offset + 8)); Offset += 12; break;
					case 0x71: PositionTest(IOUtil.ReadU32LE(DL, Offset), IOUtil.ReadU32LE(DL, Offset + 4)); Offset += 8; break;
					case 0x72: VectorTest(IOUtil.ReadU32LE(DL, Offset)); Offset += 4; break;
					default:
						break;
				}
			}
		}

		public void Nop() { }
		public void MatrixMode(NDSMatrixMode mode)
		{
			MtxMode = mode;
		}
		public void PushMatrix()
		{
			if (MtxMode == NDSMatrixMode.Position || MtxMode == NDSMatrixMode.Position_Vector) PosMtxStack[StackPtr] = CurPosMtx;
			if (MtxMode == NDSMatrixMode.Position_Vector) DirMtxStack[StackPtr] = CurDirMtx;
			StackPtr++;
		}
		public void PopMatrix(uint cmd)
		{
			int num = (int)cmd;
			PopMatrix((num << 26) >> 26);
		}
		public void PopMatrix(int num)
		{
			StackPtr -= num;
			if (MtxMode == NDSMatrixMode.Position || MtxMode == NDSMatrixMode.Position_Vector) CurPosMtx = PosMtxStack[StackPtr];
			if (MtxMode == NDSMatrixMode.Position_Vector) CurDirMtx = DirMtxStack[StackPtr];
		}
		public void StoreMatrix(uint index)
		{
			index &= 0x1F;
			if (MtxMode == NDSMatrixMode.Position || MtxMode == NDSMatrixMode.Position_Vector) PosMtxStack[index] = CurPosMtx;
			if (MtxMode == NDSMatrixMode.Position_Vector) DirMtxStack[index] = CurDirMtx;
		}
		public void RestoreMatrix(uint index)
		{
			index &= 0x1F;
			if (MtxMode == NDSMatrixMode.Position || MtxMode == NDSMatrixMode.Position_Vector) CurPosMtx = PosMtxStack[index];
			if (MtxMode == NDSMatrixMode.Position_Vector) CurDirMtx = DirMtxStack[index];
		}
		public void Identity()
		{
			if (MtxMode == NDSMatrixMode.Position || MtxMode == NDSMatrixMode.Position_Vector) CurPosMtx = Matrix44.Identity;
			if (MtxMode == NDSMatrixMode.Position_Vector) CurDirMtx = Matrix44.Identity;
		}
		public void LoadMatrix44(uint[] mtx)
		{
			LoadMatrix44(new Matrix44(
				(int)mtx[0] / 4096f, (int)mtx[1] / 4096f, (int)mtx[2] / 4096f, (int)mtx[3] / 4096f,
				(int)mtx[4] / 4096f, (int)mtx[5] / 4096f, (int)mtx[6] / 4096f, (int)mtx[7] / 4096f,
				(int)mtx[8] / 4096f, (int)mtx[9] / 4096f, (int)mtx[10] / 4096f, (int)mtx[11] / 4096f,
				(int)mtx[12] / 4096f, (int)mtx[13] / 4096f, (int)mtx[14] / 4096f, (int)mtx[15] / 4096f));
		}
		public void LoadMatrix44(Matrix44 mtx)
		{
			if (MtxMode == NDSMatrixMode.Position || MtxMode == NDSMatrixMode.Position_Vector) CurPosMtx = mtx;
			if (MtxMode == NDSMatrixMode.Position_Vector) CurDirMtx = mtx;
		}
		public void LoadMatrix43(uint[] mtx)
		{
			LoadMatrix43(new Matrix43(
				(int)mtx[0] / 4096f, (int)mtx[1] / 4096f, (int)mtx[2] / 4096f,
				(int)mtx[3] / 4096f, (int)mtx[4] / 4096f, (int)mtx[5] / 4096f,
				(int)mtx[6] / 4096f, (int)mtx[7] / 4096f, (int)mtx[8] / 4096f,
				(int)mtx[9] / 4096f, (int)mtx[10] / 4096f, (int)mtx[11] / 4096f));
		}
		public void LoadMatrix43(Matrix43 mtx)
		{
			if (MtxMode == NDSMatrixMode.Position || MtxMode == NDSMatrixMode.Position_Vector) CurPosMtx = new Matrix44(mtx, new Vector4(0, 0, 0, 1));
			if (MtxMode == NDSMatrixMode.Position_Vector) CurDirMtx = new Matrix44(mtx, new Vector4(0, 0, 0, 1));
		}
		public void MultMatrix44(uint[] mtx)
		{
			MultMatrix44(new Matrix44(
				(int)mtx[0] / 4096f, (int)mtx[1] / 4096f, (int)mtx[2] / 4096f, (int)mtx[3] / 4096f,
				(int)mtx[4] / 4096f, (int)mtx[5] / 4096f, (int)mtx[6] / 4096f, (int)mtx[7] / 4096f,
				(int)mtx[8] / 4096f, (int)mtx[9] / 4096f, (int)mtx[10] / 4096f, (int)mtx[11] / 4096f,
				(int)mtx[12] / 4096f, (int)mtx[13] / 4096f, (int)mtx[14] / 4096f, (int)mtx[15] / 4096f));
		}
		public void MultMatrix44(Matrix44 mtx)
		{
			if (MtxMode == NDSMatrixMode.Position || MtxMode == NDSMatrixMode.Position_Vector) CurPosMtx = CurPosMtx * mtx;
			if (MtxMode == NDSMatrixMode.Position_Vector) CurDirMtx = CurDirMtx * mtx;
		}
		public void MultMatrix43(uint[] mtx)
		{
			MultMatrix43(new Matrix43(
				(int)mtx[0] / 4096f, (int)mtx[1] / 4096f, (int)mtx[2] / 4096f,
				(int)mtx[3] / 4096f, (int)mtx[4] / 4096f, (int)mtx[5] / 4096f,
				(int)mtx[6] / 4096f, (int)mtx[7] / 4096f, (int)mtx[8] / 4096f,
				(int)mtx[9] / 4096f, (int)mtx[10] / 4096f, (int)mtx[11] / 4096f));
		}
		public void MultMatrix43(Matrix43 mtx)
		{
			if (MtxMode == NDSMatrixMode.Position || MtxMode == NDSMatrixMode.Position_Vector) CurPosMtx = CurPosMtx * new Matrix44(mtx, new Vector4(0, 0, 0, 1));
			if (MtxMode == NDSMatrixMode.Position_Vector) CurDirMtx = CurDirMtx * new Matrix44(mtx, new Vector4(0, 0, 0, 1));
		}
		public void MultMatrix33(uint[] mtx)
		{
			MultMatrix33(new Matrix33(
				(int)mtx[0] / 4096f, (int)mtx[1] / 4096f, (int)mtx[2] / 4096f,
				(int)mtx[3] / 4096f, (int)mtx[4] / 4096f, (int)mtx[5] / 4096f,
				(int)mtx[6] / 4096f, (int)mtx[7] / 4096f, (int)mtx[8] / 4096f));
		}
		public void MultMatrix33(Matrix33 mtx)
		{
			if (MtxMode == NDSMatrixMode.Position || MtxMode == NDSMatrixMode.Position_Vector) CurPosMtx = CurPosMtx * new Matrix44(mtx, 0, 0, 0, new Vector4(0, 0, 0, 1));
			if (MtxMode == NDSMatrixMode.Position_Vector) CurDirMtx = CurDirMtx * new Matrix44(mtx, 0, 0, 0, new Vector4(0, 0, 0, 1));
		}
		public void Scale(uint x, uint y, uint z)
		{
			Scale(new Vector3((int)x / 4096f, (int)y / 4096f, (int)z / 4096f));
		}
		public void Scale(Vector3 scale)
		{
			Matrix44 m = Matrix44.CreateScale(scale);
			if (MtxMode == NDSMatrixMode.Position || MtxMode == NDSMatrixMode.Position_Vector) CurPosMtx = CurPosMtx * m;
		}
		public void Translate(uint x, uint y, uint z)
		{
			Translate(new Vector3((int)x / 4096f, (int)y / 4096f, (int)z / 4096f));
		}
		public void Translate(Vector3 translation)
		{
			Matrix44 m = Matrix44.CreateTranslation(translation);
			if (MtxMode == NDSMatrixMode.Position || MtxMode == NDSMatrixMode.Position_Vector) CurPosMtx = CurPosMtx * m;
			if (MtxMode == NDSMatrixMode.Position_Vector) CurDirMtx = CurDirMtx * m;
		}
		public void Color(uint color)
		{
			Color(System.Drawing.Color.FromArgb((int)GFXUtil.XBGR1555ToArgb((ushort)color)));
		}
		public void Color(Color color)
		{
			Gl.glColor4f(color.R / 255f, color.G / 255f, color.B / 255f, Alpha / 31f);
		}
		public void Normal(uint normal)
		{
			Vector3 norm = new Vector3();
			norm.X = ((short)(((normal >> 0) & 0x3FF) << 6) >> 6) / 512f;
			norm.Y = ((short)(((normal >> 10) & 0x3FF) << 6) >> 6) / 512f;
			norm.Z = ((short)(((normal >> 20) & 0x3FF) << 6) >> 6) / 512f;
			Normal(norm);
		}
		public void Normal(Vector3 normal)
		{
			Matrix44 m = CurDirMtx;
			m[3, 0] = m[3, 1] = m[3, 2] = 0;

			normal *= m;

			Vector3[] D = new Vector3[4];
			Vector3[] A = new Vector3[4];
			Vector3[] S = new Vector3[4];

			for (int l = 0; l < 4; l++)
			{
				if (!LightEnabled[l]) continue;
				float ld = Math.Max(0, (-LightVectors[l]).Dot(normal));
				ld = Math.Min(ld, 1);
				ld = Math.Max(ld, 0);
				D[l] = (ld * ColorToVector3(LightColors[l])) * ColorToVector3(DiffuseColor);
				A[l] = ColorToVector3(LightColors[l]) * ColorToVector3(AmbientColor);

				Vector3 H = (LightVectors[l] + new Vector3(0, 0, -1)) / 2f;

				float ls = Math.Max(0, (float)Math.Cos(2 * (-H).Angle(normal)));

				ls = Math.Min(ls, 1);
				ls = Math.Max(ls, 0);

				if (UsesSpecularReflectionTable) S[l] = (SpecularReflectionTable[(int)(ls * 127)] / 255f * ColorToVector3(LightColors[l])) * ColorToVector3(SpecularColor);
				else S[l] = (ls * ColorToVector3(LightColors[l])) * ColorToVector3(SpecularColor);
			}

			Vector3 E = ColorToVector3(EmissionColor);

			Vector3 VertexColor = E;

			for (int l = 0; l < 4; l++)
			{
				if (!LightEnabled[l]) continue;
				VertexColor += D[l] + A[l] + S[l];
			}

			VertexColor.X = Math.Min(1, VertexColor.X);
			VertexColor.Y = Math.Min(1, VertexColor.Y);
			VertexColor.Z = Math.Min(1, VertexColor.Z);

			Gl.glColor4f(VertexColor.X, VertexColor.Y, VertexColor.Z, Alpha / 31f);
		}
		public void TexCoord(uint texcoord)
		{
			TexCoord(new Vector2(((short)((texcoord >> 0) & 0xFFFF)) / 16f, ((short)((texcoord >> 16) & 0xFFFF)) / 16f));
		}
		public void TexCoord(Vector2 texcoord)
		{
			Gl.glTexCoord2f(texcoord.X, texcoord.Y);
		}
		public void Vertex(uint cmd1, uint cmd2)
		{
			float X = ((short)((cmd1 >> 0) & 0xFFFF)) / 4096f;
			float Y = ((short)((cmd1 >> 16) & 0xFFFF)) / 4096f;
			float Z = ((short)((cmd2 >> 0) & 0xFFFF)) / 4096f;
			Vertex(new Vector3(X, Y, Z));
		}
		public void Vertex(Vector3 vtx)
		{
			LastSetVtx = vtx;
			Vector3 result = vtx * CurPosMtx;
			Gl.glVertex3f(result.X, result.Y, result.Z);
		}
		public void Vertex10(uint vtx)
		{
			float X = ((short)(((vtx >> 0) & 0x3FF) << 6) >> 6) / 64f;
			float Y = ((short)(((vtx >> 10) & 0x3FF) << 6) >> 6) / 64f;
			float Z = ((short)(((vtx >> 20) & 0x3FF) << 6) >> 6) / 64f;
			Vertex(new Vector3(X, Y, Z));
		}
		public void VertexXY(uint vtx)
		{
			float X = ((short)((vtx >> 0) & 0xFFFF)) / 4096f;
			float Y = ((short)((vtx >> 16) & 0xFFFF)) / 4096f;
			VertexXY(new Vector2(X, Y));
		}
		public void VertexXY(Vector2 XY)
		{
			Vertex(new Vector3(XY, LastSetVtx.Z));
		}
		public void VertexXZ(uint vtx)
		{
			float X = ((short)((vtx >> 0) & 0xFFFF)) / 4096f;
			float Z = ((short)((vtx >> 16) & 0xFFFF)) / 4096f;
			VertexXZ(new Vector2(X, Z));
		}
		public void VertexXZ(Vector2 XZ)
		{
			Vertex(new Vector3(XZ.X, LastSetVtx.Y, XZ.Y));
		}
		public void VertexYZ(uint vtx)
		{
			float Y = ((short)((vtx >> 0) & 0xFFFF)) / 4096f;
			float Z = ((short)((vtx >> 16) & 0xFFFF)) / 4096f;
			VertexYZ(new Vector2(Y, Z));
		}
		public void VertexYZ(Vector2 YZ)
		{
			Vertex(new Vector3(LastSetVtx.X, YZ.X, YZ.Y));
		}
		public void VertexDiff(uint vtx)
		{
			float X = ((short)(((vtx >> 0) & 0x3FF) << 6) >> 6) / 4096f;
			float Y = ((short)(((vtx >> 10) & 0x3FF) << 6) >> 6) / 4096f;
			float Z = ((short)(((vtx >> 20) & 0x3FF) << 6) >> 6) / 4096f;
			Vertex(new Vector3(X, Y, Z) + LastSetVtx);
		}
		public void PolygonAttr(uint cmd)
		{
			LightEnabled[0] = ((cmd >> 0) & 0x1) == 1;
			LightEnabled[1] = ((cmd >> 1) & 0x1) == 1;
			LightEnabled[2] = ((cmd >> 2) & 0x1) == 1;
			LightEnabled[3] = ((cmd >> 3) & 0x1) == 1;
			switch ((cmd >> 4) & 0x3)
			{
				case 0: Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE); break;
				case 1: Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_DECAL); break;
				case 2:
					//Toon/highlight shading. For debugging look at player files from zelda spirit tracks!
					//There's no toon table in a nsbmd, so use default shading
					//TODO: Implement a default toon table
					Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);
					break;
				case 3:
					//System.Windows.MessageBox.Show("SHADOW!");
					Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);
					break;
			}
			//Don't enable, but let the user choose to use it or not
			switch ((cmd >> 6) & 0x03)
			{
				case 0x03: Gl.glCullFace(Gl.GL_NONE); break;
				case 0x02: Gl.glCullFace(Gl.GL_BACK); break;
				case 0x01: Gl.glCullFace(Gl.GL_FRONT); break;
				case 0x00: Gl.glCullFace(Gl.GL_FRONT_AND_BACK); break;
			}
			switch ((cmd >> 14) & 0x1)
			{
				case 0: Gl.glDepthFunc(Gl.GL_LESS); break;
				case 1:
					//System.Windows.MessageBox.Show("EQUALS!");
					//Gl.glDepthFunc(Gl.GL_EQUAL);
					Gl.glDepthFunc(Gl.GL_LESS);
					break;
			}

			Alpha = (int)((cmd >> 16) & 31);
		}
		public void TexImageParam(uint cmd)
		{

		}
		public void TexPlttBase(uint cmd)
		{

		}
		public void MaterialColor0(uint cmd)
		{
			ushort diff = (ushort)(cmd & 0x7FFF);
			bool vtx = (cmd & 0x8000) != 0;
			ushort amb = (ushort)((cmd >> 16) & 0x7FFF);
			DiffuseColor = System.Drawing.Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(diff));
			AmbientColor = System.Drawing.Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(amb));
			if (vtx) Color(DiffuseColor);
		}
		public void MaterialColor1(uint cmd)
		{
			ushort spec = (ushort)(cmd & 0x7FFF);
			bool shine = (cmd & 0x8000) != 0;
			ushort emiss = (ushort)((cmd >> 16) & 0x7FFF);
			UsesSpecularReflectionTable = shine;
			SpecularColor = System.Drawing.Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(spec));
			EmissionColor = System.Drawing.Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(emiss));
		}
		public void Shininess(uint[] SpecReflectTable)
		{
			byte[] refl = new byte[128];
			for (int i = 0; i < 32; i++)
			{
				refl[i * 4 + 0] = (byte)(SpecReflectTable[i] & 0xFF);
				refl[i * 4 + 1] = (byte)((SpecReflectTable[i] >> 8) & 0xFF);
				refl[i * 4 + 2] = (byte)((SpecReflectTable[i] >> 16) & 0xFF);
				refl[i * 4 + 3] = (byte)((SpecReflectTable[i] >> 24) & 0xFF);
			}
			Shininess(refl);
		}
		public void Shininess(byte[] SpecReflectTable)
		{
			Array.Copy(SpecReflectTable, SpecularReflectionTable, 128);
		}
		public void LightVector(uint cmd)
		{

		}
		public void LightColor(uint cmd)
		{

		}
		public void Begin(NDSPrimitiveType type)
		{
			switch (type)
			{
				case NDSPrimitiveType.Triangle:
					Gl.glBegin(Gl.GL_TRIANGLES);
					break;
				case NDSPrimitiveType.Quadrilateral:
					Gl.glBegin(Gl.GL_QUADS);
					break;
				case NDSPrimitiveType.TriangleStrips:
					Gl.glBegin(Gl.GL_TRIANGLE_STRIP);
					break;
				case NDSPrimitiveType.QuadrilateralStrips:
					Gl.glBegin(Gl.GL_QUAD_STRIP);
					break;
			}
		}
		public void End()
		{
			Gl.glEnd();
		}
		public void SwapBuffers(uint cmd)
		{

		}
		public void ViewPort(uint cmd)
		{

		}
		public void BoxTest(uint cmd1, uint cmd2, uint cmd3)
		{

		}
		public void PositionTest(uint cmd1, uint cmd2)
		{

		}
		public void VectorTest(uint cmd)
		{

		}

		public Matrix44 GetCurPosMtx()
		{
			return CurPosMtx;
		}

		private Vector3 ColorToVector3(Color c)
		{
			return new Vector3(c.R / 255f, c.G / 255f, c.B / 255f);
		}

		public enum NDSMatrixMode : int
		{
			Projection = 0,
			Position = 1,
			Position_Vector = 2,
			Texture = 3
		}

		public enum NDSPrimitiveType : int
		{
			Triangle = 0,
			Quadrilateral = 1,
			TriangleStrips = 2,
			QuadrilateralStrips = 3
		}
	}
}
