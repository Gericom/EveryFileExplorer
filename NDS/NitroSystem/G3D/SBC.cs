using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDS.GPU;
using Tao.OpenGl;
using LibEveryFileExplorer.GFX;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.Collections;

namespace NDS.NitroSystem.G3D
{
	public class SBC
	{
		private byte[] Data;
		private MDL0.Model Model;

		private int Offset = 0;
		private bool NodeVisible = true;

		public SBC(byte[] Data, MDL0.Model Model)
		{
			this.Data = Data;
			this.Model = Model;
		}

		public void Execute()
		{
			CommandContext c = new CommandContext();
			c.MatrixMode(CommandContext.NDSMatrixMode.Position_Vector);
			while (Offset < Data.Length)
			{
				byte cmd = Data[Offset++];
				switch (cmd & 0xF)
				{
					case 0://NOP
						break;
					case 1://RET
						return;
					case 2://NODE
						{
							byte nodeid = Data[Offset++];
							NodeVisible = Data[Offset++] == 1;
							break;
						}
					case 3://MTX
						if (!NodeVisible) { Offset++; break; }
						c.RestoreMatrix(Data[Offset++]);
						break;
					case 4://MAT
						{
							if (!NodeVisible) { Offset++; break; }
							byte matid = Data[Offset++];
							MDL0.Model.MaterialSet.Material m = Model.materials.materials[matid];
							Gl.glBindTexture(Gl.GL_TEXTURE_2D, matid + 1); //+ texoffset);
							Gl.glMatrixMode(Gl.GL_TEXTURE);
							Gl.glLoadIdentity();
							Gl.glScalef(1f / m.origWidth, 1f / m.origHeight, 1f);
							Gl.glMultMatrixf(m.GetMatrix());

							c.PolygonAttr(m.polyAttr);

							uint diff = 0;
							if ((m.flag & MDL0.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_DIFFUSE) != 0)
								diff = m.diffAmb & 0x7FFF;
							uint amb = 0x5294;
							if ((m.flag & MDL0.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_AMBIENT) != 0)
								amb = (m.diffAmb >> 16) & 0x7FFF;

							uint usevtx = 0;
							if ((m.diffAmb & 0x8000) != 0 && (m.flag & MDL0.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_VTXCOLOR) != 0)
								usevtx = 0x8000;

							c.MaterialColor0((uint)(amb << 16 | usevtx | diff));

							uint spec = 0;
							if ((m.flag & MDL0.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_SPECULAR) != 0)
								spec = m.specEmi & 0x7FFF;
							uint emiss = 0;
							if ((m.flag & MDL0.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_EMISSION) != 0)
								spec = (m.specEmi >> 16) & 0x7FFF;

							c.MaterialColor1((uint)(emiss << 16 | m.specEmi & 0x8000 | spec));

							Gl.glMatrixMode(Gl.GL_MODELVIEW);
							Gl.glDisable(Gl.GL_TEXTURE_GEN_S);
							Gl.glDisable(Gl.GL_TEXTURE_GEN_T);
							break;
						}
					case 5://SHP
						{
							if (!NodeVisible) { Offset++; break; }
							c.RunDL(Model.shapes.shape[Data[Offset++]].DL);
							c.End();//to prevent errors if the display list is not correctly terminated
							break;
						}
					case 6://NODEDESC
						{
							byte nodeid = Data[Offset++];
							byte parentid = Data[Offset++];
							byte segmentscale = Data[Offset++];
							bool thisnodesegmentscale = (segmentscale & 1) == 1;
							bool parentnodesegmentscale = (segmentscale >> 1 & 1) == 1;
							bool MayaScale = thisnodesegmentscale;
							int StackID = ((cmd >> 5 & 0x1) == 1) ? Data[Offset++] : -1;
							int RestID = ((cmd >> 6 & 0x1) == 1) ? Data[Offset++] : -1;
							if (RestID != -1) c.RestoreMatrix((uint)RestID);
							Model.nodes.data[nodeid].ApplyMatrix(c, MayaScale);
							//c.MultMatrix44(new Matrix44(Model.nodes.data[nodeid].GetMatrix(MayaScale, 1)));
							if (StackID != -1) c.StoreMatrix((uint)StackID);
							break;
						}
					case 7://BB
						{
							byte nodeid = Data[Offset++];
							int StackID = ((cmd >> 5 & 0x1) == 1) ? Data[Offset++] : -1;
							int RestID = ((cmd >> 6 & 0x1) == 1) ? Data[Offset++] : -1;
							if (RestID != -1) c.RestoreMatrix((uint)RestID);
							float[] proj = new float[16];
							Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, proj);

							Matrix44 m = c.GetCurPosMtx();
							m[3, 0] = m[3, 1] = m[3, 2] = 0;

							Matrix44 mtx = c.GetCurPosMtx();
							mtx[0, 0] = proj[0];
							mtx[1, 0] = proj[1];
							mtx[2, 0] = proj[2];

							mtx[0, 1] = proj[4];
							mtx[1, 1] = proj[5];
							mtx[2, 1] = proj[6];

							mtx[0, 2] = proj[8];
							mtx[1, 2] = proj[9];
							mtx[2, 2] = proj[10];

							c.LoadMatrix44(mtx * m);
							if (StackID != -1) c.StoreMatrix((uint)StackID);
							break;
						}
					case 8://BBY
						{
							byte nodeid = Data[Offset++];
							int StackID = ((cmd >> 5 & 0x1) == 1) ? Data[Offset++] : -1;
							int RestID = ((cmd >> 6 & 0x1) == 1) ? Data[Offset++] : -1;
							if (RestID != -1) c.RestoreMatrix((uint)RestID);
							float[] proj = new float[16];
							Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, proj);

							/*
							Matrix33 mtx = new Matrix33();
							mtx[0, 0] = proj[0];
							mtx[2, 0] = proj[2];

							mtx[0, 1] = proj[4];
							mtx[1, 1] = proj[5];
							mtx[2, 1] = proj[6];

							mtx[0, 2] = proj[8];
							mtx[2, 2] = proj[10];

							c.MultMatrix33(mtx);//c.LoadMatrix44(mtx);
							 * */

							Matrix44 m = c.GetCurPosMtx();
							m[3, 0] = m[3, 1] = m[3, 2] = 0;

							Matrix44 mtx = c.GetCurPosMtx();
							mtx[0, 0] = proj[0];
							mtx[2, 0] = proj[2];

							mtx[0, 1] = proj[4];
							mtx[1, 1] = proj[5];
							mtx[2, 1] = proj[6];

							mtx[0, 2] = proj[8];
							mtx[2, 2] = proj[10];

							c.LoadMatrix44(mtx * m);
							if (StackID != -1) c.StoreMatrix((uint)StackID);
							break;
						}
					case 9://NODEMIX
						{
							byte stackid = Data[Offset++];
							byte nummatrices = Data[Offset++];
							for (int i = 0; i < nummatrices; i++)
							{
								byte SrcIdx_N = Data[Offset++];
								byte NodeID_N = Data[Offset++];
								float Ratio_N = Data[Offset++] / 256f;
							}
							break;
						}
					case 10://CALLDL
						{
							uint RelAddr = IOUtil.ReadU32LE(Data, Offset);
							Offset += 4;
							uint Size = IOUtil.ReadU32LE(Data, Offset);
							Offset += 4;
							byte[] DL = new byte[Size];
							Array.Copy(Data, Offset - 9 + RelAddr, DL, 0, Size);
							c.RunDL(DL);
							break;
						}
					case 11://POSSCALE
						{
							if (cmd >> 5 == 0)
							{
								c.Scale(new Vector3(Model.info.posScale, Model.info.posScale, Model.info.posScale));
							}
							else
							{
								c.Scale(new Vector3(Model.info.invPosScale, Model.info.invPosScale, Model.info.invPosScale));
							}
							break;
						}
					case 12://ENVMAP
						{
							byte matid = Data[Offset++];
							byte flag = Data[Offset++];
							if (NodeVisible)
							{
								float[] proj = new float[16];
								Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, proj);

								Gl.glMatrixMode(Gl.GL_TEXTURE);
								//Gl.glLoadIdentity();
								MDL0.Model.MaterialSet.Material m = Model.materials.materials[matid];
								Gl.glScalef(m.origWidth * 0.5f, m.origHeight  * - 0.5f, 1);
								Gl.glTranslatef(m.origWidth * 0.5f, m.origHeight * 0.5f, 0);
								//Gl.glTexCoord2f(m.origWidth * 0.5f, m.origHeight * 0.5f);
								//Gl.glScalef(m.origWidth, m.origHeight, 1f);

								if ((m.flag & MDL0.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_EFFECTMTX) != 0)
								{
									Gl.glMultMatrixf(m.effectMtx);
								}

								Matrix44 mvm = new Matrix44(proj);
								mvm[3, 0] = mvm[3, 1] = mvm[3, 2] = 0;

								Matrix44 curmtx = c.GetCurPosMtx();
								curmtx[3, 0] = curmtx[3, 1] = curmtx[3, 2] = 0;

								mvm *= curmtx;

								mvm[12] = 0;
								mvm[13] = 0;
								mvm[14] = 0;

								Gl.glMultMatrixf((float[])mvm);

								Gl.glMatrixMode(Gl.GL_MODELVIEW);

								Gl.glTexGeni(Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP);
								Gl.glTexGeni(Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP);
								Gl.glEnable(Gl.GL_TEXTURE_GEN_S);
								Gl.glEnable(Gl.GL_TEXTURE_GEN_T);
							}
							break;
						}
					case 13://PRJMAP
						{
							byte matid = Data[Offset++];
							byte flag = Data[Offset++];
							break;
						}
				}
			}
		}
	}
}
