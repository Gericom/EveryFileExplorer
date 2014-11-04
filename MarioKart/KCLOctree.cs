using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer._3D;
using LibEveryFileExplorer.Collections;
using CommonFiles;

namespace MarioKart
{
	public class KCLOctree
	{
		public KCLOctree() { }
		public KCLOctree(EndianBinaryReader er, int NrNodes)
		{
			long baseoffset = er.BaseStream.Position;
			RootNodes = new KCLOctreeNode[NrNodes];
			for (int i = 0; i < NrNodes; i++)
			{
				RootNodes[i] = new KCLOctreeNode(er, baseoffset);
			}
		}

		public void Write(EndianBinaryWriter er)
		{
			long basepos = er.BaseStream.Position;
			Queue<uint> NodeBaseOffsets = new Queue<uint>();
			Queue<KCLOctreeNode> Nodes = new Queue<KCLOctreeNode>();
			foreach (var v in RootNodes)
			{
				NodeBaseOffsets.Enqueue(0);
				Nodes.Enqueue(v);
			}
			uint offs = (uint)(RootNodes.Length * 4);
			while (Nodes.Count > 0)
			{
				KCLOctreeNode n = Nodes.Dequeue();
				if (n.IsLeaf)
				{
					NodeBaseOffsets.Dequeue();
					er.Write((uint)0);
				}
				else
				{
					n.DataOffset = offs - NodeBaseOffsets.Dequeue();
					er.Write(n.DataOffset);
					foreach (var v in n.SubNodes)
					{
						NodeBaseOffsets.Enqueue(offs);
						Nodes.Enqueue(v);
					}
					offs += 8 * 4;
				}
			}
			foreach (var v in RootNodes)
			{
				NodeBaseOffsets.Enqueue(0);
				Nodes.Enqueue(v);
			}
			long leafstartpos = er.BaseStream.Position;
			uint relleafstartpos = offs;
			er.BaseStream.Position = basepos;
			offs = (uint)(RootNodes.Length * 4);
			while (Nodes.Count > 0)
			{
				KCLOctreeNode n = Nodes.Dequeue();
				if (n.IsLeaf)
				{
					er.Write((uint)(0x80000000 | (relleafstartpos - NodeBaseOffsets.Dequeue() - 2)));
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = leafstartpos;
					foreach (var v in n.Triangles)
					{
						er.Write((ushort)(v + 1));
					}
					er.Write((ushort)0);
					relleafstartpos += (uint)(n.Triangles.Length * 2) + 2;
					leafstartpos = er.BaseStream.Position;
					er.BaseStream.Position = curpos;
				}
				else
				{
					er.BaseStream.Position += 4;
					NodeBaseOffsets.Dequeue();
					foreach (var v in n.SubNodes)
					{
						NodeBaseOffsets.Enqueue(offs);
						Nodes.Enqueue(v);
					}
					offs += 8 * 4;
				}
			}
		}

		public KCLOctreeNode[] RootNodes;

		public class KCLOctreeNode
		{
			public KCLOctreeNode() { }
			public KCLOctreeNode(EndianBinaryReader er, long BaseOffset)
			{
				DataOffset = er.ReadUInt32();
				IsLeaf = (DataOffset >> 31) == 1;
				DataOffset &= 0x7FFFFFFF;
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = BaseOffset + DataOffset;
				if (IsLeaf)
				{
					er.BaseStream.Position += 2;//Skip starting zero
					List<ushort> tris = new List<ushort>();
					while (true)
					{
						ushort v = er.ReadUInt16();
						if (v == 0) break;
						tris.Add((ushort)(v - 1));
					}
					Triangles = tris.ToArray();
				}
				else
				{
					SubNodes = new KCLOctreeNode[8];
					for (int i = 0; i < 8; i++)
					{
						SubNodes[i] = new KCLOctreeNode(er, BaseOffset + DataOffset);
					}
				}
				er.BaseStream.Position = curpos;
			}

			public UInt32 DataOffset;
			public Boolean IsLeaf;

			public KCLOctreeNode[] SubNodes;
			public ushort[] Triangles;

