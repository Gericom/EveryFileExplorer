using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.IO;
using System.IO;
using CommonFiles;
using System.Drawing;
using LibEveryFileExplorer.Collections;
using LibEveryFileExplorer._3D;

namespace MarioKart.MK64
{
	public class LevelScript
	{
		public enum LevelScriptCommand : byte
		{
			LOAD_TEX_32x32 = 0x20,
			LOAD_TEX_64x32 = 0x21,
			LOAD_TEX_32x64 = 0x22,
			LOAD_TEX_32x32_ALPHA = 0x23,

			DRAW_TRIANGLE = 0x29,

			DRAW_TRIANGLE_2 = 0x58,

			END = 0xFF
		}

		//0x1A - 2 params
		//0x20 - 3 params
		//0x26 - 0 params
		//0x2A - 0 params
		//0x2B - 2 params
		//0x3A - 2 params
		//0x3E - 2 params
		//0x58 - 4 params

		public static MK64Model GetModel(MK64.Track t)
		{
			MK64Model m = new MK64Model();

			int[] cmdusage = new int[256];

			//maps texids to textable entries
			Dictionary<int, int> TexTableMapper = new Dictionary<int, int>();
			int id = 0;
			int i = 0;
			foreach (var v in t.TexTable)
			{
				TexTableMapper.Add(id, i);
				id += (int)v.DecompressedSize / 2048;
				i++;
			}

			bool enabled = true;

			byte MatFlagS = 0;
			byte MatFlagT = 0;
			ushort VtxBase = 0;

			int MaterialId = -1;

			int offs = 0;
			while (offs + 1 < t.LevelScript.Length)
			{
				byte cmd = t.LevelScript[offs++];
				cmdusage[cmd]++;
				switch (cmd)
				{
					case 0x15: break;
					case 0x17: break;
					case 0x18: break;
					case 0x1A: //seems to be texture flags
					case 0x1B:
					case 0x1C:
					case 0x1D:
						MatFlagS = t.LevelScript[offs];
						MatFlagT = t.LevelScript[offs + 1];
						offs += 2;
						break;
					case (byte)LevelScriptCommand.LOAD_TEX_32x32:
					case (byte)LevelScriptCommand.LOAD_TEX_64x32:
						{
							MaterialId = IOUtil.ReadU16LE(t.LevelScript, offs);
							byte Unknown = t.LevelScript[offs + 2];
							offs += 3;
							if (!m.Materials.ContainsKey(MaterialId))
							{
								m.Materials.Add(MaterialId, new MK64Model.MK64Material()
								{
									Texture = t.TexTable[TexTableMapper[MaterialId]].ToBitmap(false, false)
								});
							}
							break;
						}
					case (byte)LevelScriptCommand.LOAD_TEX_32x64:
						{
							MaterialId = IOUtil.ReadU16LE(t.LevelScript, offs);
							byte Unknown = t.LevelScript[offs + 2];
							offs += 3;
							if (!m.Materials.ContainsKey(MaterialId))
							{
								m.Materials.Add(MaterialId, new MK64Model.MK64Material()
								{
									Texture = t.TexTable[TexTableMapper[MaterialId]].ToBitmap(true, false)
								});
							}
							break;
						}
					case (byte)LevelScriptCommand.LOAD_TEX_32x32_ALPHA:
						{
							MaterialId = IOUtil.ReadU16LE(t.LevelScript, offs);
							byte Unknown = t.LevelScript[offs + 2];
							offs += 3;
							if (!m.Materials.ContainsKey(MaterialId))
							{
								m.Materials.Add(MaterialId, new MK64Model.MK64Material()
								{
									Texture = t.TexTable[TexTableMapper[MaterialId]].ToBitmap(false, true)
								});
							}
							break;
						}
					case 0x26:
						break;
					case 0x27:
						break;
					case (byte)LevelScriptCommand.DRAW_TRIANGLE:
						{
							ushort a = IOUtil.ReadU16LE(t.LevelScript, offs);
							if (enabled)
								m.AddTriangle(t, m, a, VtxBase, MaterialId);
							offs += 2;
							break;
						}
					case 0x2A:
						break;
					case 0x2B:
						{
							ushort unk = IOUtil.ReadU16LE(t.LevelScript, offs);
							offs += 2;
							break;
						}
					case 0x2C:
						{
							ushort unk = IOUtil.ReadU16LE(t.LevelScript, offs);
							offs += 2;
							break;
						}
					case 0x35:
					case 0x36:
					case 0x37:
					case 0x38:
					case 0x39:
					case 0x3A:
					case 0x3B:
					case 0x3C:
					case 0x3D:
					case 0x3E:
					case 0x3F:
					case 0x40:
					case 0x41:
					case 0x42:
					case 0x43:
					case 0x44:
					case 0x45:
					case 0x46:
					case 0x47:
					case 0x48:
					case 0x49:
					case 0x4A:
					case 0x4B:
					case 0x4C:
					case 0x4D:
					case 0x4E:
					case 0x4F:
					case 0x50:
					case 0x51:
					case 0x52:
						{
							VtxBase = IOUtil.ReadU16LE(t.LevelScript, offs);
							offs += 2;
							break;
						}
					case (byte)LevelScriptCommand.DRAW_TRIANGLE_2:
						{
							ushort a = IOUtil.ReadU16LE(t.LevelScript, offs);
							if (enabled)
								m.AddTriangle(t, m, a, VtxBase, MaterialId);
							ushort b = IOUtil.ReadU16LE(t.LevelScript, offs + 2);
							if (enabled)
								m.AddTriangle(t, m, b, VtxBase, MaterialId);
							offs += 4;
							break;
						}
					case (byte)LevelScriptCommand.END:
						goto end;
					default:
						break;
				}
			}
		end:
			return m;
		}

