using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using LibEveryFileExplorer.Collections;
using System.IO;
using LibEveryFileExplorer._3D;
using CommonFiles;
using LibEveryFileExplorer.IO;

namespace MarioKart.MKDS
{
	public class KCL : FileFormat<KCL.KCLIdentifier>, IConvertable, IFileCreatable, IViewable, IWriteable
	{
		public KCL()
		{
			Header = new MKDSKCLHeader();
		}

		public KCL(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new MKDSKCLHeader(er);
				er.BaseStream.Position = Header.VerticesOffset;
				uint nr = (Header.NormalsOffset - Header.VerticesOffset) / 0xC;
				Vertices = new Vector3[nr];
				for (int i = 0; i < nr; i++) Vertices[i] = er.ReadVecFx32();

				er.BaseStream.Position = Header.NormalsOffset;
				nr = (Header.PlanesOffset - Header.NormalsOffset) / 0x6;
				Normals = new Vector3[nr];
				for (int i = 0; i < nr; i++) Normals[i] = er.ReadVecFx16();

				er.BaseStream.Position = Header.PlanesOffset;
				nr = (Header.OctreeOffset - Header.PlanesOffset) / 0x10;
				Planes = new KCLPlane[nr];
				for (int i = 0; i < nr; i++) Planes[i] = new KCLPlane(er);

				er.BaseStream.Position = Header.OctreeOffset;
				int nodes = (int)(
					((~Header.XMask >> (int)Header.CoordShift) + 1) *
					((~Header.YMask >> (int)Header.CoordShift) + 1) *
					((~Header.ZMask >> (int)Header.CoordShift) + 1));
				Octree = new KCLOctree(er, nodes);
			}
			finally
			{
				er.Close();
			}
		}
		public System.Windows.Forms.Form GetDialog()
		{
			return new System.Windows.Forms.Form();
			//throw new NotImplementedException();
		}

		public string GetSaveDefaultFileFilter()
		{
			return "Mario Kart DS Collision (*.kcl)|*.kcl";
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			Header.Write(er);
			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = 0;
			er.Write((uint)curpos);
			er.BaseStream.Position = curpos;
			foreach (Vector3 v in Vertices)
			{
				er.Write((int)(v.X * 4096f));
				er.Write((int)(v.Y * 4096f));
				er.Write((int)(v.Z * 4096f));
			}
			while ((er.BaseStream.Position % 4) != 0) er.Write((byte)0);
			curpos = er.BaseStream.Position;
			er.BaseStream.Position = 4;
			er.Write((uint)curpos);
			er.BaseStream.Position = curpos;
			foreach (Vector3 v in Normals)
			{
				er.Write((short)(v.X * 4096f));
				er.Write((short)(v.Y * 4096f));
				er.Write((short)(v.Z * 4096f));
			}
			while ((er.BaseStream.Position % 4) != 0) er.Write((byte)0);
			curpos = er.BaseStream.Position;
			er.BaseStream.Position = 8;
			er.Write((uint)(curpos - 0x10));
			er.BaseStream.Position = curpos;
			foreach (KCLPlane p in Planes) p.Write(er);
			curpos = er.BaseStream.Position;
			er.BaseStream.Position = 12;
			er.Write((uint)curpos);
			er.BaseStream.Position = curpos;
			Octree.Write(er);
			byte[] result = m.ToArray();
			er.Close();
			return result;
		}

		public string GetConversionFileFilters()
		{
			return "Wavefront OBJ File (*.obj)|*.obj";//|Mario Kart DS Collision (*.kcl)|*.kcl"
		}

		public bool Convert(int FilterIndex, string Path)
		{
			switch (FilterIndex)
			{
				case 0:
					OBJ o = ToOBJ();
					byte[] d = o.Write();
					File.Create(Path).Close();
					File.WriteAllBytes(Path, d);
					return true;
				default:
					return false;
			}
		}