			/*public int NrUniqueTris
			{
				get { return GetNrUniqueTris(new List<ushort>()); }
			}

			private int GetNrUniqueTris(List<ushort> tris)
			{
				if (IsLeaf)
				{
					int nr = 0;
					foreach (ushort i in Triangles)
					{
						if (!tris.Contains(i)) { tris.Add(i); nr++; }
					}
					return nr;
				}
				int total = 0;
				foreach (var v in SubNodes)
				{
					total += v.GetNrUniqueTris(tris);
				}
				return total;
			}

			public int GetLeafMostTris()
			{
				if (IsLeaf) return Triangles.Length;
				int maxnum = 0;
				foreach (var v in SubNodes)
				{
					int num = v.GetLeafMostTris();
					if (num > maxnum) maxnum = num;
				}
				return maxnum;
			}

			public int GetDepth()
			{
				if (IsLeaf) return 0;
				int curdepth = 0;
				foreach (var v in SubNodes)
				{
					int num = v.GetDepth() + 1;
					if (num > curdepth) curdepth = num;
				}
				return curdepth;
			}*/
			public static KCLOctreeNode Generate(Dictionary<ushort, Triangle> Triangles, Vector3 Position, float BoxSize, int MaxTris, int MinSize)
			{
				KCLOctreeNode n = new KCLOctreeNode();
				//Pump this box a little up, to prevent glitches
				Vector3 midpos = Position + new Vector3(BoxSize / 2f, BoxSize / 2f, BoxSize / 2f);
				float newsize = BoxSize + 50;// 60;
				Vector3 newpos = midpos - new Vector3(newsize / 2f, newsize / 2f, newsize / 2f);
				Dictionary<ushort, Triangle> t = new Dictionary<ushort, Triangle>();
				foreach (var v in Triangles)
				{
					if (tricube_overlap(v.Value, newpos, newsize)) t.Add(v.Key, v.Value);
				}
				if (BoxSize != MinSize && t.Count > MaxTris)
				{
					n.IsLeaf = false;
					float childsize = BoxSize / 2f;
					n.SubNodes = new KCLOctreeNode[8];
					int i = 0;
					for (int z = 0; z < 2; z++)
					{
						for (int y = 0; y < 2; y++)
						{
							for (int x = 0; x < 2; x++)
							{
								Vector3 pos = Position + childsize * new Vector3(x, y, z);
								n.SubNodes[i] = KCLOctreeNode.Generate(t, pos, childsize, MaxTris, MinSize);
								i++;
							}
						}
					}
				}
				else
				{
					n.IsLeaf = true;
					n.Triangles = t.Keys.ToArray();
				}
				return n;
			}

			private static bool axis_test(float a1, float a2, float b1, float b2, float c1, float c2, float half)
			{
				float p = a1 * b1 + a2 * b2;
				float q = a1 * c1 + a2 * c2;
				float r = half * (Math.Abs(a1) + Math.Abs(a2));
				return Math.Min(p, q) > r || Math.Max(p, q) < -r;
			}
			//Based on this algorithm: http://jgt.akpeters.com/papers/AkenineMoller01/tribox.html
			private static bool tricube_overlap(Triangle t, Vector3 Position, float BoxSize)
			{
				float half = BoxSize / 2f;
				//Position is the min pos, so add half the box size
				Position += new Vector3(half, half, half);
				Vector3 v0 = t.PointA - Position;
				Vector3 v1 = t.PointB - Position;
				Vector3 v2 = t.PointC - Position;

				if (Math.Min(Math.Min(v0.X, v1.X), v2.X) > half || Math.Max(Math.Max(v0.X, v1.X), v2.X) < -half) return false;
				if (Math.Min(Math.Min(v0.Y, v1.Y), v2.Y) > half || Math.Max(Math.Max(v0.Y, v1.Y), v2.Y) < -half) return false;
				if (Math.Min(Math.Min(v0.Z, v1.Z), v2.Z) > half || Math.Max(Math.Max(v0.Z, v1.Z), v2.Z) < -half) return false;

				float d = t.Normal.Dot(v0);
				float r = half * (Math.Abs(t.Normal.X) + Math.Abs(t.Normal.Y) + Math.Abs(t.Normal.Z));
				if (d > r || d < -r) return false;

				Vector3 e = v1 - v0;
				if (axis_test(e.Z, -e.Y, v0.Y, v0.Z, v2.Y, v2.Z, half)) return false;
				if (axis_test(-e.Z, e.X, v0.X, v0.Z, v2.X, v2.Z, half)) return false;
				if (axis_test(e.Y, -e.X, v1.X, v1.Y, v2.X, v2.Y, half)) return false;

				e = v2 - v1;
				if (axis_test(e.Z, -e.Y, v0.Y, v0.Z, v2.Y, v2.Z, half)) return false;
				if (axis_test(-e.Z, e.X, v0.X, v0.Z, v2.X, v2.Z, half)) return false;
				if (axis_test(e.Y, -e.X, v0.X, v0.Y, v1.X, v1.Y, half)) return false;

				e = v0 - v2;
				if (axis_test(e.Z, -e.Y, v0.Y, v0.Z, v1.Y, v1.Z, half)) return false;
				if (axis_test(-e.Z, e.X, v0.X, v0.Z, v1.X, v1.Z, half)) return false;
				if (axis_test(e.Y, -e.X, v1.X, v1.Y, v2.X, v2.Y, half)) return false;
				return true;
			}

