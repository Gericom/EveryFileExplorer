using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.Collections;
using NDS.GPU;
using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.G3D
{
	public class MDL0
	{
		public MDL0()
		{
			Signature = "MDL0";
			dict = new Dictionary<MDL0Data>();
		}
		public MDL0(EndianBinaryReader er)
		{
			long basepos = er.BaseStream.Position;
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "MDL0") throw new SignatureNotCorrectException(Signature, "MDL0", er.BaseStream.Position - 4);
			SectionSize = er.ReadUInt32();
			dict = new Dictionary<MDL0Data>(er);
			models = new Model[dict.numEntry];
			long curpos = er.BaseStream.Position;
			for (int i = 0; i < dict.numEntry; i++)
			{
				er.BaseStream.Position = dict[i].Value.Offset + basepos;//er.GetMarker("ModelSet");
				models[i] = new Model(er);
			}
		}
		public void Write(EndianBinaryWriter er)
		{
			long offpos = er.BaseStream.Position;
			//header.Write(er, 0);
			er.Write(Signature, Encoding.ASCII, false);
			er.Write((uint)0);
			dict.Write(er);
			for (int i = 0; i < models.Length; i++)
			{
				dict[i].Value.Offset = (uint)(er.BaseStream.Position - offpos);
				models[i].Write(er);
			}
			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = offpos + 4;
			er.Write((UInt32)(curpos - offpos));
			dict.Write(er);
			er.BaseStream.Position = curpos;
		}
		public String Signature;
		public UInt32 SectionSize;
		public Dictionary<MDL0Data> dict;
		public class MDL0Data : DictionaryData
		{
			public override UInt16 GetDataSize()
			{
				return 4;
			}
			public override void Read(EndianBinaryReader er)
			{
				Offset = er.ReadUInt32();
			}
			public override void Write(EndianBinaryWriter er)
			{
				er.Write(Offset);
			}
			public UInt32 Offset;
		}
		public Model[] models;
		public class Model
		{
			public Model(EndianBinaryReader er)
			{
				long basepos = er.BaseStream.Position;
				size = er.ReadUInt32();
				ofsSbc = er.ReadUInt32();
				ofsMat = er.ReadUInt32();
				ofsShp = er.ReadUInt32();
				ofsEvpMtx = er.ReadUInt32();
				info = new ModelInfo(er);
				nodes = new NodeSet(er);
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = ofsSbc + basepos;
				sbc = er.ReadBytes((int)(ofsMat - ofsSbc));
				er.BaseStream.Position = curpos;
				er.BaseStream.Position = ofsMat + basepos;
				materials = new MaterialSet(er);
				er.BaseStream.Position = ofsShp + basepos;
				shapes = new ShapeSet(er);
				if (ofsEvpMtx != size && ofsEvpMtx != 0)
				{
					er.BaseStream.Position = ofsEvpMtx + basepos;
					evpMatrices = new EvpMatrices(er, nodes.dict.numEntry);
				}
				/*long modelset = er.GetMarker("ModelSet");
				er.ClearMarkers();
				curpos = er.BaseStream.Position;
				er.BaseStream.Position = modelset;
				er.SetMarkerOnCurrentOffset("ModelSet");
				er.BaseStream.Position = curpos;*/
			}
			public Model() { }
			public void Write(EndianBinaryWriter er)
			{
				long offpos = er.BaseStream.Position;
				er.Write((UInt32)0);//size
				er.Write((UInt32)0);//ofsSbc
				er.Write((UInt32)0);//ofsMat
				er.Write((UInt32)0);//ofsShp
				er.Write((UInt32)0);//ofsEvpMtx
				info.Write(er);
				nodes.Write(er);

				//Sbc Offset
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = offpos + 4;
				er.Write((UInt32)(curpos - offpos));
				er.BaseStream.Position = curpos;

				er.Write(sbc, 0, sbc.Length);

				//Material Offset
				curpos = er.BaseStream.Position;
				er.BaseStream.Position = offpos + 8;
				er.Write((UInt32)(curpos - offpos));
				er.BaseStream.Position = curpos;

				materials.Write(er);

				//Shape Offset
				curpos = er.BaseStream.Position;
				er.BaseStream.Position = offpos + 12;
				er.Write((UInt32)(curpos - offpos));
				er.BaseStream.Position = curpos;

				shapes.Write(er);

				if (evpMatrices != null)
				{
					//EvpMtx Offset
					curpos = er.BaseStream.Position;
					er.BaseStream.Position = offpos + 16;
					er.Write((UInt32)(curpos - offpos));
					er.BaseStream.Position = curpos;

					evpMatrices.Write(er);
				}
				else
				{
					curpos = er.BaseStream.Position;
					er.BaseStream.Position = offpos + 16;
					er.Write((UInt32)(curpos - offpos));
					er.BaseStream.Position = curpos;
				}

				//Size
				curpos = er.BaseStream.Position;
				er.BaseStream.Position = offpos;
				er.Write((UInt32)(curpos - offpos));
				er.BaseStream.Position = curpos;
			}
			public UInt32 size;
			public UInt32 ofsSbc;
			public UInt32 ofsMat;
			public UInt32 ofsShp;
			public UInt32 ofsEvpMtx;
			public ModelInfo info;
			public class ModelInfo
			{
				public ModelInfo(EndianBinaryReader er)
				{
					sbcType = er.ReadByte();
					scalingRule = er.ReadByte();
					texMtxMode = er.ReadByte();
					numNode = er.ReadByte();
					numMat = er.ReadByte();
					numShp = er.ReadByte();
					firstUnusedMtxStackID = er.ReadByte();
					er.ReadByte();//PADDING(1 byte);
					posScale = er.ReadFx32();
					invPosScale = er.ReadFx32();
					numVertex = er.ReadUInt16();
					numPolygon = er.ReadUInt16();
					numTriangle = er.ReadUInt16();
					numQuad = er.ReadUInt16();
					boxX = er.ReadFx16();
					boxY = er.ReadFx16();
					boxZ = er.ReadFx16();
					boxW = er.ReadFx16();
					boxH = er.ReadFx16();
					boxD = er.ReadFx16();
					boxPosScale = er.ReadFx32();
					boxInvPosScale = er.ReadFx32();
				}
				public ModelInfo() { }
				public void Write(EndianBinaryWriter er)
				{
					er.Write(sbcType);
					er.Write(scalingRule);
					er.Write(texMtxMode);
					er.Write(numNode);
					er.Write(numMat);
					er.Write(numShp);
					er.Write(firstUnusedMtxStackID);
					er.Write((byte)0);//PADDING(1 byte);
					er.Write((UInt32)(posScale * 4096f));
					er.Write((UInt32)(invPosScale * 4096f));
					er.Write(numVertex);
					er.Write(numPolygon);
					er.Write(numTriangle);
					er.Write(numQuad);
					er.Write((UInt16)(boxX * 4096f));
					er.Write((UInt16)(boxY * 4096f));
					er.Write((UInt16)(boxZ * 4096f));
					er.Write((UInt16)(boxW * 4096f));
					er.Write((UInt16)(boxH * 4096f));
					er.Write((UInt16)(boxD * 4096f));
					er.Write((UInt32)(boxPosScale * 4096f));
					er.Write((UInt32)(boxInvPosScale * 4096f));
				}
				public byte sbcType;
				public byte scalingRule;
				public byte texMtxMode;
				public byte numNode;
				public byte numMat;
				public byte numShp;
				public byte firstUnusedMtxStackID;

				public Single posScale;
				public Single invPosScale;
				public UInt16 numVertex;
				public UInt16 numPolygon;
				public UInt16 numTriangle;
				public UInt16 numQuad;
				public Single boxX, boxY, boxZ;
				public Single boxW, boxH, boxD;
				public Single boxPosScale;
				public Single boxInvPosScale;
			}
			public NodeSet nodes;
			public class NodeSet
			{
				public NodeSet(EndianBinaryReader er)
				{
					//er.SetMarkerOnCurrentOffset("NodeInfo");
					long basepos = er.BaseStream.Position;
					dict = new Dictionary<NodeSetData>(er);
					data = new NodeData[dict.numEntry];
					long curpos = er.BaseStream.Position;
					for (int i = 0; i < dict.numEntry; i++)
					{
						er.BaseStream.Position = /*er.GetMarker("NodeInfo")*/basepos + dict[i].Value.Offset;
						data[i] = new NodeData(er);
					}
					er.BaseStream.Position = curpos;
				}
				public void Write(EndianBinaryWriter er)
				{
					long offpos = er.BaseStream.Position;
					dict.Write(er);
					for (int i = 0; i < data.Length; i++)
					{
						dict[i].Value.Offset = (UInt32)(er.BaseStream.Position - offpos);
						data[i].Write(er);
					}
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = offpos;
					dict.Write(er);
					er.BaseStream.Position = curpos;
				}
				public NodeSet() { }
				public Dictionary<NodeSetData> dict;
				public class NodeSetData : DictionaryData
				{
					public override UInt16 GetDataSize()
					{
						return 4;
					}
					public override void Read(EndianBinaryReader er)
					{
						Offset = er.ReadUInt32();
					}
					public override void Write(EndianBinaryWriter er)
					{
						er.Write(Offset);
					}
					public UInt32 Offset;
				}
				public NodeData[] data;
				public class NodeData
				{
					public const UInt16 NNS_G3D_SRTFLAG_TRANS_ZERO = 0x0001;
					public const UInt16 NNS_G3D_SRTFLAG_ROT_ZERO = 0x0002;
					public const UInt16 NNS_G3D_SRTFLAG_SCALE_ONE = 0x0004;
					public const UInt16 NNS_G3D_SRTFLAG_PIVOT_EXIST = 0x0008;

					public NodeData(EndianBinaryReader er)
					{
						flag = er.ReadUInt16();
						_00 = er.ReadInt16();
						if ((flag & NNS_G3D_SRTFLAG_TRANS_ZERO) == 0)
						{
							Tx = er.ReadFx32();
							Ty = er.ReadFx32();
							Tz = er.ReadFx32();
						}
						if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0 &&
				(flag & NNS_G3D_SRTFLAG_PIVOT_EXIST) == 0)
						{
							_01 = er.ReadFx16();
							_02 = er.ReadFx16();
							_10 = er.ReadFx16();
							_11 = er.ReadFx16();
							_12 = er.ReadFx16();
							_20 = er.ReadFx16();
							_21 = er.ReadFx16();
							_22 = er.ReadFx16();
						}

						if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0 &&
				(flag & NNS_G3D_SRTFLAG_PIVOT_EXIST) != 0)
						{
							A = er.ReadFx16();
							B = er.ReadFx16();
						}

						if ((flag & NNS_G3D_SRTFLAG_SCALE_ONE) == 0)
						{
							Sx = er.ReadFx32();
							Sy = er.ReadFx32();
							Sz = er.ReadFx32();
							InvSx = er.ReadFx32();
							InvSy = er.ReadFx32();
							InvSz = er.ReadFx32();
						}
					}
					public NodeData() { }
					public void Write(EndianBinaryWriter er)
					{
						er.Write(flag);
						er.Write(_00);

						if ((flag & NNS_G3D_SRTFLAG_TRANS_ZERO) == 0)
						{
							er.Write((UInt32)(Tx * 4096f));
							er.Write((UInt32)(Ty * 4096f));
							er.Write((UInt32)(Tz * 4096f));
						}
						if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0 &&
				(flag & NNS_G3D_SRTFLAG_PIVOT_EXIST) == 0)
						{
							er.Write((UInt16)(_01 * 4096f));
							er.Write((UInt16)(_02 * 4096f));
							er.Write((UInt16)(_10 * 4096f));
							er.Write((UInt16)(_11 * 4096f));
							er.Write((UInt16)(_12 * 4096f));
							er.Write((UInt16)(_20 * 4096f));
							er.Write((UInt16)(_21 * 4096f));
							er.Write((UInt16)(_22 * 4096f));
						}

						if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0 &&
				(flag & NNS_G3D_SRTFLAG_PIVOT_EXIST) != 0)
						{
							er.Write((UInt16)(A * 4096f));
							er.Write((UInt16)(B * 4096f));
						}

						if ((flag & NNS_G3D_SRTFLAG_SCALE_ONE) == 0)
						{
							er.Write((UInt32)(Sx * 4096f));
							er.Write((UInt32)(Sy * 4096f));
							er.Write((UInt32)(Sz * 4096f));
							er.Write((UInt32)(InvSx * 4096f));
							er.Write((UInt32)(InvSy * 4096f));
							er.Write((UInt32)(InvSz * 4096f));
						}
					}
					public UInt16 flag;
					public Int16 _00;

					public Single Tx, Ty, Tz;

					public Single _01, _02;
					public Single _10, _11, _12;
					public Single _20, _21, _22;

					public Single A, B;

					public Single Sx, Sy, Sz;
					public Single InvSx, InvSy, InvSz;

					public void ApplyMatrix(CommandContext c, bool MayaScale)
					{
						/*bool trFlag = (flag & NNS_G3D_SRTFLAG_TRANS_ZERO) == 0;

						if (MayaScale && (flag & NNS_G3D_SRTFLAG_SCALE_ONE) == 0)
						{
							if (trFlag)
							{
								c.Translate(new Vector3(Tx, Ty, Tz));
								trFlag = false;
							}
							c.Scale(new Vector3(InvSx, InvSy, InvSz));
						}

						if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0)
						{
							float[] r = loadIdentity();
							if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0 && (flag & NNS_G3D_SRTFLAG_PIVOT_EXIST) == 0)
							{
								r[0] = _00 / 4096f;
								r[1] = _01;
								r[2] = _02;
								r[4] = _10;
								r[5] = _11;
								r[6] = _12;
								r[8] = _20;
								r[9] = _21;
								r[10] = _22;
							}
							else if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0 && (flag & NNS_G3D_SRTFLAG_PIVOT_EXIST) != 0)
							{
								r = multMatrix(r, (float[])Util.G3DPivotToMatrix(new float[] { A, B }, (int)(flag >> 4) & 0x0f, (int)(flag >> 8) & 0x0f));
							}

							if (trFlag)
							{
								r[12] = Tx;
								r[13] = Ty;
								r[14] = Tz;
							}
							c.MultMatrix44(new Matrix44(r));
						}
						else
						{
							if (trFlag)
							{
								c.Translate(new Vector3(Tx, Ty, Tz));
							}
						}

						if ((flag & NNS_G3D_SRTFLAG_SCALE_ONE) == 0)
						{
							c.Scale(new Vector3(Sx, Sy, Sz));
						}*/

						if ((flag & NNS_G3D_SRTFLAG_TRANS_ZERO) == 0) c.Translate(new Vector3(Tx, Ty, Tz));
						if (MayaScale && (flag & NNS_G3D_SRTFLAG_SCALE_ONE) == 0) c.Scale(new Vector3(InvSx, InvSy, InvSz));
						float[] r = loadIdentity();
						if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0 &&
				(flag & NNS_G3D_SRTFLAG_PIVOT_EXIST) == 0)
						{
							r[0] = _00 / 4096f;
							r[1] = _01;
							r[2] = _02;
							r[4] = _10;
							r[5] = _11;
							r[6] = _12;
							r[8] = _20;
							r[9] = _21;
							r[10] = _22;
						}
						else if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0 &&
				(flag & NNS_G3D_SRTFLAG_PIVOT_EXIST) != 0)
						{
							r = multMatrix(r, (float[])Util.G3DPivotToMatrix(new float[] { A, B }, (int)(flag >> 4) & 0x0f, (int)(flag >> 8) & 0x0f));
						}
						c.MultMatrix44(new Matrix44(r));
						if ((flag & NNS_G3D_SRTFLAG_SCALE_ONE) == 0) c.Scale(new Vector3(Sx, Sy, Sz));
					}

					public float[] GetMatrix(bool MayaScale, int PosScale)
					{
						float[] s = loadIdentity();
						float[] Is = loadIdentity();
						float[] r = loadIdentity();
						float[] t = loadIdentity();
						if ((flag & NNS_G3D_SRTFLAG_TRANS_ZERO) == 0)
						{
							t = translate(t, Tx / PosScale, Ty / PosScale, Tz / PosScale);
						}
						if (MayaScale)
						{
							if ((flag & NNS_G3D_SRTFLAG_SCALE_ONE) == 0)
							{
								Is = scale(Is, InvSx / PosScale, InvSy / PosScale, InvSz / PosScale);
							}
						}
						if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0 &&
				(flag & NNS_G3D_SRTFLAG_PIVOT_EXIST) == 0)
						{
							r[0] = _00 / 4096f;
							r[1] = _01;
							r[2] = _02;
							r[4] = _10;
							r[5] = _11;
							r[6] = _12;
							r[8] = _20;
							r[9] = _21;
							r[10] = _22;
						}
						else if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0 &&
				(flag & NNS_G3D_SRTFLAG_PIVOT_EXIST) != 0)
						{
							r = multMatrix(r, (float[])Util.G3DPivotToMatrix(new float[] { A, B }, (int)(flag >> 4) & 0x0f, (int)(flag >> 8) & 0x0f));
						}
						if ((flag & NNS_G3D_SRTFLAG_SCALE_ONE) == 0)
						{
							s = scale(s, Sx / PosScale, Sy / PosScale, Sz / PosScale);
						}
						float[] f = (float[])Matrix44.Identity;
						f = multMatrix(f, t);
						if (MayaScale)
						{
							f = multMatrix(f, Is);
						}
						f = multMatrix(f, r);
						f = multMatrix(f, s);
						return f;
					}
					public float[] GetRotation()
					{
						float[] r = loadIdentity();
						if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0 &&
				(flag & NNS_G3D_SRTFLAG_PIVOT_EXIST) == 0)
						{
							r[0] = _00 / 4096f;
							r[1] = _01;
							r[2] = _02;
							r[4] = _10;
							r[5] = _11;
							r[6] = _12;
							r[8] = _20;
							r[9] = _21;
							r[10] = _22;
						}
						else if ((flag & NNS_G3D_SRTFLAG_ROT_ZERO) == 0 &&
				(flag & NNS_G3D_SRTFLAG_PIVOT_EXIST) != 0)
						{
							r = multMatrix(r, (float[])Util.G3DPivotToMatrix(new float[] { A, B }, (int)(flag >> 4) & 0x0f, (int)(flag >> 8) & 0x0f));
						}
						return r;
					}
					private float[] translate(float[] a, float x, float y, float z)
					{
						float[] b = loadIdentity();
						b[12] = x;
						b[13] = y;
						b[14] = z;
						return multMatrix(a, b);
					}
					private float[] loadIdentity()
					{
						float[] a = new float[16];
						a[0] = 1.0F;
						a[5] = 1.0F;
						a[10] = 1.0F;
						a[15] = 1.0F;
						return a;
					}
					private static float[] multMatrix(float[] a, float[] b)
					{
						float[] c = new float[16];
						for (int i = 0; i < 4; i++)
						{
							for (int j = 0; j < 4; j++)
							{
								c[(i << 2) + j] = 0.0F;
								for (int k = 0; k < 4; k++)
									c[(i << 2) + j] += a[(k << 2) + j] * b[(i << 2) + k];

							}

						}

						return c;
					}
					private float[] scale(float[] a, float x, float y, float z)
					{
						float[] b = loadIdentity();
						b[0] = x;
						b[5] = y;
						b[10] = z;
						return multMatrix(a, b);
					}


				}
			}
			public byte[] sbc;
			public MaterialSet materials;
			public class MaterialSet
			{
				public MaterialSet(EndianBinaryReader er)
				{
					//er.SetMarkerOnCurrentOffset("MaterialSet");
					long basepos = er.BaseStream.Position;
					ofsDictTexToMatList = er.ReadUInt16();
					ofsDictPlttToMatList = er.ReadUInt16();
					dict = new Dictionary<MaterialSetData>(er);

					long curpos = er.BaseStream.Position;
					materials = new Material[dict.numEntry];
					for (int i = 0; i < dict.numEntry; i++)
					{
						er.BaseStream.Position = dict[i].Value.Offset + basepos;//er.GetMarker("MaterialSet");
						materials[i] = new Material(er);
					}
					er.BaseStream.Position = curpos;
					dictTexToMatList = new Dictionary<TexToMatData>(er);
					for (int i = 0; i < dictTexToMatList.numEntry; i++)
					{
						curpos = er.BaseStream.Position;
						er.BaseStream.Position = dictTexToMatList[i].Value.Offset + basepos;
						dictTexToMatList[i].Value.Materials = er.ReadBytes((int)dictTexToMatList[i].Value.NrMat);
						er.BaseStream.Position = curpos;
					}

					dictPlttToMatList = new Dictionary<PlttToMatData>(er);
					for (int i = 0; i < dictPlttToMatList.numEntry; i++)
					{
						curpos = er.BaseStream.Position;
						er.BaseStream.Position = dictPlttToMatList[i].Value.Offset + basepos;
						dictPlttToMatList[i].Value.Materials = er.ReadBytes((int)dictPlttToMatList[i].Value.NrMat);
						er.BaseStream.Position = curpos;
					}

					while ((er.BaseStream.Position % 4) != 0) er.ReadByte();
					//er.ReadBytes(4);//PADDING(4 bytes alignment);
				}
				public MaterialSet() { }
				public void Write(EndianBinaryWriter er)
				{
					long offpos = er.BaseStream.Position;
					er.Write((UInt16)0);//ofsDictTexToMatList
					er.Write((UInt16)0);//ofsDictPlttToMatList
					dict.Write(er);

					//ofsDictTexToMatList
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = offpos;
					er.Write((UInt16)(curpos - offpos));
					er.BaseStream.Position = curpos;

					dictTexToMatList.Write(er);

					//ofsDictPlttToMatList
					curpos = er.BaseStream.Position;
					er.BaseStream.Position = offpos + 2;
					er.Write((UInt16)(curpos - offpos));
					er.BaseStream.Position = curpos;

					dictPlttToMatList.Write(er);

					for (int i = 0; i < dictTexToMatList.numEntry; i++)
					{
						dictTexToMatList[i].Value.Offset = (UInt16)(er.BaseStream.Position - offpos);
						foreach (int m in dictTexToMatList[i].Value.Materials) er.Write((byte)m);
					}

					for (int i = 0; i < dictPlttToMatList.numEntry; i++)
					{
						dictPlttToMatList[i].Value.Offset = (UInt16)(er.BaseStream.Position - offpos);
						foreach (int m in dictPlttToMatList[i].Value.Materials) er.Write((byte)m);
					}
					while ((er.BaseStream.Position % 4) != 0) er.Write((byte)0);
					//er.Write((UInt32)0);//PADDING(4 bytes alignment);

					for (int i = 0; i < materials.Length; i++)
					{
						dict[i].Value.Offset = (UInt32)(er.BaseStream.Position - offpos);
						materials[i].Write(er);
					}

					curpos = er.BaseStream.Position;
					er.BaseStream.Position = offpos + 4;
					dict.Write(er);
					dictTexToMatList.Write(er);
					dictPlttToMatList.Write(er);
					er.BaseStream.Position = curpos;
				}
				public UInt16 ofsDictTexToMatList;
				public UInt16 ofsDictPlttToMatList;
				public Dictionary<MaterialSetData> dict;
				public class MaterialSetData : DictionaryData
				{
					public override UInt16 GetDataSize()
					{
						return 4;
					}
					public override void Read(EndianBinaryReader er)
					{
						Offset = er.ReadUInt32();
					}
					public override void Write(EndianBinaryWriter er)
					{
						er.Write(Offset);
					}
					public UInt32 Offset;
				}
				public Dictionary<TexToMatData> dictTexToMatList;
				public class TexToMatData : DictionaryData
				{
					public override UInt16 GetDataSize()
					{
						return 4;
					}
					public override void Read(EndianBinaryReader er)
					{
						Flag = er.ReadUInt32();
						Offset = (ushort)(Flag & 0xFFFF);
						NrMat = (byte)(Flag >> 16 & 0x7F);
						Bound = (byte)(Flag >> 24 & 0xFF);

						/*Materials = new int[NrMat];
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Offset + er.GetMarker("MaterialSet");
						for (int i = 0; i < NrMat; i++)
						{
							Materials[i] = er.ReadByte();
						}
						er.BaseStream.Position = curpos;*/
					}
					public override void Write(EndianBinaryWriter er)
					{
						Flag = (UInt32)(((Bound & 0xFF) << 24) | ((NrMat & 0xFF) << 16) | ((Offset & 0xFFFF) << 0));
						er.Write(Flag);
					}
					public UInt32 Flag;
					public UInt16 Offset;
					public byte NrMat;
					public byte Bound;

					public /*int*/byte[] Materials;
				}
				public Dictionary<PlttToMatData> dictPlttToMatList;
				public class PlttToMatData : DictionaryData
				{
					public override UInt16 GetDataSize()
					{
						return 4;
					}
					public override void Read(EndianBinaryReader er)
					{
						Flag = er.ReadUInt32();
						Offset = (ushort)(Flag & 0xFFFF);
						NrMat = (byte)(Flag >> 16 & 0x7F);
						Bound = (byte)(Flag >> 24 & 0xFF);

						//Materials = new byte[NrMat];
						/*long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Offset + er.GetMarker("MaterialSet");
						for (int i = 0; i < NrMat; i++)
						{
							Materials[i] = er.ReadByte();
						}
						er.BaseStream.Position = curpos;*/
					}
					public override void Write(EndianBinaryWriter er)
					{
						Flag = (UInt32)(((Bound & 0xFF) << 24) | ((NrMat & 0xFF) << 16) | ((Offset & 0xFFFF) << 0));
						er.Write(Flag);
					}
					public UInt32 Flag;
					public UInt16 Offset;
					public byte NrMat;
					public byte Bound;

					public /*int*/byte[] Materials;
				}
				public Material[] materials;
				public class Material
				{
					[Flags]
					public enum NNS_G3D_MATFLAG : ushort
					{
						NNS_G3D_MATFLAG_TEXMTX_USE = 0x0001,
						NNS_G3D_MATFLAG_TEXMTX_SCALEONE = 0x0002,
						NNS_G3D_MATFLAG_TEXMTX_ROTZERO = 0x0004,
						NNS_G3D_MATFLAG_TEXMTX_TRANSZERO = 0x0008,
						NNS_G3D_MATFLAG_ORIGWH_SAME = 0x0010,
						NNS_G3D_MATFLAG_WIREFRAME = 0x0020,
						NNS_G3D_MATFLAG_DIFFUSE = 0x0040,
						NNS_G3D_MATFLAG_AMBIENT = 0x0080,
						NNS_G3D_MATFLAG_VTXCOLOR = 0x0100,
						NNS_G3D_MATFLAG_SPECULAR = 0x0200,
						NNS_G3D_MATFLAG_EMISSION = 0x0400,
						NNS_G3D_MATFLAG_SHININESS = 0x0800,
						NNS_G3D_MATFLAG_TEXPLTTBASE = 0x1000,
						NNS_G3D_MATFLAG_EFFECTMTX = 0x2000,
					};
					public Material(EndianBinaryReader er)
					{
						itemTag = er.ReadUInt16();
						size = er.ReadUInt16();
						diffAmb = er.ReadUInt32();
						specEmi = er.ReadUInt32();
						polyAttr = er.ReadUInt32();
						polyAttrMask = er.ReadUInt32();
						texImageParam = er.ReadUInt32();
						texImageParamMask = er.ReadUInt32();
						texPlttBase = er.ReadUInt16();
						flag = (NNS_G3D_MATFLAG)er.ReadUInt16();
						origWidth = er.ReadUInt16();
						origHeight = er.ReadUInt16();
						magW = er.ReadFx32();
						magH = er.ReadFx32();
						if ((flag & NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_TEXMTX_SCALEONE) == 0)
						{
							scaleS = er.ReadFx32();
							scaleT = er.ReadFx32();
						}

						if ((flag & NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_TEXMTX_ROTZERO) == 0)
						{
							rotSin = er.ReadFx16();
							rotCos = er.ReadFx16();
						}

						if ((flag & NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_TEXMTX_TRANSZERO) == 0)
						{
							transS = er.ReadFx32();
							transT = er.ReadFx32();
						}

						if ((flag & NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_EFFECTMTX) == NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_EFFECTMTX)
						{
							effectMtx = er.ReadFx32s(16);
						}
					}
					public Material() { }
					public void Write(EndianBinaryWriter er)
					{
						long offsetpos = er.BaseStream.Position;
						er.Write(itemTag);
						er.Write((UInt16)0);
						er.Write(diffAmb);
						er.Write(specEmi);
						er.Write(polyAttr);
						er.Write(polyAttrMask);
						er.Write(texImageParam);
						er.Write(texImageParamMask);
						er.Write(texPlttBase);
						er.Write((ushort)flag);
						er.Write(origWidth);
						er.Write(origHeight);
						er.Write((UInt32)(magW * 4096f));
						er.Write((UInt32)(magH * 4096f));

						if ((flag & NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_TEXMTX_SCALEONE) == 0)
						{
							er.Write((UInt32)(scaleS * 4096f));
							er.Write((UInt32)(scaleT * 4096f));
						}

						if ((flag & NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_TEXMTX_ROTZERO) == 0)
						{
							er.Write((UInt16)(rotSin * 4096f));
							er.Write((UInt16)(rotCos * 4096f));
						}

						if ((flag & NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_TEXMTX_TRANSZERO) == 0)
						{
							er.Write((UInt32)(transS * 4096f));
							er.Write((UInt32)(transT * 4096f));
						}

						if ((flag & NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_EFFECTMTX) != 0)
						{
							foreach (float f in effectMtx)
							{
								er.Write((UInt32)(f * 4096f));
							}
						}
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = offsetpos + 2;
						er.Write((UInt16)(curpos - offsetpos));
						er.BaseStream.Position = curpos;
					}
					public UInt16 itemTag;
					public UInt16 size;
					public UInt32 diffAmb, specEmi;
					public UInt32 polyAttr, polyAttrMask;
					public UInt32 texImageParam, texImageParamMask;
					public UInt16 texPlttBase;
					public NNS_G3D_MATFLAG flag;
					public UInt16 origWidth, origHeight;
					public Single magW, magH;

					public Single scaleS, scaleT;

					public Single rotSin, rotCos;

					public Single transS, transT;

					public Single[] effectMtx;

					public Textures.ImageFormat Fmt;

					public float[] GetMatrix()
					{
						/*if ((flag & NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_EFFECTMTX) != 0)
						{
							//MTX44 S_ = new MTX44();
							//S_.Scale(1f / (float)origWidth, 1f / (float)origHeight, 1f);
							//return ((MTX44)effectMtx).MultMatrix(S_);
							return effectMtx;
						}*/
						Matrix44 m = new Matrix44();//zero!
						bool Scale = (flag & NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_TEXMTX_SCALEONE) == 0;
						bool Rotation = (flag & NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_TEXMTX_ROTZERO) == 0;
						bool Translation = (flag & NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_TEXMTX_TRANSZERO) == 0;
						/*if (!Scale && !Rotation && !Translation)
						{
							m[0, 0] = 1;
							m[1, 0] = 0;
							m[0, 1] = 0;
							m[1, 1] = 1;
							m[0, 3] = 0;
							m[1, 3] = 0;
						}
						else if (!Scale && !Rotation && Translation)
						{
							m[0, 0] = 1;
							m[1, 1] = 1;

							m[1, 0] = 0;

							m[0, 3] = -(transS * origWidth) * 16f;
							m[1, 3] = (transT * origHeight) * 16f;

							m[0, 1] = 0;
						}
						else if (!Scale && Rotation && !Translation)
						{
							Single tmpW, tmpH;

							tmpW = (Int32)origWidth * 4096f;
							tmpH = (Int32)origHeight * 4096f;

							Single Div = tmpH / tmpW;

							m[0, 0] = rotCos;
							m[1, 1] = rotCos;

							m[1, 0] = -rotSin * Div / 4096f;

							Div = tmpW / tmpH;

							m[0, 3] = ((-rotSin - rotCos + 1) * origWidth * 8);
							m[1, 3] = ((rotSin - rotCos + 1) * origHeight * 8);

							m[0, 1] = rotSin * Div / 4096f;
						}
						else if (!Scale && Rotation && Translation)
						{
							Single tmpW, tmpH;

							tmpW = (Int32)origWidth * 4096f;
							tmpH = (Int32)origHeight * 4096f;

							Single Div = tmpH / tmpW;

							m[0, 0] = rotCos;
							m[1, 1] = rotCos;

							m[1, 0] = -rotSin * Div / 4096f;

							Div = tmpW / tmpH;

							m[0, 3] = ((-rotSin - rotCos + 1) * origWidth * 8) -
									 (transS * origWidth * 16);
							m[1, 3] = ((rotSin - rotCos + 1) * origHeight * 8) +
									 (transT * origHeight * 16);

							m[0, 1] = rotSin * Div / 4096f;

						}
						else if (Scale && !Rotation && !Translation)
						{
							m[0, 0] = scaleS;
							m[1, 1] = scaleT;

							m[1, 0] = 0;

							m[0, 3] = 0;
							m[1, 3] = ((-2 * scaleT + 1 * 2) * origHeight * 8);

							m[0, 1] = 0;
						}
						else if (Scale && !Rotation && Translation)
						{
							m[0, 0] = scaleS;
							m[1, 1] = scaleT;

							m[1, 0] = 0;

							m[0, 3] = -(Single)((Double)scaleS * transS / 255f) * origWidth;
							m[1, 3] = ((-scaleT - scaleT + 1 * 2) * origHeight * 8) +
									 (Single)((Double)scaleT * transT / 255f) * origHeight;

							m[0, 1] = 0;

						}
						else if (Scale && Rotation && !Translation)
						{
							Single ss_sin, ss_cos;
							Single st_sin, st_cos;
							Single tmpW, tmpH;

							tmpW = (Int32)origWidth * 4096f;
							tmpH = (Int32)origHeight * 4096f;

							Single Div = tmpH / tmpW;

							ss_sin = (Single)((Double)scaleS * rotSin / 4096f);
							ss_cos = (Single)((Double)scaleS * rotCos / 4096f);
							st_sin = (Single)((Double)scaleT * rotSin / 4096f);
							st_cos = (Single)((Double)scaleT * rotCos / 4096f);

							m[0, 0] = ss_cos;
							m[1, 1] = st_cos;

							m[0, 1] = -st_sin * Div / 4096f;

							Div = tmpW / tmpH;

							m[0, 3] = ((-ss_sin - ss_cos + scaleS) * origWidth * 8);
							m[1, 3] = ((st_sin - st_cos - scaleT + 1 * 2) * origHeight * 8);

							m[0, 1] = ss_sin * Div / 4096f;

						}
						else if (Scale && Rotation && Translation)
						{
							Single ss_sin, ss_cos;
							Single st_sin, st_cos;
							Single tmpW, tmpH;

							tmpW = (Int32)origWidth * 4096f;
							tmpH = (Int32)origHeight * 4096f;

							Single Div = tmpH / tmpW;

							ss_sin = (Single)((Double)scaleS * rotSin / 4096f);
							ss_cos = (Single)((Double)scaleS * rotCos / 4096f);
							st_sin = (Single)((Double)scaleT * rotSin / 4096f);
							st_cos = (Single)((Double)scaleT * rotCos / 4096f);

							m[0, 0] = ss_cos;
							m[1, 1] = st_cos;

							m[1, 0] = -st_sin * Div / 4096f;

							Div = tmpW / tmpH;

							m[0, 3] = ((-ss_sin - ss_cos + scaleS) * origWidth * 8) -
									 (Single)((Double)scaleS * transS / 255f) * origWidth;
							m[1, 3] = ((st_sin - st_cos - scaleT + 1 * 2) * origHeight * 8) +
									 (Single)((Double)scaleT * transT / 255f) * origHeight;

							m[0, 1] = ss_sin * Div / 4096f;

						}*/

						//Single Ss, St;
						//Single Ts, Tt;
						//Single Sin, Cos;
						/*MTX44 S = new MTX44();
						MTX44 R = new MTX44();
						MTX44 T = new MTX44();
						if ((flag & NNS_G3D_MATFLAG_TEXMTX_SCALEONE) == 0)
						{
							//Ss = scaleS / (float)origWidth;
							//St = scaleT / (float)origHeight;
							S.Scale(scaleS / (float)origWidth, scaleT / (float)origHeight, 1f);
						}
						else
						{
							//Ss = 1 / (float)origWidth;// / (float)origWidth;
							//St = 1 / (float)origHeight;// / (float)origHeight;
							S.Scale(1f / (float)origWidth, 1f / (float)origHeight, 1f);
						}

						if ((flag & NNS_G3D_MATFLAG_TEXMTX_ROTZERO) == 0)
						{
							//Sin = rotSin;
							//Cos = rotCos;
							R[0, 0] = rotCos;
							R[1, 0] = -rotSin;
							R[0, 1] = rotSin;
							R[1, 1] = -rotCos;
						}
						else
						{
							//Sin = 0;
							//Cos = 1;
							//R = Matrix4.Identity;
						}

						if ((flag & NNS_G3D_MATFLAG_TEXMTX_TRANSZERO) == 0)
						{
							//Ts = transS;
							//Tt = transT;
							T.translate(transS, transT, 0);
						}
						else
						{
							//Ts = 0;
							//Tt = 0;
						}
						MTX44 m = new MTX44();

						/*Single ss_sin, ss_cos;
						Single st_sin, st_cos;
						Single tmpW, tmpH;

						tmpW = (Single)origWidth * 4096f;
						tmpH = (Single)origHeight * 4096f;

						Single div = tmpH / tmpW;

						ss_sin = (Single)((Double)Ss * Sin / 4096f);
						ss_cos = (Single)((Double)Ss * Cos / 4096f);
						st_sin = (Single)((Double)St * Sin / 4096f);
						st_cos = (Single)((Double)St * Cos / 4096f);

						m[0, 0] = ss_cos;// / (float)origWidth;
						m[1, 1] = st_cos;// / (float)origHeight;
						m[1, 0] = -st_sin * div / 4096f;
						div = tmpW / tmpH;

						m[0,3] = ((-ss_sin - ss_cos + Ss) * origWidth * 8) -
								 (Int32)((Int64)Ss * Ts / 255f) * origWidth;
						//m[0, 3] /= 4096f;
						m[1,3] = ((st_sin - st_cos - St + 1 * 2) * origHeight * 8) +
								 (Int32)((Int64)St * Tt / 255f) * origHeight;
						//m[1, 3] /= 4096f;
						m[0,1] = ss_sin * div / 4096f;
						Single ss_sin, ss_cos;
						Single st_sin, st_cos;
						Single tmpW, tmpH;

						int FX32_SHIFT = 12;

						tmpW = (Int32)origWidth * 4096f;
						tmpH = (Int32)origHeight * 4096f;

						Single Div = tmpH / tmpW;
						//FX_DivAsync(tmpH, tmpW);

						ss_sin = (Single)((Double)Ss * Sin / 4096f);
						ss_cos = (Single)((Double)Ss * Cos / 4096f);
						st_sin = (Single)((Double)St * Sin / 4096f);
						st_cos = (Single)((Double)St * Cos / 4096f);

						m[0, 0] = ss_cos;
						m[1, 1] = st_cos;

						m[1, 0] = -st_sin * Div / 4096f;
						Div = tmpW / tmpH;
						//FX_DivAsync(tmpW, tmpH);

						m[0,3] = ((-ss_sin - ss_cos + Ss) * origWidth * 8) -
								 (Single)((Double)Ss * Ts / 255f) * origWidth;
						m[0, 3] /= 4096f;
						m[1,3] = ((st_sin - st_cos - St + 1 * 2) * origHeight * 8) +
								 (Single)((Double)St * Tt / 255f) * origHeight;
						m[1, 3] /= 4096f;

						m[0, 1] = ss_sin * Div / 4096f;/

						m = m.MultMatrix(T);
						m = m.MultMatrix(R);
						m = m.MultMatrix(S);*/
						//m[2, 2] = 1;
						//m[3, 3] = 1;
						//	m.Scale(1f / origWidth, 1f / origHeight, 1);
						//m[0, 0] /= origWidth;
						//m[1, 1] /= origWidth;
						//m[0, 3] /= origWidth;
						//m[1, 3] /= origHeight;
						if (Translation)
						{
							m[3, 0] = transS;
							m[3, 1] = transT;
						}
						float Sx = (Scale ? scaleS : 1);
						float Sy = (Scale ? scaleT : 1);
						if (Rotation)
						{
							m[0, 0] = rotCos * Sx;
							m[0, 1] = -rotSin;
							m[1, 0] = rotSin;
							m[1, 1] = -rotCos * Sy;
						}
						else
						{
							m[0, 0] = Sx;
							m[1, 1] = Sy;
						}
						m[2, 2] = 1;
						m[3, 3] = 1;
						return (float[])m;
					}

				}
			}
			public ShapeSet shapes;
			public class ShapeSet
			{
				public ShapeSet(EndianBinaryReader er)
				{
					long basepos = er.BaseStream.Position;
					dict = new Dictionary<ShapeSetData>(er);
					shape = new Shape[dict.numEntry];
					long curpos = er.BaseStream.Position;
					for (int i = 0; i < dict.numEntry; i++)
					{
						er.BaseStream.Position = dict[i].Value.Offset + basepos;
						shape[i] = new Shape(er);
					}
					er.BaseStream.Position = curpos;
				}
				public ShapeSet() { }
				public void Write(EndianBinaryWriter er)
				{
					long offpos = er.BaseStream.Position;
					dict.Write(er);
					for (int i = 0; i < shape.Length; i++)
					{
						dict[i].Value.Offset = (UInt32)(er.BaseStream.Position - offpos);
						shape[i].Write(er);
					}
					for (int i = 0; i < shape.Length; i++)
					{
						shape[i].ofsDL = (UInt32)(er.BaseStream.Position - offpos - dict[i].Value.Offset);
						er.Write(shape[i].DL, 0, shape[i].DL.Length);
					}
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = offpos;
					dict.Write(er);
					for (int i = 0; i < shape.Length; i++)
					{
						shape[i].Write(er);
					}
					er.BaseStream.Position = curpos;
				}
				public Dictionary<ShapeSetData> dict;
				public class ShapeSetData : DictionaryData
				{
					public override UInt16 GetDataSize()
					{
						return 4;
					}
					public override void Read(EndianBinaryReader er)
					{
						Offset = er.ReadUInt32();
					}
					public override void Write(EndianBinaryWriter er)
					{
						er.Write(Offset);
					}
					public UInt32 Offset;
				}
				public Shape[] shape;
				public class Shape
				{
					[Flags]
					public enum NNS_G3D_SHPFLAG : uint
					{
						NNS_G3D_SHPFLAG_USE_NORMAL = 0x00000001,
						NNS_G3D_SHPFLAG_USE_COLOR = 0x00000002,
						NNS_G3D_SHPFLAG_USE_TEXCOORD = 0x00000004,
						NNS_G3D_SHPFLAG_USE_RESTOREMTX = 0x00000008
					};
					public Shape(EndianBinaryReader er)
					{
						long pos = er.BaseStream.Position;
						itemTag = er.ReadUInt16();
						size = er.ReadUInt16();
						flag = (NNS_G3D_SHPFLAG)er.ReadUInt32();
						ofsDL = er.ReadUInt32();
						sizeDL = er.ReadUInt32();
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = pos + ofsDL;
						DL = er.ReadBytes((int)sizeDL);
						er.BaseStream.Position = curpos;
					}
					public Shape() { }
					public void Write(EndianBinaryWriter er)
					{
						long offspos = er.BaseStream.Position;
						er.Write(itemTag);
						er.Write((UInt16)0);
						er.Write((UInt32)flag);
						er.Write(ofsDL);
						er.Write((UInt32)DL.Length);
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = offspos + 2;
						er.Write((UInt16)(curpos - offspos));
						er.BaseStream.Position = curpos;
					}
					public UInt16 itemTag;
					public UInt16 size;
					public NNS_G3D_SHPFLAG flag;
					public UInt32 ofsDL;
					public UInt32 sizeDL;

					public byte[] DL;
				}
			}
			public EvpMatrices evpMatrices;
			public class EvpMatrices
			{
				public EvpMatrices(EndianBinaryReader er, int NumNodes)
				{
					m = new envelope[NumNodes];
					for (int i = 0; i < NumNodes; i++)
					{
						m[i] = new envelope(er);
					}
				}
				public void Write(EndianBinaryWriter er)
				{
					foreach (envelope e in m)
					{
						e.Write(er);
					}
				}
				public envelope[] m;
				public class envelope
				{
					public envelope(EndianBinaryReader er)
					{
						invM = Matrix44.Identity;
						for (int y = 0; y < 4; y++)
						{
							for (int x = 0; x < 3; x++)
							{
								invM[x, y] = er.ReadFx32();
							}
						}
						invN = Matrix44.Identity;
						for (int y = 0; y < 3; y++)
						{
							for (int x = 0; x < 3; x++)
							{
								invN[x, y] = er.ReadFx32();
							}
						}
					}
					public void Write(EndianBinaryWriter er)
					{
						for (int y = 0; y < 4; y++)
						{
							for (int x = 0; x < 3; x++)
							{
								er.Write((UInt32)(invM[x, y] * 4096f));
							}
						}
						for (int y = 0; y < 3; y++)
						{
							for (int x = 0; x < 3; x++)
							{
								er.Write((UInt32)(invN[x, y] * 4096f));
							}
						}
					}
					public Matrix44 invM;
					public Matrix44 invN;
				}
			}

			public void ProcessSbc()
			{
				new SBC(sbc, this).Execute();
			}

			/*public void ProcessSbc(float X, float Y, float dist, float elev, float ang, bool picking = false, int texoffset = 1)
			{
				ProcessSbc(null, null, 0, 0, null, 0, 0, null, 0, 0, null, 0, 0, null, 0, 0, X, Y, dist, elev, ang, picking, texoffset);
			}
			public void ProcessSbc(NSBTX.TexplttSet Textures, NSBCA Bca, int BcaAnmNumber, int BcaFrameNumber, NSBTA Bta, int BtaAnmNumber, int BtaFrameNumber, NSBTP Btp, int BtpAnmNumber, int BtpFrameNumber, NSBMA Bma, int BmaAnmNumber, int BmaFrameNumber, NSBVA Bva, int BvaAnmNumber, int BvaFrameNumber, float X, float Y, float dist, float elev, float ang, bool picking = false, int texoffset = 1)
			{
				int mode = 0;
			redo:
				bool UseBca = Bca != null && BcaAnmNumber >= 0;
				bool UseBta = Bta != null && BtaAnmNumber >= 0;
				bool UseBtp = Btp != null && BtpAnmNumber >= 0;
				bool UseBma = Bma != null && BmaAnmNumber >= 0;
				bool UseBva = Bva != null && BvaAnmNumber >= 0;
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
				//Gl.glLoadIdentity();
				//Gl.glScalef(info.invPosScale, info.invPosScale, info.invPosScale);

				var Context = new GlNitro.Nitro3DContext();

				int offset = 0;

				int PosScale = 1;
				PosScale = (int)info.posScale;
				MTX44 curmtx = new MTX44();

				bool nodevisible = true;
				bool render = true;
				while (offset != sbc.Length)
				{
					byte cmd;
					switch ((cmd = sbc[offset++]) & 0xF)
					{
						case 0://NOP
							break;
						case 1://RET
							goto end;
						case 2://NODE
							{
								byte nodeid = sbc[offset++];
								if (!UseBva) nodevisible = sbc[offset++] == 1;
								else
								{
									offset++;
									nodevisible = Bva.visAnmSet.visAnm[BvaAnmNumber].GetFrame(BvaFrameNumber, nodeid);
								}
								break;
							}
						case 3://MTX
							if (nodevisible) curmtx = Context.MatrixStack[sbc[offset++]].Clone();
							else offset++;
							break;
						case 4://MAT
							{
								if (nodevisible && !picking)
								{
									byte matid = sbc[offset++];
									render = true;
									if ((materials.materials[matid].texImageParam & 0xFFFF) != 0)
									{
										System.Windows.MessageBox.Show("Texoffset is not 0!!!");
									}
									if (materials.materials[matid].Fmt != Converters.Graphic.GXTexFmt.GX_TEXFMT_NONE)
									{
										if ((materials.materials[matid].Fmt == Converters.Graphic.GXTexFmt.GX_TEXFMT_A3I5 ||
											materials.materials[matid].Fmt == Converters.Graphic.GXTexFmt.GX_TEXFMT_A5I3 ||
											(int)((materials.materials[matid].polyAttr >> 16) & 31) != 31) && mode != 1) render = false;
										if ((materials.materials[matid].Fmt == Converters.Graphic.GXTexFmt.GX_TEXFMT_NONE ||
											materials.materials[matid].Fmt == Converters.Graphic.GXTexFmt.GX_TEXFMT_PLTT4 ||
											materials.materials[matid].Fmt == Converters.Graphic.GXTexFmt.GX_TEXFMT_PLTT16 ||
											materials.materials[matid].Fmt == Converters.Graphic.GXTexFmt.GX_TEXFMT_PLTT256 ||
											materials.materials[matid].Fmt == Converters.Graphic.GXTexFmt.GX_TEXFMT_COMP4x4 ||
											materials.materials[matid].Fmt == Converters.Graphic.GXTexFmt.GX_TEXFMT_DIRECT) && (int)((materials.materials[matid].polyAttr >> 16) & 31) == 31 && mode != 0) render = false;
									}
									else render = true;
									if (render)
									{
										if (UseBtp && Btp.texPatAnmSet.texPatAnm[BtpAnmNumber].dict.Contains(materials.dict[matid].Key) && Textures != null)
										{
											int texid;
											int palid;
											Btp.texPatAnmSet.texPatAnm[BtpAnmNumber].dict[materials.dict[matid].Key].GetData(out texid, out palid, BtpFrameNumber);
											String tex = Btp.texPatAnmSet.texPatAnm[BtpAnmNumber].texName[texid];
											String pal = Btp.texPatAnmSet.texPatAnm[BtpAnmNumber].plttName[palid];
											var Tex = Textures.dictTex[tex];
											var Pltt = Textures.dictPltt[pal];
											GlNitro.glNitroTexImage2D(Tex.ToBitmap(Pltt), materials.materials[matid], matid + texoffset);
										}
										Gl.glBindTexture(Gl.GL_TEXTURE_2D, matid + texoffset);
										Gl.glMatrixMode(Gl.GL_TEXTURE);
										Gl.glLoadIdentity();
										Gl.glScalef(1f / materials.materials[matid].origWidth, 1f / materials.materials[matid].origHeight, 1f);
										if (!(UseBta && Bta.texSRTAnmSet.texSRTAnm[BtaAnmNumber].Contains(materials.dict[matid].Key) != -1))
										{
											Gl.glMultMatrixf(materials.materials[matid].GetMatrix());
										}
										else
										{
											Gl.glMultMatrixf(Bta.texSRTAnmSet.texSRTAnm[BtaAnmNumber].dict[Bta.texSRTAnmSet.texSRTAnm[BtaAnmNumber].Contains(materials.dict[matid].Key)].Value.GetMatrix(BtaFrameNumber, materials.materials[matid].origWidth, materials.materials[matid].origHeight));
										}

										Context.LightEnabled[0] = ((materials.materials[matid].polyAttr >> 0) & 0x1) == 1;
										Context.LightEnabled[1] = ((materials.materials[matid].polyAttr >> 1) & 0x1) == 1;
										Context.LightEnabled[2] = ((materials.materials[matid].polyAttr >> 2) & 0x1) == 1;
										Context.LightEnabled[3] = ((materials.materials[matid].polyAttr >> 3) & 0x1) == 1;
										//Gl.glEnable(Gl.GL_ALPHA_TEST);
										//Gl.glAlphaFunc(Gl.GL_GEQUAL, 0f);
										if (!UseBma || !Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict.Contains(materials.dict[matid].Key))
										{
											Color Diffuse;
											if ((materials.materials[matid].flag & MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_DIFFUSE) == MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_DIFFUSE) Diffuse = Converters.Graphic.ConvertABGR1555((short)(materials.materials[matid].diffAmb & 0x7FFF));
											else Diffuse = Color.Black;
											Color Ambient;
											if ((materials.materials[matid].flag & MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_AMBIENT) == MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_AMBIENT) Ambient = Converters.Graphic.ConvertABGR1555((short)(materials.materials[matid].diffAmb >> 16 & 0x7FFF));
											else Ambient = Color.FromArgb(160, 160, 160);

											Context.DiffuseColor = Diffuse;
											Context.AmbientColor = Ambient;

											Color Specular;
											if ((materials.materials[matid].flag & MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_SPECULAR) == MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_SPECULAR) Specular = Converters.Graphic.ConvertABGR1555((short)(materials.materials[matid].specEmi & 0x7FFF));
											else Specular = Color.Black;
											Color Emission;
											if ((materials.materials[matid].flag & MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_EMISSION) == MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_EMISSION) Emission = Converters.Graphic.ConvertABGR1555((short)(materials.materials[matid].specEmi >> 16 & 0x7FFF));
											else Emission = Color.Black;

											Context.SpecularColor = Specular;
											Context.EmissionColor = Emission;
										}
										else
										{
											Color Diffuse = Converters.Graphic.ConvertABGR1555((short)(Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].GetValue(Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].tagDiffuse, Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].ConstDiffuse, Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].Diffuse, BmaFrameNumber)));
											Color Ambient = Converters.Graphic.ConvertABGR1555((short)(Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].GetValue(Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].tagAmbient, Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].ConstAmbient, Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].Ambient, BmaFrameNumber)));

											Context.DiffuseColor = Diffuse;
											Context.AmbientColor = Ambient;

											Color Specular = Converters.Graphic.ConvertABGR1555((short)(Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].GetValue(Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].tagSpecular, Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].ConstSpecular, Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].Specular, BmaFrameNumber)));
											Color Emission = Converters.Graphic.ConvertABGR1555((short)(Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].GetValue(Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].tagEmission, Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].ConstEmission, Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].Emission, BmaFrameNumber)));

											Context.SpecularColor = Specular;
											Context.EmissionColor = Emission;
										}
										switch ((materials.materials[matid].polyAttr >> 14) & 0x1)
										{
											case 0: Gl.glDepthFunc(Gl.GL_LESS); break;
											case 1:
												System.Windows.MessageBox.Show("EQUALS!");
												Gl.glDepthFunc(Gl.GL_EQUAL);
												break;
										}

										int mode2 = -1;
										switch ((materials.materials[matid].polyAttr >> 4) & 0x3)
										{
											case 0: mode2 = Gl.GL_MODULATE; break;
											case 1: mode2 = Gl.GL_DECAL; break;
											case 2:
												//Toon/highlight shading. For debugging look at player files from zelda spirit tracks!
												//There's no toon table in a nsbmd, so use default shading
												//TODO: Implement a default toon table
												mode2 = Gl.GL_MODULATE;
												break;
											case 3:
												System.Windows.MessageBox.Show("SHADOW!");
												//Gl.glDepthFunc(Gl.GL_EQUAL);
												mode2 = Gl.GL_MODULATE;
												break;
										}
										Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, mode2);
										if (!UseBma || !Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict.Contains(materials.dict[matid].Key))
										{
											Context.Alpha = (int)((materials.materials[matid].polyAttr >> 16) & 31);
										}
										else
										{
											Context.Alpha = Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].GetValue(Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].tagPolygonAlpha, Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].ConstPolygonAlpha, Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].PolygonAlpha, BmaFrameNumber);
										}
										//if (((materials.materials[matid].polyAttr >> 15) & 1) == 1) Gl.glEnable(Gl.GL_FOG);
										//else Gl.glDisable(Gl.GL_FOG);
										if (!UseBma || !Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict.Contains(materials.dict[matid].Key))
										{
											if (((materials.materials[matid].diffAmb >> 15) & 1) == 1 && (materials.materials[matid].flag & MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_VTXCOLOR) == MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_VTXCOLOR)
											{
												Color Diffuse = Converters.Graphic.ConvertABGR1555((short)(materials.materials[matid].diffAmb & 0x7FFF));
												Gl.glColor4f(Diffuse.R / 255f, Diffuse.G / 255f, Diffuse.B / 255f, Context.Alpha / 31f);
											}
											else
											{
												Gl.glColor4f(0, 0, 0, Context.Alpha / 31f);
											}
										}
										else
										{
											if ((materials.materials[matid].diffAmb >> 15 & 1) == 1)
											{
												Color Diffuse = Converters.Graphic.ConvertABGR1555((short)(Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].GetValue(Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].tagDiffuse, Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].ConstDiffuse, Bma.matColAnmSet.matColAnm[BmaAnmNumber].dict[materials.dict[matid].Key].Diffuse, BmaFrameNumber)));
												Gl.glColor4f(Diffuse.R / 255f, Diffuse.G / 255f, Diffuse.B / 255f, Context.Alpha / 31f);
											}
											else
											{
												Gl.glColor4f(0, 0, 0, Context.Alpha / 31f);
											}
										}
										Context.UseSpecularReflectionTable = (materials.materials[matid].specEmi >> 15 & 1) == 1;
										int cullmode = -1;
										//Gl.glEnable(Gl.GL_CULL_FACE);
										switch (materials.materials[matid].polyAttr >> 6 & 0x03)
										{
											case 0x03: cullmode = Gl.GL_NONE; break;
											case 0x02: cullmode = Gl.GL_BACK; break;
											case 0x01: cullmode = Gl.GL_FRONT; break;
											case 0x00: cullmode = Gl.GL_FRONT_AND_BACK; break;
										}
										Gl.glCullFace(cullmode);

										Gl.glMatrixMode(Gl.GL_MODELVIEW);
										Gl.glDisable(Gl.GL_TEXTURE_GEN_S);
										Gl.glDisable(Gl.GL_TEXTURE_GEN_T);
									}
									else
									{

									}
								}
								else
								{
									offset++;
								}
								break;
							}
						case 5://SHP
							{
								if (nodevisible && render) GlNitro.glNitroGx(shapes.shape[sbc[offset++]].DL, curmtx.Clone(), ref Context, PosScale, picking);
								else offset++;
								break;
							}
						case 6://NODEDESC
							{
								byte nodeid = sbc[offset++];
								byte parentid = sbc[offset++];
								byte segmentscale = sbc[offset++];
								bool thisnodesegmentscale = (segmentscale & 1) == 1;
								bool parentnodesegmentscale = (segmentscale >> 1 & 1) == 1;
								bool MayaScale = thisnodesegmentscale;
								int StackID = ((cmd >> 5 & 0x1) == 1) ? sbc[offset++] : -1;
								int RestID = ((cmd >> 6 & 0x1) == 1) ? sbc[offset++] : -1;

								if (RestID != -1) curmtx = Context.MatrixStack[RestID];
								if (!UseBca) curmtx = curmtx.MultMatrix(nodes.data[nodeid].GetMatrix(MayaScale, PosScale));
								else
								{
									try
									{
										curmtx = curmtx.MultMatrix(Bca.jntAnmSet.jntAnm[BcaAnmNumber].tagData[nodeid].GetMatrix(BcaFrameNumber, MayaScale, PosScale, nodes.data[nodeid]));
									}
									catch
									{
										curmtx = curmtx.MultMatrix(nodes.data[nodeid].GetMatrix(MayaScale, PosScale));
									}
								}
								if (StackID != -1) Context.MatrixStack[StackID] = curmtx;
								break;
							}
						case 7://BB
							{
								Gl.glCullFace(Gl.GL_NONE);
								byte nodeid = sbc[offset++];
								int StackID = ((cmd >> 5 & 0x1) == 1) ? sbc[offset++] : -1;
								int RestID = ((cmd >> 6 & 0x1) == 1) ? sbc[offset++] : -1;

								if (RestID != -1) curmtx = Context.MatrixStack[RestID];

								float[] proj = new float[16];
								Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, proj);

								curmtx[0, 0] = proj[0];
								curmtx[0, 1] = proj[1];
								curmtx[0, 2] = proj[2];

								curmtx[1, 0] = proj[4];
								curmtx[1, 1] = proj[5];
								curmtx[1, 2] = proj[6];

								curmtx[2, 0] = proj[8];
								curmtx[2, 1] = proj[9];
								curmtx[2, 2] = proj[10];

								if (StackID != -1) Context.MatrixStack[StackID] = curmtx;
								break;
							}
						case 8://BBY
							{
								Gl.glCullFace(Gl.GL_NONE);
								byte nodeid = sbc[offset++];
								int StackID = ((cmd >> 5 & 0x1) == 1) ? sbc[offset++] : -1;
								int RestID = ((cmd >> 6 & 0x1) == 1) ? sbc[offset++] : -1;

								if (RestID != -1) curmtx = Context.MatrixStack[RestID];

								float[] proj = new float[16];
								Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, proj);

								//	curmtx = curmtx.MultMatrix(nodes.data[nodeid].GetMatrix(false, PosScale));

								/*curmtx[0, 0] = proj[0];
								//curmtx[0, 1] = proj[1];
								curmtx[0, 2] = proj[2];

								//curmtx[1, 0] = proj[4];
								//curmtx[1, 1] = proj[5];
								//curmtx[1, 2] = proj[6];

								curmtx[2, 0] = proj[8];
								//curmtx[2, 1] = proj[9];
								curmtx[2, 2] = proj[10];/
								curmtx[0, 0] = proj[0];
								//curmtx[0, 1] = proj[1];
								curmtx[0, 2] = proj[2];

								curmtx[1, 0] = proj[4];
								curmtx[1, 1] = proj[5];
								curmtx[1, 2] = proj[6];

								curmtx[2, 0] = proj[8];
								//curmtx[2, 1] = proj[9];
								curmtx[2, 2] = proj[10];

								if (StackID != -1) Context.MatrixStack[StackID] = curmtx;
								break;
							}
						case 9://NODEMIX
							{
								MTX44 M = new MTX44();//MtxFx43
								M.Zero();
								MTX44 N = new MTX44();//MtxFx33
								N.Zero();

								MTX44 x = new MTX44();//MtxFx44
								MTX44 y = new MTX44();//MtxFx33

								float w = 0;


								byte stackid = sbc[offset++];
								byte nummatrices = sbc[offset++];
								for (int i = 0; i < nummatrices; i++)
								{
									byte SrcIdx_N = sbc[offset++];
									byte NodeID_N = sbc[offset++];
									float Ratio_N = sbc[offset++] / 256f;

									curmtx = Context.MatrixStack[SrcIdx_N];
									curmtx = curmtx.MultMatrix(evpMatrices.m[NodeID_N].invM);

									if (i != 0)
									{
										N += w * y;
									}

									x = curmtx;

									curmtx = curmtx.MultMatrix(evpMatrices.m[NodeID_N].invN);
									w = Ratio_N;
									M += w * x;

									y = curmtx;
								}
								N += w * y;
								curmtx = M;// *(1f / PosScale);
								Context.MatrixStack[stackid] = curmtx;
								break;
							}
						case 10://CALLDL
							{
								int RelAddr = Converters.Bytes.Read4BytesAsInt32(sbc, offset);
								offset += 4;
								int Size = Converters.Bytes.Read4BytesAsInt32(sbc, offset);
								offset += 4;
								GlNitro.glNitroGx(sbc.ToList().GetRange(offset - 9 + RelAddr, Size).ToArray(), curmtx.Clone(), /*31, ref MatrixStack/ ref Context, PosScale);
								break;
							}
						case 11://POSSCALE
							{
								if (cmd >> 5 == 0)
								{
									//PosScale = (int)info.posScale;
									//Gl.glScalef(info.posScale, info.posScale, info.posScale);
									//Gl.glScalef(1f / info.posScale, 1f / info.posScale, 1f / info.posScale);
								}
								else
								{
									//PosScale = 1;
									//Gl.glScalef(info.posScale, info.posScale, info.posScale);
									//Gl.glScalef(1f / info.posScale, 1f / info.posScale, 1f / info.posScale);
								}
								break;
							}
						case 12://ENVMAP
							{
								byte matid = sbc[offset++];
								byte flag = sbc[offset++];
								if (nodevisible && !picking)
								{
									float[] proj = new float[16];
									Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, proj);

									Gl.glMatrixMode(Gl.GL_TEXTURE);
									Gl.glLoadIdentity();
									MaterialSet.Material m = materials.materials[matid];
									Gl.glScalef(0.5f, -0.5f, 1);
									Gl.glTranslatef(0.5f, 0.5f, 0);

									if ((m.flag & MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_EFFECTMTX) != 0)
									{
										Gl.glMultMatrixf(m.effectMtx);
									}

									MTX44 mvm = new MTX44();
									mvm.SetValues(proj);

									mvm.MultMatrix(curmtx);

									mvm[12] = 0;
									mvm[13] = 0;
									mvm[14] = 0;

									Gl.glMultMatrixf(mvm);

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
								byte matid = sbc[offset++];
								byte flag = sbc[offset++];
								Gl.glTexGeni(Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_OBJECT_LINEAR);
								Gl.glTexGeni(Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_OBJECT_LINEAR);
								Gl.glEnable(Gl.GL_TEXTURE_GEN_S);
								Gl.glEnable(Gl.GL_TEXTURE_GEN_T);
								break;
							}
					}

				}
			end:
				if (mode == 0)
				{
					mode = 1;
					if (picking)
					{
						Gl.glDepthMask(1);
					}
					else
					{
						//Gl.glDepthMask(Gl.GL_FALSE);
						//Gl.glEnable(Gl.GL_DEPTH_TEST);

						//Gl.glDepthMask(Gl.GL_FALSE);
						//Gl.glBlendFunc(Gl.GL_ONE_MINUS_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
						//
					}
					goto redo;
				}
			}
			public byte[] ExportBones()
			{
				//Gl.glLoadIdentity();
				//Gl.glScalef(info.invPosScale, info.invPosScale, info.invPosScale);
				MTX44[] MatrixStack = new MTX44[31];
				for (int i = 0; i < 31; i++)
				{
					MatrixStack[i] = new MTX44();
				}
				int offset = 0;
				//int curstackid = -1;
				//int stackid = -1;
				//Matrix4 PolyMtx = new Matrix4();
				//Matrix4 PolyMtx2 = new Matrix4();
				int PosScale = 1;
				PosScale = (int)info.posScale;
				MTX44 curmtx = new MTX44();
				//int stackID = -1;

				List<_3D_Formats.MA.Node> Nodes = new List<_3D_Formats.MA.Node>();

				//int stackid2 = -1;
				//int PolygonStackID = -1;
				//int MatID = -1;
				//int JointID = -1;
				bool nodevisible = true;
				while (offset != sbc.Length)
				{
					byte cmd;
					switch ((cmd = sbc[offset++]) & 0xF)
					{
						case 0://NOP
							break;
						case 1://RET
							goto end;
						case 2://NODE
							{
								byte nodeid = sbc[offset++];
								nodevisible = sbc[offset++] == 1;
								break;
							}
						case 3://MTX
							if (nodevisible)
							{
								curmtx = MatrixStack[sbc[offset++]].Clone();
							}
							else
							{
								offset++;
							}
							break;
						case 4://MAT
							{
								offset++;
								break;
							}
						case 5://SHP
							{
								offset++;
								break;
							}
						case 6://NODEDESC
							{
								byte nodeid = sbc[offset++];
								byte parentid = sbc[offset++];
								byte segmentscale = sbc[offset++];
								bool thisnodesegmentscale = (segmentscale & 1) == 1;
								bool parentnodesegmentscale = (segmentscale >> 1 & 1) == 1;
								bool MayaScale = thisnodesegmentscale;
								int StackID = ((cmd >> 5 & 0x1) == 1) ? sbc[offset++] : -1;
								int RestID = ((cmd >> 6 & 0x1) == 1) ? sbc[offset++] : -1;

								if (RestID != -1)
								{
									curmtx = MatrixStack[RestID];
								}
								curmtx = curmtx.MultMatrix(nodes.data[nodeid].GetMatrix(MayaScale, PosScale));

								Nodes.Add(new _3D_Formats.MA.Node(curmtx, nodes.dict.names[nodeid], (parentid == 0xFF ? null : nodes.dict.names[parentid])));

								if (StackID != -1)
								{
									MatrixStack[StackID] = curmtx;
								}
								break;
							}
						case 7://BB
							{
								byte nodeid = sbc[offset++];
								int StackID = ((cmd >> 5 & 0x1) == 1) ? sbc[offset++] : -1;
								int RestID = ((cmd >> 6 & 0x1) == 1) ? sbc[offset++] : -1;
								break;
							}
						case 8://BBY
							{
								byte nodeid = sbc[offset++];
								int StackID = ((cmd >> 5 & 0x1) == 1) ? sbc[offset++] : -1;
								int RestID = ((cmd >> 6 & 0x1) == 1) ? sbc[offset++] : -1;
								break;
							}
						case 9://NODEMIX
							{
								MTX44 M = new MTX44();//MtxFx43
								M.Zero();
								MTX44 N = new MTX44();//MtxFx33
								N.Zero();

								MTX44 x = new MTX44();//MtxFx44
								MTX44 y = new MTX44();//MtxFx33

								float w = 0;


								byte stackid = sbc[offset++];
								byte nummatrices = sbc[offset++];
								for (int i = 0; i < nummatrices; i++)
								{
									byte SrcIdx_N = sbc[offset++];
									byte NodeID_N = sbc[offset++];
									float Ratio_N = sbc[offset++] / 256f;

									curmtx = MatrixStack[SrcIdx_N];
									curmtx = curmtx.MultMatrix(evpMatrices.m[NodeID_N].invM);

									if (i != 0)
									{
										N += w * y;
									}

									x = curmtx;

									curmtx = curmtx.MultMatrix(evpMatrices.m[NodeID_N].invN);
									w = Ratio_N;
									M += w * x;

									y = curmtx;
								}
								N += w * y;
								curmtx = M;// *(1f / PosScale);
								MatrixStack[stackid] = curmtx;
								break;
							}
						case 10://CALLDL
							{
								int RelAddr = Converters.Bytes.Read4BytesAsInt32(sbc, offset);
								offset += 4;
								int Size = Converters.Bytes.Read4BytesAsInt32(sbc, offset);
								offset += 4;
								break;
							}
						case 11://POSSCALE
							{
								break;
							}
						case 12://ENVMAP
							{
								byte matid = sbc[offset++];
								byte flag = sbc[offset++];
								break;
							}
						case 13://PRJMAP
							{
								byte matid = sbc[offset++];
								byte flag = sbc[offset++];
								break;
							}
					}

				}
			end:
				return _3D_Formats.MA.WriteBones(Nodes.ToArray());
			}
			public void ExportMesh(NSBTX.TexplttSet Textures, string FileName, string ImageFormat)
			{
				List<_3D_Formats.Group> Groups = new List<_3D_Formats.Group>();
				List<String> Material = new List<string>();
				MaterialSet.Material m = null;
				//Gl.glLoadIdentity();
				//Gl.glScalef(info.invPosScale, info.invPosScale, info.invPosScale);
				MTX44[] MatrixStack = new MTX44[31];
				for (int i = 0; i < 31; i++)
				{
					MatrixStack[i] = new MTX44();
				}
				int offset = 0;
				//int curstackid = -1;
				//int stackid = -1;
				//Matrix4 PolyMtx = new Matrix4();
				//Matrix4 PolyMtx2 = new Matrix4();
				int PosScale = 1;
				PosScale = (int)info.posScale;
				MTX44 curmtx = new MTX44();
				//int stackID = -1;


				//int stackid2 = -1;
				//int PolygonStackID = -1;
				//int MatID = -1;
				//int JointID = -1;
				bool nodevisible = true;
				bool render = true;
				int alpha = 31;
				while (offset != sbc.Length)
				{
					byte cmd;
					switch ((cmd = sbc[offset++]) & 0xF)
					{
						case 0://NOP
							break;
						case 1://RET
							goto end;
						case 2://NODE
							{
								byte nodeid = sbc[offset++];
								nodevisible = sbc[offset++] == 1;
								break;
							}
						case 3://MTX
							if (nodevisible)
							{
								curmtx = MatrixStack[sbc[offset++]].Clone();
							}
							else
							{
								offset++;
							}
							break;
						case 4://MAT
							{
								byte matid = sbc[offset++];
								m = materials.materials[matid];
								Material.Add(materials.dict.names[matid]);
								break;
							}
						case 5://SHP
							{
								Groups.Add(GlNitro.glNitroGxRipper(shapes.shape[sbc[offset++]].DL, curmtx.Clone(), alpha, ref MatrixStack, PosScale, m));
								break;
							}
						case 6://NODEDESC
							{
								byte nodeid = sbc[offset++];
								byte parentid = sbc[offset++];
								byte segmentscale = sbc[offset++];
								bool thisnodesegmentscale = (segmentscale & 1) == 1;
								bool parentnodesegmentscale = (segmentscale >> 1 & 1) == 1;
								bool MayaScale = thisnodesegmentscale;
								int StackID = ((cmd >> 5 & 0x1) == 1) ? sbc[offset++] : -1;
								int RestID = ((cmd >> 6 & 0x1) == 1) ? sbc[offset++] : -1;

								if (RestID != -1)
								{
									curmtx = MatrixStack[RestID];
								}
								curmtx = curmtx.MultMatrix(nodes.data[nodeid].GetMatrix(MayaScale, PosScale));

								if (StackID != -1)
								{
									MatrixStack[StackID] = curmtx;
								}
								break;
							}
						case 7://BB
							{
								Gl.glCullFace(Gl.GL_NONE);
								byte nodeid = sbc[offset++];
								int StackID = ((cmd >> 5 & 0x1) == 1) ? sbc[offset++] : -1;
								int RestID = ((cmd >> 6 & 0x1) == 1) ? sbc[offset++] : -1;
								break;
							}
						case 8://BBY
							{
								Gl.glCullFace(Gl.GL_NONE);
								byte nodeid = sbc[offset++];
								int StackID = ((cmd >> 5 & 0x1) == 1) ? sbc[offset++] : -1;
								int RestID = ((cmd >> 6 & 0x1) == 1) ? sbc[offset++] : -1;
								break;
							}
						case 9://NODEMIX
							{
								MTX44 M = new MTX44();//MtxFx43
								M.Zero();
								MTX44 N = new MTX44();//MtxFx33
								N.Zero();

								MTX44 x = new MTX44();//MtxFx44
								MTX44 y = new MTX44();//MtxFx33

								float w = 0;


								byte stackid = sbc[offset++];
								byte nummatrices = sbc[offset++];
								for (int i = 0; i < nummatrices; i++)
								{
									byte SrcIdx_N = sbc[offset++];
									byte NodeID_N = sbc[offset++];
									float Ratio_N = sbc[offset++] / 256f;

									curmtx = MatrixStack[SrcIdx_N];
									curmtx = curmtx.MultMatrix(evpMatrices.m[NodeID_N].invM);

									if (i != 0)
									{
										N += w * y;
									}

									x = curmtx;

									curmtx = curmtx.MultMatrix(evpMatrices.m[NodeID_N].invN);
									w = Ratio_N;
									M += w * x;

									y = curmtx;
								}
								N += w * y;
								curmtx = M;// *(1f / PosScale);
								MatrixStack[stackid] = curmtx;
								break;
							}
						case 10://CALLDL
							{
								int RelAddr = Converters.Bytes.Read4BytesAsInt32(sbc, offset);
								offset += 4;
								int Size = Converters.Bytes.Read4BytesAsInt32(sbc, offset);
								offset += 4;
								Groups.Add(GlNitro.glNitroGxRipper(sbc.ToList().GetRange(offset - 9 + RelAddr, Size).ToArray(), curmtx.Clone(), 31, ref MatrixStack, PosScale, m));
								break;
							}
						case 11://POSSCALE
							{
								if (cmd >> 5 == 0)
								{
									//PosScale = (int)info.posScale;
									//Gl.glScalef(info.posScale, info.posScale, info.posScale);
									//Gl.glScalef(1f / info.posScale, 1f / info.posScale, 1f / info.posScale);
								}
								else
								{
									//PosScale = 1;
									//Gl.glScalef(info.posScale, info.posScale, info.posScale);
									//Gl.glScalef(1f / info.posScale, 1f / info.posScale, 1f / info.posScale);
								}
								break;
							}
						case 12://ENVMAP
							{
								byte matid = sbc[offset++];
								byte flag = sbc[offset++];
								break;
							}
						case 13://PRJMAP
							{
								byte matid = sbc[offset++];
								byte flag = sbc[offset++];
								break;
							}
					}

				}
			end:
				System.IO.File.Create(FileName).Close();
				TextWriter tw = new StreamWriter(FileName);
				tw.WriteLine("# Created by MKDS Course Modifier");
				tw.WriteLine("mtllib {0}", Path.ChangeExtension(Path.GetFileName(FileName), "mtl"));
				int nr = 1;
				int l = 0;
				foreach (_3D_Formats.Group g in Groups)
				{
					tw.WriteLine("g {0}", shapes.dict.names[l]);
					tw.WriteLine("usemtl {0}", Material[l]);
					foreach (_3D_Formats.Polygon p in g)
					{
						foreach (OpenTK.Vector3 v in p.Vertex)
						{
							tw.WriteLine("v {0} {1} {2}", (v.X * this.info.posScale).ToString().Replace(",", "."), (v.Y * this.info.posScale).ToString().Replace(",", "."), (v.Z * this.info.posScale).ToString().Replace(",", "."));
						}
						foreach (OpenTK.Vector3 v in p.Normals)
						{
							tw.WriteLine("vn {0} {1} {2}", (v.X).ToString().Replace(",", "."), (v.Y).ToString().Replace(",", "."), (v.Z).ToString().Replace(",", "."));
						}
						foreach (OpenTK.Vector2 v in p.TexCoords)
						{
							tw.WriteLine("vt {0} {1}", (v.X).ToString().Replace(",", "."), (v.Y).ToString().Replace(",", "."));
						}
						switch (p.PolyType)
						{
							case _3D_Formats.PolygonType.Triangle:
								for (int i = 0; i < p.Vertex.Length; i += 3)
								{
									tw.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", nr, nr + 1, nr + 2);
									nr += 3;
								}
								break;
							case _3D_Formats.PolygonType.Quad:
								for (int i = 0; i < p.Vertex.Length; i += 4)
								{
									tw.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2} {3}/{3}/{3}", nr, nr + 1, nr + 2, nr + 3);
									nr += 4;
								}
								break;
							case _3D_Formats.PolygonType.TriangleStrip:
								for (int i = 0; i + 2 < p.Vertex.Length; i += 2)
								{
									string s = "f";
									s += string.Format(" {0}/{0}/{0}", nr + i);
									s += string.Format(" {0}/{0}/{0}", nr + i + 1);
									s += string.Format(" {0}/{0}/{0}", nr + i + 2);
									tw.WriteLine(s);

									if (i + 3 < p.Vertex.Length)
									{
										s = "f";
										s += string.Format(" {0}/{0}/{0}", nr + i + 1);
										s += string.Format(" {0}/{0}/{0}", nr + i + 3);
										s += string.Format(" {0}/{0}/{0}", nr + i + 2);
										tw.WriteLine(s);
									}

								}
								nr += p.Vertex.Length;
								break;
							case _3D_Formats.PolygonType.QuadStrip:
								for (int i = 0; i + 2 < p.Vertex.Length; i += 2)
								{
									string s = "f";
									s += string.Format(" {0}/{0}/{0}", nr + i);
									s += string.Format(" {0}/{0}/{0}", nr + i + 1);
									s += string.Format(" {0}/{0}/{0}", nr + i + 2);
									tw.WriteLine(s);

									if (i + 3 < p.Vertex.Length)
									{
										s = "f";
										s += string.Format(" {0}/{0}/{0}", nr + i + 1);
										s += string.Format(" {0}/{0}/{0}", nr + i + 3);
										s += string.Format(" {0}/{0}/{0}", nr + i + 2);
										tw.WriteLine(s);
									}

								}
								nr += p.Vertex.Length;
								break;
						}
					}
					l++;
				}
				tw.Close();
				System.IO.File.Create(Path.ChangeExtension(FileName, "mtl")).Close();
				tw = new StreamWriter(Path.ChangeExtension(FileName, "mtl"));
				int q = 0;
				foreach (MaterialSet.Material mat in materials.materials)
				{
					tw.WriteLine("newmtl {0}", materials.dict.names[q]);
					Color Diffuse = Converters.Graphic.ConvertABGR1555((short)(mat.diffAmb & 0x7FFF));
					Color Ambient = Converters.Graphic.ConvertABGR1555((short)(mat.diffAmb >> 16 & 0x7FFF));
					tw.WriteLine("Ka {0} {1} {2}", (Ambient.R / 255f).ToString().Replace(",", "."), (Ambient.G / 255f).ToString().Replace(",", "."), (Ambient.B / 255f).ToString().Replace(",", "."));
					tw.WriteLine("Kd {0} {1} {2}", (Diffuse.R / 255f).ToString().Replace(",", "."), (Diffuse.G / 255f).ToString().Replace(",", "."), (Diffuse.B / 255f).ToString().Replace(",", "."));
					tw.WriteLine("d {0}", (((mat.polyAttr >> 16) & 31) / 31f).ToString().Replace(",", "."));
					tw.WriteLine("Tr {0}", (((mat.polyAttr >> 16) & 31) / 31f).ToString().Replace(",", "."));
					tw.WriteLine("map_Ka {0}.{1}", materials.dict.names[q], ImageFormat.ToLower());
					tw.WriteLine("map_Kd {0}.{1}", materials.dict.names[q], ImageFormat.ToLower());
					tw.WriteLine("map_d {0}.{1}", materials.dict.names[q], ImageFormat.ToLower());
					q++;
				}
				tw.Close();
				if (Textures == null) return;
				for (int j = 0; j < materials.materials.Length; j++)
				{
					NSBTX.TexplttSet.DictTexData t = null;
					for (int k = 0; k < materials.dictTexToMatList.numEntry; k++)
					{
						if (materials.dictTexToMatList[k].Value.Materials.Contains(j))
						{
							int texid = k;
							for (int z = 0; z < Textures.dictTex.numEntry; z++)
							{
								if (Textures.dictTex[z].Key == materials.dictTexToMatList[k].Key) { texid = z; break; }
							}
							t = Textures.dictTex[texid].Value;
							break;
						}
					}
					if (t == null)
						continue;
					NSBTX.TexplttSet.DictPlttData p = null;
					if (t.Fmt != MKDS_Course_Modifier.Converters.Graphic.GXTexFmt.GX_TEXFMT_DIRECT)
					{
						for (int k = 0; k < materials.dictPlttToMatList.numEntry; k++)
						{
							if (materials.dictPlttToMatList[k].Value.Materials.Contains(j))
							{
								int palid = k;
								for (int z = 0; z < Textures.dictPltt.numEntry; z++)
								{
									if (Textures.dictPltt[z].Key == materials.dictPlttToMatList[k].Key) { palid = z; break; }
								}
								p = Textures.dictPltt[palid].Value;
								break;
							}
						}
					}
					Bitmap qq = t.ToBitmap(p);
					Bitmap qq2 = new Bitmap((int)(qq.Width * ((materials.materials[j].texImageParam >> 18 & 0x1) + 1)), (int)(qq.Height * ((materials.materials[j].texImageParam >> 19 & 0x1) + 1)));
					using (Graphics g = Graphics.FromImage(qq2))
					{
						g.DrawImage(qq, 0, 0);
						bool S = false;
						bool T = false;
						if ((materials.materials[j].texImageParam >> 16 & 0x1) == 1)
						{
							if ((materials.materials[j].texImageParam >> 18 & 0x1) == 1)
							{
								g.DrawImage(qq, qq.Width * 2, 0, -qq.Width, qq.Height);
								S = true;
							}
						}
						if ((materials.materials[j].texImageParam >> 17 & 0x1) == 1)
						{
							if ((materials.materials[j].texImageParam >> 19 & 0x1) == 1)
							{
								g.DrawImage(qq, 0, qq.Height * 2, qq.Width, -qq.Height);
								T = true;
							}
						}
						if (S && T)
						{
							g.DrawImage(qq, qq.Width * 2, qq.Height * 2, -qq.Width, -qq.Height);
						}
					}
					switch (ImageFormat)
					{
						case "PNG":
							qq2.Save(Path.GetDirectoryName(FileName) + "\\" + materials.dict.names[j] + ".png", System.Drawing.Imaging.ImageFormat.Png);
							break;
						case "TIFF":
							qq2.Save(Path.GetDirectoryName(FileName) + "\\" + materials.dict.names[j] + ".tiff", System.Drawing.Imaging.ImageFormat.Tiff);
							break;
						case "TGA":
							Misc.DevIl.SaveAsTGA(qq2, Path.GetDirectoryName(FileName) + "\\" + materials.dict.names[j] + ".tga");
							break;
					}
				}
			}*/
		}
	}
}