		public bool CreateFromFile()
		{
			System.Windows.Forms.OpenFileDialog f = new System.Windows.Forms.OpenFileDialog();
			f.Filter = OBJ.Identifier.GetFileFilter();
			if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& f.FileName.Length > 0)
			{
				OBJ o = new OBJ(File.ReadAllBytes(f.FileName));
				List<String> matnames = new List<string>();
				foreach (var v in o.Faces) if (!matnames.Contains(v.Material)) matnames.Add(v.Material);
				UI.KCLCollisionTypeSelector ty = new UI.KCLCollisionTypeSelector(matnames.ToArray());
				ty.DialogResult = System.Windows.Forms.DialogResult.None;
				ty.ShowDialog();
				while (ty.DialogResult != System.Windows.Forms.DialogResult.OK) ;
				Dictionary<string, ushort> Mapping;
				Dictionary<string, bool> Colli;
				Mapping = ty.Mapping;
				Colli = ty.Colli;
				List<Vector3> Vertex = new List<Vector3>();
				List<Vector3> Normals = new List<Vector3>();
				List<KCLPlane> planes = new List<KCLPlane>();
				List<Triangle> Triangles = new List<Triangle>();
				foreach (var v in o.Faces)
				{
					if (Colli[v.Material])
					{
						Triangle t = new Triangle(o.Vertices[v.VertexIndieces[0]], o.Vertices[v.VertexIndieces[1]], o.Vertices[v.VertexIndieces[2]]);
						Vector3 qq = (t.PointB - t.PointA).Cross(t.PointC - t.PointA);
						if ((qq.X * qq.X + qq.Y * qq.Y + qq.Z * qq.Z) < 0.01) continue;
						KCLPlane p = new KCLPlane();
						p.CollisionType = (ushort)(((Mapping[v.Material] & 0xFF) << 8) | (Mapping[v.Material] >> 8));
						Vector3 a = (t.PointC - t.PointA).Cross(t.Normal);
						a.Normalize();
						a = -a;
						Vector3 b = (t.PointB - t.PointA).Cross(t.Normal);
						b.Normalize();
						Vector3 c = (t.PointC - t.PointB).Cross(t.Normal);
						c.Normalize();
						p.Length = (t.PointC - t.PointA).Dot(c);
						int q = ContainsVector3(t.PointA, Vertex);
						if (q == -1) { p.VertexIndex = (ushort)Vertex.Count; Vertex.Add(t.PointA); }
						else p.VertexIndex = (ushort)q;
						q = ContainsVector3(t.Normal, Normals);
						if (q == -1) { p.NormalIndex = (ushort)Normals.Count; Normals.Add(t.Normal); }
						else p.NormalIndex = (ushort)q;
						q = ContainsVector3(a, Normals);
						if (q == -1) { p.NormalAIndex = (ushort)Normals.Count; Normals.Add(a); }
						else p.NormalAIndex = (ushort)q;
						q = ContainsVector3(b, Normals);
						if (q == -1) { p.NormalBIndex = (ushort)Normals.Count; Normals.Add(b); }
						else p.NormalBIndex = (ushort)q;
						q = ContainsVector3(c, Normals);
						if (q == -1) { p.NormalCIndex = (ushort)Normals.Count; Normals.Add(c); }
						else p.NormalCIndex = (ushort)q;
						planes.Add(p);
						Triangles.Add(t);
					}
				}
				Vertices = Vertex.ToArray();
				this.Normals = Normals.ToArray();
				Planes = planes.ToArray();
				Header = new MKDSKCLHeader();
				Octree = KCLOctree.FromTriangles(Triangles.ToArray(), Header, 2048, 128, 128, 50);
				return true;
			}
			return false;
		}

		private int ContainsVector3(Vector3 a, List<Vector3> b)
		{
			for (int i = 0; i < b.Count; i++)
			{
				if (b[i].X == a.X && b[i].Y == a.Y && b[i].Z == a.Z)
				{
					return i;
				}
			}
			return -1;
		}

		public MKDSKCLHeader Header;
		public class MKDSKCLHeader : KCLHeader
		{
			public MKDSKCLHeader() { }
			public MKDSKCLHeader(EndianBinaryReader er)
			{
				VerticesOffset = er.ReadUInt32();
				NormalsOffset = er.ReadUInt32();
				PlanesOffset = er.ReadUInt32() + 0x10;
				OctreeOffset = er.ReadUInt32();
				Unknown1 = er.ReadFx32();
				OctreeOrigin = er.ReadVecFx32();
				XMask = er.ReadUInt32();
				YMask = er.ReadUInt32();
				ZMask = er.ReadUInt32();
				CoordShift = er.ReadUInt32();
				YShift = er.ReadUInt32();
				ZShift = er.ReadUInt32();
				Unknown2 = er.ReadFx32();
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(VerticesOffset);
				er.Write(NormalsOffset);
				er.Write((uint)(PlanesOffset - 0x10));
				er.Write(OctreeOffset);
				er.Write((int)(Unknown1 * 4096f));
				er.Write((int)(OctreeOrigin.X * 4096f));
				er.Write((int)(OctreeOrigin.Y * 4096f));
				er.Write((int)(OctreeOrigin.Z * 4096f));
				er.Write(XMask);
				er.Write(YMask);
				er.Write(ZMask);
				er.Write(CoordShift);
				er.Write(YShift);
				er.Write(ZShift);
				er.Write((int)(Unknown2 * 4096f));
			}
		}

		public Vector3[] Vertices;
		public Vector3[] Normals;

		public KCLPlane[] Planes;
		public class KCLPlane
		{
			public KCLPlane() { }
			public KCLPlane(EndianBinaryReader er)
			{
				Length = er.ReadFx32();
				VertexIndex = er.ReadUInt16();
				NormalIndex = er.ReadUInt16();
				NormalAIndex = er.ReadUInt16();
				NormalBIndex = er.ReadUInt16();
				NormalCIndex = er.ReadUInt16();
				CollisionType = er.ReadUInt16();
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write((int)(Length * 4096f));
				er.Write(VertexIndex);
				er.Write(NormalIndex);
				er.Write(NormalAIndex);
				er.Write(NormalBIndex);
				er.Write(NormalCIndex);
				er.Write(CollisionType);
			}

			public Single Length;
			public UInt16 VertexIndex;
			public UInt16 NormalIndex;
			public UInt16 NormalAIndex;
			public UInt16 NormalBIndex;
			public UInt16 NormalCIndex;
			public UInt16 CollisionType;
		}
		//nr tris, depth
		//CrossCourse: 34, 5
		//Stage4: 43, 3
		//Yoshi: 48, 5
		//Test1Course: 56, 5
		//Mansion: 24, 6

		//minimal cube size for root = 128, max 2048

		//It looks like the min cube size is 32 and the max number of tris when cube > 32, 35
		public KCLOctree Octree;

		public Triangle GetTriangle(KCLPlane Plane)
		{
			Vector3 A = Vertices[Plane.VertexIndex];
			Vector3 CrossA = Normals[Plane.NormalAIndex].Cross(Normals[Plane.NormalIndex]);
			Vector3 CrossB = Normals[Plane.NormalBIndex].Cross(Normals[Plane.NormalIndex]);
			Vector3 B = A + CrossB * (Plane.Length / CrossB.Dot(Normals[Plane.NormalCIndex]));
			Vector3 C = A + CrossA * (Plane.Length / CrossA.Dot(Normals[Plane.NormalCIndex]));
			if (float.IsInfinity(B.X) || float.IsNaN(B.X) ||
				float.IsInfinity(B.Y) || float.IsNaN(B.Y) ||
				float.IsInfinity(B.Z) || float.IsNaN(B.Z))
				B = A;
			if (float.IsInfinity(C.X) || float.IsNaN(C.X) ||
					float.IsInfinity(C.Y) || float.IsNaN(C.Y) ||
					float.IsInfinity(C.Z) || float.IsNaN(C.Z))
				C = A;
			return new Triangle(A, B, C);
		}

		public OBJ ToOBJ()
		{
			OBJ o = new OBJ();
			int v = 0;
			foreach (var vv in Planes)
			{
				Triangle t = GetTriangle(vv);
				o.Vertices.Add(t.PointA);
				o.Vertices.Add(t.PointB);
				o.Vertices.Add(t.PointC);
				var f = new OBJ.OBJFace();
				f.Material = vv.CollisionType.ToString("X");
				f.VertexIndieces.Add(v);
				f.VertexIndieces.Add(v + 1);
				f.VertexIndieces.Add(v + 2);
				o.Faces.Add(f);
				v += 3;
			}
			return o;
		}

		public static Color GetColor(ushort ID)
		{
			int ShadowEffect = (ID >> 0) & 0x7;
			int Intensity = (ID >> 3) & 0x3;
			int BasicEffect = (ID >> 5) & 0x7;


			int CollisionType = (ID >> 8) & 31;
			int CollisionEffect = (ID >> 13) & 0x7;

			switch (CollisionType)
			{
				case 0:
					{
						switch (BasicEffect)
						{
							case 0: return Color.Gray;
							case 1: return Color.PaleGoldenrod;
							case 2: return Color.Gray;
							//case 3: return Color.LightBlue;
							case 4: return ColorTranslator.FromHtml("#964B00");
							case 5: return Color.Snow;
							case 6: return Color.LightGray;
							case 7: return Color.Gray;
							default: break;
						}
						break;
					}
				case 1:
					{
						switch (BasicEffect)
						{
							case 0:
								{
									switch (CollisionEffect)
									{
										case 0: return Color.LightSkyBlue;
										case 4: return Color.PaleGoldenrod;
									}
									break;
								}
							case 1: return Color.PaleGoldenrod;
							case 3: return Color.SkyBlue;
							case 4: return Color.WhiteSmoke;
						}
						break;
					}
				case 2:
					{
						switch (BasicEffect)
						{
							case 2: return Color.GhostWhite;
							case 3: return ColorTranslator.FromHtml("#6F4E37");
						}
						break;
					}
				case 3:
					{
						switch (BasicEffect)
						{
							case 0: return Color.Gold;
							case 1: return ColorTranslator.FromHtml("#6F4E37");
							case 2: return Color.Green;
							case 3: return Color.Gold;
							case 4: return Color.PapayaWhip;
							case 5: return Color.Snow;
							default: break;
						}
						break;
					}
				case 5:
					{
						switch (BasicEffect)
						{
							case 3: return Color.Gold;
							case 4: return Color.GreenYellow;
						}
						break;
					}
				case 6:
					{
						switch (BasicEffect)
						{
							case 1: return ColorTranslator.FromHtml("#6F4E37");
						}
						break;
					}
				case 7:
					{
						switch (BasicEffect)
						{
							case 0: return Color.Red;
						}
						break;
					}
				case 8:
					{
						switch (BasicEffect)
						{
							case 0: return Color.Orange;
							case 1: return ColorTranslator.FromHtml("#B06500");
							case 2: return Color.Silver;
							case 3: return ColorTranslator.FromHtml("#964B00");
							case 7: return Color.DeepSkyBlue;
						}
						break;
					}
				case 9:
					{
						switch (BasicEffect)
						{
						}
						break;
					}
				case 0xA:
					{
						switch (BasicEffect)
						{
							case 0: return Color.FromArgb(255, Color.Black);
							case 1: return Color.DarkGoldenrod;
							case 3: return Color.FromArgb(255, Color.Snow);
							case 4: return Color.DarkSlateBlue;
						}
						break;
					}
				case 0xB:
					{
						switch (BasicEffect)
						{
							case 0: return Color.FromArgb(0, Color.Black);
							case 1: return Color.DarkSlateBlue;
							case 2: return Color.OrangeRed;
						}
						break;
					}
				case 0xC:
					{
						switch (BasicEffect)
						{
							case 1: return Color.IndianRed;
							case 2: return Color.IndianRed;
							case 3: return Color.IndianRed;
						}
						break;
					}
				case 0xD:
					{
						switch (BasicEffect)
						{
							//case 4: return Color.FromArgb(0, 32, 0);
						}
						break;
					}
				case 0x0F:
					{
						return Color.FromArgb(255, 0, 127);
					}
				case 0x10:
					{
						switch (BasicEffect)
						{

						}
						break;
					}
				case 0x11:
					{
						return Color.SkyBlue;
					}
				case 0x12:
					{
						switch (BasicEffect)
						{
							case 0: return Color.Red;
						}
						break;
					}
				case 0x13:
					{
						switch (BasicEffect)
						{
							case 0: return Color.DarkRed;
							case 1: return Color.DarkRed;
						}
						break;
					}
				case 0x14:
					{
						switch (BasicEffect)
						{
							case 3: return Color.ForestGreen;
						}
						break;
					}
				case 0x15:
					{
						switch (BasicEffect)
						{
							case 4: return Color.DarkGreen;
						}
						break;
					}
				case 0x16:
					{
						switch (BasicEffect)
						{

						}
						break;
					}
				default: break;
			}

			return Color.Purple;
		}

		public class KCLIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return "Mario Kart DS";
			}

			public override string GetFileDescription()
			{
				return "Mario Kart DS Collision (KCL)";
			}

			public override string GetFileFilter()
			{
				return "Mario Kart DS Collision (*.kcl)|*.kcl";
			}

			public override Bitmap GetIcon()
			{
				return Resource.leaf_brown_pencil;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 0x3C &&
					File.Data[0] == 0x3C && File.Data[1] == 0 && File.Data[2] == 0 && File.Data[3] == 0 &&
					File.Data[0x10] == 0 && File.Data[0x11] == 0xE0 && File.Data[0x12] == 1 && File.Data[0x13] == 0 &&
					File.Data[0x38] == 0 && File.Data[0x39] == 0x90 && File.Data[0x3A] == 1 && File.Data[0x3B] == 0
					) return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
