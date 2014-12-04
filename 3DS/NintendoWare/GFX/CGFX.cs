using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using System.Windows.Forms;
using LibEveryFileExplorer._3D;
using CommonFiles;
using Tao.OpenGl;
using LibEveryFileExplorer.Collections;

namespace _3DS.NintendoWare.GFX
{
	public class CGFX : FileFormat<CGFX.CGFXIdentifier>, IConvertable, IViewable, IWriteable
	{
		public CGFX(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new CGFXHeader(er);
				this.Data = new DATA(er);
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new UI.CGFXViewer(this);
		}

		public string GetConversionFileFilters()
		{
			return "COLLADA DAE File (*.dae)|*.dae|Wavefront OBJ File (*.obj)|*.obj";
		}

		public bool Convert(int FilterIndex, String Path)
		{
			switch (FilterIndex)
			{
				case 0:
					{
						DAE o = ToDAE(0);
						File.Create(Path).Close();
						File.WriteAllBytes(Path, o.Write());
						Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path) + "\\Tex");
						foreach (var v in Data.Textures)
						{
							if (!(v is ImageTextureCtr)) continue;
							((ImageTextureCtr)v).GetBitmap().Save(System.IO.Path.GetDirectoryName(Path) + "\\Tex\\" + v.Name + ".png");
						}
						return true;
					}
				case 1:
					{
						if (Data.Models.Length == 0) return false;
						OBJ o = ToOBJ(0);
						o.MTLPath = System.IO.Path.GetFileNameWithoutExtension(Path) + ".mtl";
						MTL m = ToMTL(0);
						byte[] d = o.Write();
						byte[] d2 = m.Write();
						File.Create(Path).Close();
						File.WriteAllBytes(Path, d);
						File.Create(System.IO.Path.ChangeExtension(Path, "mtl")).Close();
						File.WriteAllBytes(System.IO.Path.ChangeExtension(Path, "mtl"), d2);
						Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path) + "\\Tex");
						foreach (var v in Data.Textures)
						{
							//if (v.NrLevels > 2) v.GetBitmap(2).Save(System.IO.Path.GetDirectoryName(Path) + "\\Tex\\" + v.Name + ".png");
							//else if (v.NrLevels > 1) v.GetBitmap(1).Save(System.IO.Path.GetDirectoryName(Path) + "\\Tex\\" + v.Name + ".png");
							//else v.GetBitmap(0).Save(System.IO.Path.GetDirectoryName(Path) + "\\Tex\\" + v.Name + ".png");
							if (!(v is ImageTextureCtr)) continue;
							((ImageTextureCtr)v).GetBitmap().Save(System.IO.Path.GetDirectoryName(Path) + "\\Tex\\" + v.Name + ".png");
						}
						return true;
					}
				default:
					return false;
			}
		}

		public string GetSaveDefaultFileFilter()
		{
			return "CTR Graphics Resource (*.bcres)|*.bcres";
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			Header.NrBlocks = 1;
			Header.Write(er);
			CGFXWriterContext c = new CGFXWriterContext();
			Data.Write(er, c);
			if (c.DoWriteIMAGBlock())
			{
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = 0x10;
				er.Write((uint)2);
				er.BaseStream.Position = curpos;
				c.WriteIMAGBlock(er);
			}
			long curpos2 = er.BaseStream.Position;
			er.BaseStream.Position = 0xC;
			er.Write((uint)(curpos2));
			er.BaseStream.Position = curpos2;

			byte[] result = m.ToArray();
			er.Close();
			return result;
		}

		public CGFXHeader Header;
		public class CGFXHeader
		{
			public CGFXHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "CGFX") throw new SignatureNotCorrectException(Signature, "CGFX", er.BaseStream.Position - 4);
				Endianness = er.ReadUInt16();
				HeaderSize = er.ReadUInt16();
				Version = er.ReadUInt32();
				FileSize = er.ReadUInt32();
				NrBlocks = er.ReadUInt32();
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(Signature, Encoding.ASCII, false);
				er.Write(Endianness);
				er.Write(HeaderSize);
				er.Write(Version);
				er.Write((uint)0);
				er.Write(NrBlocks);
			}
			public String Signature;
			public UInt16 Endianness;
			public UInt16 HeaderSize;
			public UInt32 Version;
			public UInt32 FileSize;
			public UInt32 NrBlocks;
		}
		public DATA Data;
		public class DATA
		{
			public DATA(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "DATA") throw new SignatureNotCorrectException(Signature, "DATA", er.BaseStream.Position - 4);
				SectionSize = er.ReadUInt32();
				DictionaryEntries = new DictionaryInfo[16];
				for (int i = 0; i < 16; i++)
				{
					DictionaryEntries[i] = new DictionaryInfo(er);
				}
				Dictionaries = new DICT[16];
				for (int i = 0; i < 16; i++)
				{
					if (i == 15 && DictionaryEntries[i].NrItems == 0x54434944)
					{
						DictionaryEntries[i].NrItems = 0;
						DictionaryEntries[i].Offset = 0;
					}
					if (DictionaryEntries[i].Offset != 0)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = DictionaryEntries[i].Offset;
						Dictionaries[i] = new DICT(er);
						er.BaseStream.Position = curpos;
					}
					else Dictionaries[i] = null;
				}

				if (Dictionaries[0] != null)
				{
					Models = new CMDL[Dictionaries[0].Count];
					for (int i = 0; i < Dictionaries[0].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Dictionaries[0][i].DataOffset;
						Models[i] = new CMDL(er);
						er.BaseStream.Position = curpos;
					}
				}

				if (Dictionaries[1] != null)
				{
					Textures = new TXOB[Dictionaries[1].Count];
					for (int i = 0; i < Dictionaries[1].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Dictionaries[1][i].DataOffset;
						Textures[i] = TXOB.FromStream(er);//new TXOB(er);
						er.BaseStream.Position = curpos;
					}
				}
				if (Dictionaries[9] != null)
				{
					SkeletonAnimations = new CANM[Dictionaries[9].Count];
					for (int i = 0; i < Dictionaries[9].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Dictionaries[9][i].DataOffset;
						SkeletonAnimations[i] = new CANM(er);
						er.BaseStream.Position = curpos;
					}
				}
				if (Dictionaries[10] != null)
				{
					MaterialAnimations = new CANM[Dictionaries[10].Count];
					for (int i = 0; i < Dictionaries[10].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Dictionaries[10][i].DataOffset;
						MaterialAnimations[i] = new CANM(er);
						er.BaseStream.Position = curpos;
					}
				}
				if (Dictionaries[11] != null)
				{
					VisibilityAnimations = new CANM[Dictionaries[11].Count];
					for (int i = 0; i < Dictionaries[11].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = Dictionaries[11][i].DataOffset;
						VisibilityAnimations[i] = new CANM(er);
						er.BaseStream.Position = curpos;
					}
				}
			}
			public void Write(EndianBinaryWriter er, CGFXWriterContext c)
			{
				long basepos = er.BaseStream.Position;
				er.Write(Signature, Encoding.ASCII, false);
				er.Write((uint)0);
				for (int i = 0; i < 16; i++)
				{
					if (Dictionaries[i] != null)
					{
						if (i != 0 && i != 1) throw new NotImplementedException();
						er.Write((uint)Dictionaries[i].Count);
						er.Write((uint)0);//dictoffset
					}
					else
					{
						er.Write((uint)0);
						er.Write((uint)0);
					}
				}
				long[] dictoffsets = new long[16];
				for (int i = 0; i < 16; i++)
				{
					if (Dictionaries[i] != null)
					{
						dictoffsets[i] = er.BaseStream.Position;
						er.BaseStream.Position = basepos + 8 + i * 8 + 4;
						er.Write((uint)(dictoffsets[i] - (basepos + 8 + i * 8 + 4)));
						er.BaseStream.Position = dictoffsets[i];
						Dictionaries[i].Write(er, c);
					}
				}
				if (Dictionaries[0] != null)
				{
					for (int i = 0; i < Dictionaries[0].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						long bpos = er.BaseStream.Position = dictoffsets[0] + 0x1C + i * 0x10 + 0xC;
						er.Write((uint)(curpos - bpos));
						er.BaseStream.Position = curpos;
						Models[i].Write(er, c);
					}
				}
				if (Dictionaries[1] != null)
				{
					for (int i = 0; i < Dictionaries[1].Count; i++)
					{
						long curpos = er.BaseStream.Position;
						long bpos = er.BaseStream.Position = dictoffsets[1] + 0x1C + i * 0x10 + 0xC;
						er.Write((uint)(curpos - bpos));
						er.BaseStream.Position = curpos;
						Textures[i].Write(er, c);
					}
				}
				c.WriteStringTable(er);
				if (c.DoWriteIMAGBlock())
				{
					int length = c.GetIMAGBlockSize();
					while (((er.BaseStream.Position + length) % 64) != 0) er.Write((byte)0);
				}
				long curpos2 = er.BaseStream.Position;
				er.BaseStream.Position = basepos + 4;
				er.Write((uint)(curpos2 - basepos));
				er.BaseStream.Position = curpos2;
			}

			public String Signature;
			public UInt32 SectionSize;
			public DictionaryInfo[] DictionaryEntries;//x15
			public class DictionaryInfo
			{
				public DictionaryInfo(EndianBinaryReader er)
				{
					NrItems = er.ReadUInt32();
					long pos = er.BaseStream.Position;
					Offset = er.ReadUInt32();
					if (Offset != 0) Offset += (UInt32)pos;
				}
				public UInt32 NrItems;
				public UInt32 Offset;
			}
			public DICT[] Dictionaries;
			//0x0 - CMDL
			//0x1 - TXOB
			//0x9 - CANM (Skeleton Animation)
			//0xA - CANM (Material Animation)

			public CMDL[] Models;
			public TXOB[] Textures;
			public CANM[] SkeletonAnimations;
			public CANM[] MaterialAnimations;
			public CANM[] VisibilityAnimations;
		}

		public OBJ ToOBJ(int Model)
		{
			if (Data.Models.Length == 0 || Data.Models.Length <= Model) return null;
			var o = new OBJ();
			int v = 0;
			int vn = 0;
			int vt = 0;
			//int vc = 0;
			int ff = 0;
			var m = Data.Models[Model];
			//foreach (CMDL m in Models)
			{
				foreach (var vv in m.Shapes)
				{
					Polygon p = vv.GetVertexData(m);
					var mat = m.Materials[m.Meshes[ff].MaterialIndex];

					int TexCoord = -1;
					if (mat.NrActiveTextureCoordiators > 0 && mat.TextureCoordiators[0].MappingMethod == 0)// && mat.TextureCoordiators[0].SourceCoordinate == 0)
					{
						if (mat.TextureCoordiators[0].SourceCoordinate == 0) TexCoord = 0;
						else if (mat.TextureCoordiators[0].SourceCoordinate == 1) TexCoord = 1;
						else TexCoord = 2;
					}

					foreach (var q in vv.PrimitiveSets[0].Primitives[0].IndexStreams)
					{
						Vector3[] defs = q.GetFaceData();
						foreach (Vector3 d in defs)
						{
							OBJ.OBJFace f = new OBJ.OBJFace();
							f.Material = mat.Name;
							o.Vertices.Add(p.Vertex[(int)d.X]);
							o.Vertices.Add(p.Vertex[(int)d.Y]);
							o.Vertices.Add(p.Vertex[(int)d.Z]);
							f.VertexIndieces.Add(v);
							f.VertexIndieces.Add(v + 1);
							f.VertexIndieces.Add(v + 2);
							v += 3;
							if (p.Normals != null)
							{
								o.Normals.Add(p.Normals[(int)d.X]);
								o.Normals.Add(p.Normals[(int)d.Y]);
								o.Normals.Add(p.Normals[(int)d.Z]);
								f.NormalIndieces.Add(vn);
								f.NormalIndieces.Add(vn + 1);
								f.NormalIndieces.Add(vn + 2);
								vn += 3;
							}
							if (TexCoord == 0)
							{
								o.TexCoords.Add(p.TexCoords[(int)d.X] * new Matrix34(mat.TextureCoordiators[0].Matrix));
								o.TexCoords.Add(p.TexCoords[(int)d.Y] * new Matrix34(mat.TextureCoordiators[0].Matrix));
								o.TexCoords.Add(p.TexCoords[(int)d.Z] * new Matrix34(mat.TextureCoordiators[0].Matrix));
							}
							else if (TexCoord == 1)
							{
								o.TexCoords.Add(p.TexCoords2[(int)d.X] * new Matrix34(mat.TextureCoordiators[0].Matrix));
								o.TexCoords.Add(p.TexCoords2[(int)d.Y] * new Matrix34(mat.TextureCoordiators[0].Matrix));
								o.TexCoords.Add(p.TexCoords2[(int)d.Z] * new Matrix34(mat.TextureCoordiators[0].Matrix));
							}
							else if (TexCoord == 2)
							{
								o.TexCoords.Add(p.TexCoords3[(int)d.X] * new Matrix34(mat.TextureCoordiators[0].Matrix));
								o.TexCoords.Add(p.TexCoords3[(int)d.Y] * new Matrix34(mat.TextureCoordiators[0].Matrix));
								o.TexCoords.Add(p.TexCoords3[(int)d.Z] * new Matrix34(mat.TextureCoordiators[0].Matrix));
							}
							else goto cont;
							f.TexCoordIndieces.Add(vt);
							f.TexCoordIndieces.Add(vt + 1);
							f.TexCoordIndieces.Add(vt + 2);
							vt += 3;
						cont:
							/*if (p.Colors != null)
							{
								o.VertexColors.Add(p.Colors[(int)d.X]);
								o.VertexColors.Add(p.Colors[(int)d.Y]);
								o.VertexColors.Add(p.Colors[(int)d.Z]);
								f.VertexColorIndieces.Add(vc);
								f.VertexColorIndieces.Add(vc + 1);
								f.VertexColorIndieces.Add(vc + 2);
								vc += 3;
							}*/
							o.Faces.Add(f);
						}
					}
					ff++;
				}
			}
			return o;
		}

		public DAE ToDAE(int Model)
		{
			if (Data.Models.Length == 0 || Data.Models.Length <= Model) return null;
			var m = Data.Models[Model];
			var o = new DAE();
			o.Content.scene = new DAE.COLLADA._scene();
			o.Content.scene.instance_visual_scene = new DAE.InstanceWithExtra();
			o.Content.scene.instance_visual_scene.url = "#ID1";
			o.Content.library_visual_scenes = new DAE.library_visual_scenes();
			var scene = new DAE.visual_scene("ID1");
			var rootnode = new DAE.node(m.Name);
			scene.node.Add(rootnode);
			o.Content.library_visual_scenes.visual_scene.Add(scene);
			o.Content.library_geometries = new DAE.library_geometries();
			o.Content.library_materials = new DAE.library_materials();
			o.Content.library_effects = new DAE.library_effects();
			if (Data.Textures != null && Data.Textures.Length > 0) o.Content.library_images = new DAE.library_images();
			int id = 2;
			int i = 0;
			foreach (var matt in m.Materials)
			{
				var mat2 = new DAE.material();
				mat2.id = "ID" + id++;
				mat2.name = matt.Name;
				var eff = new DAE.effect();
				eff.id = "ID" + id++;
				mat2.instance_effect = new DAE.instance_effect();
				mat2.instance_effect.url = "#" + eff.id;
				eff.profile_COMMON = new DAE.profile_COMMON();
				eff.profile_COMMON.technique = new DAE.profile_COMMON._technique();
				eff.profile_COMMON.technique.sid = "COMMON";
				eff.profile_COMMON.technique.lambert = new DAE.profile_COMMON._technique._lambert();
				eff.profile_COMMON.technique.lambert.diffuse = new DAE.common_color_or_texture_type();
				if (matt.Tex0 != null && matt.Tex0.TextureObject is ReferenceTexture)
				{
					string texid = "ID" + id++;
					o.Content.library_images.image.Add(new DAE.image() { id = texid, init_from = "Tex/" + ((ReferenceTexture)matt.Tex0.TextureObject).LinkedTextureName + ".png" });

					var param1 = new DAE.common_newparam_type() { sid = "ID" + id++, choice = new DAE.fx_surface_common() { type = DAE.fx_surface_type_enum._2D } };
					((DAE.fx_surface_common)param1.choice).init_from.Add(new DAE.fx_surface_init_from_common() { content = texid });
					eff.profile_COMMON.newparam = new List<DAE.common_newparam_type>();
					eff.profile_COMMON.newparam.Add(param1);
					eff.profile_COMMON.newparam.Add(new DAE.common_newparam_type() { sid = "ID" + id++, choice = new DAE.fx_sampler2D_common() { source = param1.sid } });
					eff.profile_COMMON.technique.lambert.diffuse.texture = new DAE.common_color_or_texture_type._texture() { texture = eff.profile_COMMON.newparam[1].sid, texcoord = "UVSET0" };
				}
				else
				{
					eff.profile_COMMON.technique.lambert.diffuse.color = new DAE.common_color_or_texture_type._color();
					eff.profile_COMMON.technique.lambert.diffuse.color.content = Color.White;
				}
				o.Content.library_materials.material.Add(mat2);
				o.Content.library_effects.effect.Add(eff);
			}

			int ff = 0;
			//foreach (CMDL m in Models)
			{
				foreach (var vv in m.Shapes)
				{
					var geometry = new DAE.geometry();
					geometry.id = "ID" + id++;

					Polygon p = vv.GetVertexData(m);
					var mat = m.Materials[m.Meshes[ff].MaterialIndex];

					geometry.mesh = new DAE.mesh();
					geometry.mesh.vertices = new DAE.vertices() { id = "ID" + id++ };
					if (p.Vertex != null)
					{
						var src = new DAE.source() { id = "ID" + id++ };
						src.float_array = new DAE.source._float_array() { id = "ID" + id++, count = (uint)p.Vertex.Length * 3 };
						foreach (var v in p.Vertex)
						{
							src.float_array.content.Add(v.X);
							src.float_array.content.Add(v.Y);
							src.float_array.content.Add(v.Z);
						}
						src.technique_common = new DAE.source._technique_common();
						src.technique_common.accessor = new DAE.accessor();
						src.technique_common.accessor.count = (uint)p.Vertex.Length;
						src.technique_common.accessor.source = "#" + src.float_array.id;
						src.technique_common.accessor.stride = 3;
						src.technique_common.accessor.param.Add(new DAE.param() { name = "X", type = "float" });
						src.technique_common.accessor.param.Add(new DAE.param() { name = "Y", type = "float" });
						src.technique_common.accessor.param.Add(new DAE.param() { name = "Z", type = "float" });
						geometry.mesh.source.Add(src);
						geometry.mesh.vertices.input.Add(new DAE.InputLocal() { semantic = "POSITION", source = "#" + src.id });
					}
					if (p.Normals != null)
					{
						var src = new DAE.source() { id = "ID" + id++ };
						src.float_array = new DAE.source._float_array() { id = "ID" + id++, count = (uint)p.Normals.Length * 3 };
						foreach (var v in p.Normals)
						{
							src.float_array.content.Add(v.X);
							src.float_array.content.Add(v.Y);
							src.float_array.content.Add(v.Z);
						}
						src.technique_common = new DAE.source._technique_common();
						src.technique_common.accessor = new DAE.accessor();
						src.technique_common.accessor.count = (uint)p.Normals.Length;
						src.technique_common.accessor.source = "#" + src.float_array.id;
						src.technique_common.accessor.stride = 3;
						src.technique_common.accessor.param.Add(new DAE.param() { name = "X", type = "float" });
						src.technique_common.accessor.param.Add(new DAE.param() { name = "Y", type = "float" });
						src.technique_common.accessor.param.Add(new DAE.param() { name = "Z", type = "float" });
						geometry.mesh.source.Add(src);
						geometry.mesh.vertices.input.Add(new DAE.InputLocal() { semantic = "NORMAL", source = "#" + src.id });
					}
					DAE.source texcoordsrc = null;
					if (p.TexCoords != null)
					{
						if (mat.NrActiveTextureCoordiators > 0 && mat.TextureCoordiators[0].MappingMethod == 0)
						{
							Vector2[] texc;
							if (mat.TextureCoordiators[0].SourceCoordinate == 0)
							{
								texc = p.TexCoords.ToArray();
							}
							else if (mat.TextureCoordiators[0].SourceCoordinate == 1)
							{
								texc = p.TexCoords2.ToArray();
							}
							else
							{
								texc = p.TexCoords3.ToArray();
							}

							var src = texcoordsrc = new DAE.source() { id = "ID" + id++ };
							src.float_array = new DAE.source._float_array() { id = "ID" + id++, count = (uint)texc.Length * 2 };
							foreach (var v in texc)
							{
								Vector2 result = v * new Matrix34(mat.TextureCoordiators[0].Matrix);
								src.float_array.content.Add(result.X);
								src.float_array.content.Add(result.Y);
							}
							src.technique_common = new DAE.source._technique_common();
							src.technique_common.accessor = new DAE.accessor();
							src.technique_common.accessor.count = (uint)p.TexCoords.Length;
							src.technique_common.accessor.source = "#" + src.float_array.id;
							src.technique_common.accessor.stride = 2;
							src.technique_common.accessor.param.Add(new DAE.param() { name = "S", type = "float" });
							src.technique_common.accessor.param.Add(new DAE.param() { name = "T", type = "float" });
							geometry.mesh.source.Add(src);
						}
					}
					DAE.source colorsrc = null;
					if (p.Colors != null)
					{
						var src = colorsrc = new DAE.source() { id = "ID" + id++ };
						src.float_array = new DAE.source._float_array() { id = "ID" + id++, count = (uint)p.Colors.Length * 4 };
						foreach (var v in p.Colors)
						{
							src.float_array.content.Add(v.R / 255f);
							src.float_array.content.Add(v.G / 255f);
							src.float_array.content.Add(v.B / 255f);
							src.float_array.content.Add(v.A / 255f);
						}
						src.technique_common = new DAE.source._technique_common();
						src.technique_common.accessor = new DAE.accessor();
						src.technique_common.accessor.count = (uint)p.Colors.Length;
						src.technique_common.accessor.source = "#" + src.float_array.id;
						src.technique_common.accessor.stride = 4;
						src.technique_common.accessor.param.Add(new DAE.param() { name = "R", type = "float" });
						src.technique_common.accessor.param.Add(new DAE.param() { name = "G", type = "float" });
						src.technique_common.accessor.param.Add(new DAE.param() { name = "B", type = "float" });
						src.technique_common.accessor.param.Add(new DAE.param() { name = "A", type = "float" });
						geometry.mesh.source.Add(src);
					}

					foreach (var q in vv.PrimitiveSets[0].Primitives[0].IndexStreams)
					{
						Vector3[] defs = q.GetFaceData();

						var tri = new DAE.triangles() { count = (uint)defs.Length, material = mat.Name };
						uint offs = 0;
						tri.input.Add(new DAE.InputLocalOffset() { offset = offs++, semantic = "VERTEX", source = "#" + geometry.mesh.vertices.id });
						if (texcoordsrc != null) tri.input.Add(new DAE.InputLocalOffset() { offset = offs++, semantic = "TEXCOORD", source = "#" + texcoordsrc.id });
						if (colorsrc != null) tri.input.Add(new DAE.InputLocalOffset() { offset = offs++, semantic = "COLOR", source = "#" + colorsrc.id, set = 0 });
						tri.p.Add(new DAE.p());
						foreach (Vector3 d in defs)
						{
							tri.p[0].content.Add((ulong)d.X);
							if (texcoordsrc != null) tri.p[0].content.Add((ulong)d.X);
							if (colorsrc != null) tri.p[0].content.Add((ulong)d.X);
							tri.p[0].content.Add((ulong)d.Y);
							if (texcoordsrc != null) tri.p[0].content.Add((ulong)d.Y);
							if (colorsrc != null) tri.p[0].content.Add((ulong)d.Y);
							tri.p[0].content.Add((ulong)d.Z);
							if (texcoordsrc != null) tri.p[0].content.Add((ulong)d.Z);
							if (colorsrc != null) tri.p[0].content.Add((ulong)d.Z);
						}
						geometry.mesh.triangles.Add(tri);
					}
					o.Content.library_geometries.geometry.Add(geometry);
					var instgem = new DAE.instance_geometry() { url = "#" + geometry.id };
					instgem.bind_material = new DAE.bind_material();
					instgem.bind_material.technique_common = new DAE.bind_material._technique_common();
					var instmat = new DAE.instance_material();
					instmat.symbol = mat.Name;
					instmat.target = "#" + o.Content.library_materials.material[(int)m.Meshes[ff].MaterialIndex].id;
					instmat.bind_vertex_input.Add(new DAE.instance_material._bind_vertex_input() { semantic = "UVSET0", input_semantic = "TEXCOORD", input_set = 0 });
					instgem.bind_material.technique_common.instance_material.Add(instmat);
					rootnode.instance_geometry.Add(instgem);
					ff++;
				}
			}
			return o;
		}

		public MTL ToMTL(int Model)
		{
			if (Data.Models.Length == 0 || Data.Models.Length <= Model) return null;
			var o = new MTL();
			var m = Data.Models[Model];
			//foreach (CMDL m in Models)
			{
				foreach (var vv in m.Materials)
				{
					MTL.MTLMaterial mm = new MTL.MTLMaterial(vv.Name);
					mm.DiffuseColor = vv.MaterialColor.DiffuseU32;
					mm.AmbientColor = vv.MaterialColor.AmbientU32;
					mm.Alpha = vv.MaterialColor.Diffuse.W;
					mm.SpecularColor = vv.MaterialColor.Specular0U32;
					if (vv.Tex0 != null && vv.Tex0.TextureObject is ReferenceTexture/* && vv.TextureCoordiators[0].SourceCoordinate == 0*/) mm.DiffuseMapPath = "Tex/" + ((ReferenceTexture)vv.Tex0.TextureObject).LinkedTextureName + ".png";
					//else if (vv.Tex1 != null && vv.Tex1.TextureObject is ReferenceTexture && vv.TextureCoordiators[1].SourceCoordinate == 0) mm.DiffuseMapPath = "Tex/" + ((ReferenceTexture)vv.Tex1.TextureObject).LinkedTextureName + ".png";
					//else if (vv.Tex2 != null && vv.Tex2.TextureObject is ReferenceTexture && vv.TextureCoordiators[2].SourceCoordinate == 0) mm.DiffuseMapPath = "Tex/" + ((ReferenceTexture)vv.Tex2.TextureObject).LinkedTextureName + ".png";
					o.Materials.Add(mm);
				}
			}
			return o;
		}

		public class CGFXIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Models;
			}

			public override string GetFileDescription()
			{
				return "CTR Graphics (CGFX)";
			}

			public override string GetFileFilter()
			{
				return "CTR Graphics (*.bccam, *.bcfog, *.bclgt, *.bcmata, *.bcmcla, *.bcmdl, *.bcptl, *.bcres, *.bcsdr, *.bcskla)|*.bccam;*.bcfog;*.bclgt;*.bcmata;*.bcmcla;*.bcmdl;*.bcptl;*.bcres;*.bcsdr;*.bcskla";
			}

			public override Bitmap GetIcon()
			{
				return Resource.leaf;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'G' && File.Data[2] == 'F' && File.Data[3] == 'X') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