		public class MK64Model
		{
			public Dictionary<int, MK64Material> Materials = new Dictionary<int, MK64Material>();
			public class MK64Material
			{
				public enum MK64TexWrap
				{
					Clamp,
					Repeat,
					Mirror
				}

				public Bitmap Texture;
				public MK64TexWrap WrapS;
				public MK64TexWrap WrapT;
			}
			public List<MK64Triangle> Triangles = new List<MK64Triangle>();
			public class MK64Triangle
			{
				public List<Vector3> Vertices = new List<Vector3>();
				public List<Vector2> TexCoords = new List<Vector2>();
				public List<Color> VertexColors = new List<Color>();

				public int MaterialId = -1;
			}

			public void AddTriangle(MK64.Track Track, MK64Model Model, ushort TriDef, int VtxBase, int MaterialId)
			{
				MK64Triangle t = new MK64Triangle();
				int vtx1 = VtxBase + (TriDef & 0x1F);
				int vtx2 = VtxBase + ((TriDef >> 5) & 0x1F);
				int vtx3 = VtxBase + ((TriDef >> 10) & 0x1F);
				t.Vertices.Add(Track.VertexData[vtx1].Position);
				t.Vertices.Add(Track.VertexData[vtx2].Position);
				t.Vertices.Add(Track.VertexData[vtx3].Position);
				if (MaterialId != -1 && Model.Materials.ContainsKey(MaterialId) && Model.Materials[MaterialId].Texture != null)
				{
					t.TexCoords.Add(new Vector2(Track.VertexData[vtx1].TexCoord.X / ((float)Model.Materials[MaterialId].Texture.Width) * 0.75f, -Track.VertexData[vtx1].TexCoord.Y / ((float)Model.Materials[MaterialId].Texture.Height) * 0.75f));
					t.TexCoords.Add(new Vector2(Track.VertexData[vtx2].TexCoord.X / ((float)Model.Materials[MaterialId].Texture.Width) * 0.75f, -Track.VertexData[vtx2].TexCoord.Y / ((float)Model.Materials[MaterialId].Texture.Height) * 0.75f));
					t.TexCoords.Add(new Vector2(Track.VertexData[vtx3].TexCoord.X / ((float)Model.Materials[MaterialId].Texture.Width) * 0.75f, -Track.VertexData[vtx3].TexCoord.Y / ((float)Model.Materials[MaterialId].Texture.Height) * 0.75f));
				}
				else
				{
					t.TexCoords.Add(new Vector2(Track.VertexData[vtx1].TexCoord.X / 32f * 0.75f, -Track.VertexData[vtx1].TexCoord.Y / 32f * 0.75f));
					t.TexCoords.Add(new Vector2(Track.VertexData[vtx2].TexCoord.X / 32f * 0.75f, -Track.VertexData[vtx2].TexCoord.Y / 32f * 0.75f));
					t.TexCoords.Add(new Vector2(Track.VertexData[vtx3].TexCoord.X / 32f * 0.75f, -Track.VertexData[vtx3].TexCoord.Y / 32f * 0.75f));
				}
				t.VertexColors.Add(Track.VertexData[vtx1].VertexColor);
				t.VertexColors.Add(Track.VertexData[vtx2].VertexColor);
				t.VertexColors.Add(Track.VertexData[vtx3].VertexColor);
				t.MaterialId = MaterialId;
				Triangles.Add(t);
			}

			public OBJ ToOBJ()
			{
				OBJ o = new OBJ();
				foreach (var v in Triangles)
				{
					int baseidx = o.Vertices.Count;
					o.Vertices.AddRange(v.Vertices);
					o.TexCoords.AddRange(v.TexCoords);
					for (int i = 0; i < v.Vertices.Count; i += 3)
					{
						OBJ.OBJFace f = new OBJ.OBJFace();
						f.Material = "M" + v.MaterialId + "M";
						f.VertexIndieces.Add(baseidx + i);
						f.VertexIndieces.Add(baseidx + i + 1);
						f.VertexIndieces.Add(baseidx + i + 2);
						f.TexCoordIndieces.Add(baseidx + i);
						f.TexCoordIndieces.Add(baseidx + i + 1);
						f.TexCoordIndieces.Add(baseidx + i + 2);
						o.Faces.Add(f);
					}
				}
				return o;
			}

			public MTL ToMTL()
			{
				MTL m = new MTL();
				foreach (var v in Materials)
				{
					MTL.MTLMaterial mm = new MTL.MTLMaterial("M" + v.Key + "M");
					mm.DiffuseColor = Color.White;
					mm.DiffuseMapPath = "Tex/" + v.Key + ".png";
					m.Materials.Add(mm);
				}
				return m;
			}
		}
	}
}