			/*private Triangle[] GetTrianglesInside(Triangle[] Triangles)
			{
				if (IsLeaf)
				{
					List<Triangle> t = new List<Triangle>();
					foreach (ushort i in this.Triangles) t.Add(Triangles[i]);
					return t.ToArray();
				}
				else
				{
					List<Triangle> t = new List<Triangle>();
					foreach (var v in SubNodes)
					{
						Triangle[] tt = v.GetTrianglesInside(Triangles);
						foreach (Triangle qq in tt) if (!t.Contains(qq)) t.Add(qq);
					}
					return t.ToArray();
				}
			}*/

			/*public OBJ ExportCube(Triangle[] Triangles, Vector3 Position, float BoxSize)
			{
				OBJ o = new OBJ();
				Triangle[] tt = GetTrianglesInside(Triangles);
				int v = 0;
				foreach (Triangle t in tt)
				{
					o.Vertices.Add(t.PointA);
					o.Vertices.Add(t.PointB);
					o.Vertices.Add(t.PointC);
					var f = new OBJ.OBJFace();
					f.VertexIndieces.Add(v);
					f.VertexIndieces.Add(v + 1);
					f.VertexIndieces.Add(v + 2);
					o.Faces.Add(f);
					v += 3;
				}
				for (int z = 0; z < 2; z++)
				{
					for (int y = 0; y < 2; y++)
					{
						for (int x = 0; x < 2; x++)
						{
							o.Vertices.Add(Position + BoxSize * new Vector3(x, y, z));
						}
					}
				}
				//back
				var ff = new OBJ.OBJFace();
				ff.VertexIndieces.Add(v + 0);
				ff.VertexIndieces.Add(v + 1);
				ff.VertexIndieces.Add(v + 3);
				ff.VertexIndieces.Add(v + 2);
				o.Faces.Add(ff);
				//front
				ff = new OBJ.OBJFace();
				ff.VertexIndieces.Add(v + 4);
				ff.VertexIndieces.Add(v + 5);
				ff.VertexIndieces.Add(v + 7);
				ff.VertexIndieces.Add(v + 6);
				o.Faces.Add(ff);
				//top
				ff = new OBJ.OBJFace();
				ff.VertexIndieces.Add(v + 2);
				ff.VertexIndieces.Add(v + 3);
				ff.VertexIndieces.Add(v + 7);
				ff.VertexIndieces.Add(v + 6);
				o.Faces.Add(ff);
				//bottom
				ff = new OBJ.OBJFace();
				ff.VertexIndieces.Add(v + 0);
				ff.VertexIndieces.Add(v + 1);
				ff.VertexIndieces.Add(v + 5);
				ff.VertexIndieces.Add(v + 4);
				o.Faces.Add(ff);
				//left
				ff = new OBJ.OBJFace();
				ff.VertexIndieces.Add(v + 0);
				ff.VertexIndieces.Add(v + 2);
				ff.VertexIndieces.Add(v + 6);
				ff.VertexIndieces.Add(v + 4);
				o.Faces.Add(ff);
				//right
				ff = new OBJ.OBJFace();
				ff.VertexIndieces.Add(v + 1);
				ff.VertexIndieces.Add(v + 3);
				ff.VertexIndieces.Add(v + 7);
				ff.VertexIndieces.Add(v + 5);
				o.Faces.Add(ff);
				return o;
			}*/
		}

		public static KCLOctree FromTriangles(Triangle[] Triangles, KCLHeader Header, int MaxRootSize = 2048, int MinRootSize = 128, int MinCubeSize = 32, int MaxNrTris = 10)//35)
		{
			Header.Unknown1 = 30;
			Header.Unknown2 = 25;
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			Dictionary<ushort, Triangle> tt = new Dictionary<ushort, Triangle>();
			ushort index = 0;
			foreach (var t in Triangles)
			{
				if (t.PointA.X < min.X) min.X = t.PointA.X;
				if (t.PointA.Y < min.Y) min.Y = t.PointA.Y;
				if (t.PointA.Z < min.Z) min.Z = t.PointA.Z;
				if (t.PointA.X > max.X) max.X = t.PointA.X;
				if (t.PointA.Y > max.Y) max.Y = t.PointA.Y;
				if (t.PointA.Z > max.Z) max.Z = t.PointA.Z;

				if (t.PointB.X < min.X) min.X = t.PointB.X;
				if (t.PointB.Y < min.Y) min.Y = t.PointB.Y;
				if (t.PointB.Z < min.Z) min.Z = t.PointB.Z;
				if (t.PointB.X > max.X) max.X = t.PointB.X;
				if (t.PointB.Y > max.Y) max.Y = t.PointB.Y;
				if (t.PointB.Z > max.Z) max.Z = t.PointB.Z;

				if (t.PointC.X < min.X) min.X = t.PointC.X;
				if (t.PointC.Y < min.Y) min.Y = t.PointC.Y;
				if (t.PointC.Z < min.Z) min.Z = t.PointC.Z;
				if (t.PointC.X > max.X) max.X = t.PointC.X;
				if (t.PointC.Y > max.Y) max.Y = t.PointC.Y;
				if (t.PointC.Z > max.Z) max.Z = t.PointC.Z;
				tt.Add(index, t);
				index++;
			}
			//in real mkds, 25 is subtracted from the min pos
			min -= new Vector3(25, 25, 25);
			//TODO: after that, from some of the components (may be more than one) 30 is subtracted aswell => How do I know from which ones I have to do that?

			//Assume the same is done for max:
			max += new Vector3(25, 25, 25);
			//TODO: +30
			Header.OctreeOrigin = min;
			Vector3 size = max - min;
			float mincomp = Math.Min(Math.Min(size.X, size.Y), size.Z);
			int CoordShift = Get2Power(mincomp);
			if (CoordShift > Get2Power(MaxRootSize)) CoordShift = Get2Power(MaxRootSize);
			else if (CoordShift < Get2Power(MinRootSize)) CoordShift = Get2Power(MinRootSize);
			Header.CoordShift = (uint)CoordShift;
			int cubesize = 1 << CoordShift;
			int NrX = (1 << Get2Power(size.X)) / cubesize;
			int NrY = (1 << Get2Power(size.Y)) / cubesize;
			int NrZ = (1 << Get2Power(size.Z)) / cubesize;
			Header.YShift = (uint)(Get2Power(size.X) - CoordShift);
			Header.ZShift = (uint)(Get2Power(size.X) - CoordShift + Get2Power(size.Y) - CoordShift);
			Header.XMask = 0xFFFFFFFF << Get2Power(size.X);
			Header.YMask = 0xFFFFFFFF << Get2Power(size.Y);
			Header.ZMask = 0xFFFFFFFF << Get2Power(size.Z);

			KCLOctree k = new KCLOctree();
			k.RootNodes = new KCLOctreeNode[NrX * NrY * NrZ];
			int i = 0;
			for (int z = 0; z < NrZ; z++)
			{
				for (int y = 0; y < NrY; y++)
				{
					for (int x = 0; x < NrX; x++)
					{
						Vector3 pos = min + ((float)cubesize) * new Vector3(x, y, z);
						k.RootNodes[i] = KCLOctreeNode.Generate(tt, pos, cubesize, MaxNrTris, MinCubeSize);
						i++;
					}
				}
			}
			return k;
		}

		private static int Get2Power(float Value)
		{
			return (int)Math.Ceiling(Math.Log(Value, 2));
		}
	}
}