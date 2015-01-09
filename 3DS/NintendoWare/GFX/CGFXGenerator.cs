using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonFiles;
using System.IO;
using System.Drawing;
using LibEveryFileExplorer.Collections;
using LibEveryFileExplorer.IO;

namespace _3DS.NintendoWare.GFX
{
	public class CGFXGenerator
	{
		public static void FromOBJ(CGFX c, String OBJPath, String ModelName = "EFEModel")
		{
			OBJ Model = new OBJ(File.ReadAllBytes(OBJPath));
			if (Model.MTLPath == null) throw new Exception("Model without materials not supported!");
			String MtlPath = Path.GetDirectoryName(OBJPath) + "\\" + Model.MTLPath;
			MTL MatLib = new MTL(File.ReadAllBytes(MtlPath));
			List<String> MatNames = new List<string>();
			foreach (var f in Model.Faces)
			{
				if (!MatNames.Contains(f.Material)) MatNames.Add(f.Material);
			}
			Bitmap[] Textures = new Bitmap[MatLib.Materials.Count];
			int q = 0;
			int NrTextures = 0;
			foreach (var v in MatLib.Materials)
			{
				if (!MatNames.Contains(v.Name)) { q++; continue; }
				if (v.DiffuseMapPath != null)
				{
					Textures[q] = new Bitmap(new MemoryStream(File.ReadAllBytes(Path.GetDirectoryName(MtlPath) + "\\" + v.DiffuseMapPath)));
					NrTextures++;
				}
				q++;
			}
			c.Data.Dictionaries[0] = new DICT();
			c.Data.Dictionaries[0].Add(ModelName);
			c.Data.Models = new CMDL[1];
			c.Data.Models[0] = new CMDL(ModelName);
			CMDL cmdl = c.Data.Models[0];
			//Mesh Node Visibility
			{
				cmdl.NrMeshNodes = 1;
				cmdl.MeshNodeVisibilitiesDict = new DICT();
				cmdl.MeshNodeVisibilitiesDict.Add("CompleteModel");
				cmdl.MeshNodeVisibilities = new CMDL.MeshNodeVisibilityCtr[] { new CMDL.MeshNodeVisibilityCtr("CompleteModel") };
			}
			//Meshes
			{
				cmdl.NrMeshes = (uint)MatNames.Count;
				cmdl.Meshes = new CMDL.Mesh[MatNames.Count];
				for (int i = 0; i < MatNames.Count; i++)
				{
					cmdl.Meshes[i] = new CMDL.Mesh();
					cmdl.Meshes[i].MeshNodeName = "CompleteModel";
					cmdl.Meshes[i].MaterialIndex = (uint)i;
					cmdl.Meshes[i].ShapeIndex = (uint)i;
				}
			}
			//Materials
			{
				cmdl.NrMaterials = (uint)MatNames.Count;
				cmdl.MaterialsDict = new DICT();
				cmdl.Materials = new CMDL.MTOB[MatNames.Count];
				for (int i = 0; i < MatNames.Count; i++)
				{
					cmdl.MaterialsDict.Add(MatNames[i]);
					cmdl.Materials[i] = new CMDL.MTOB(MatNames[i]);
					cmdl.Materials[i].FragShader.TextureCombiners[0].SrcRgb = 0xEE0;
					cmdl.Materials[i].FragShader.TextureCombiners[0].SrcAlpha = 0xEE0;
					for (int qq = 1; qq < 6; qq++)
					{
						cmdl.Materials[i].FragShader.TextureCombiners[qq].SrcRgb = 0xEEF;
						cmdl.Materials[i].FragShader.TextureCombiners[qq].SrcAlpha = 0xEEF;
					}
					Bitmap tex = Textures[MatLib.Materials.IndexOf(MatLib.GetMaterialByName(MatNames[i]))];
					if (tex != null)
					{
						cmdl.Materials[i].NrActiveTextureCoordiators = 1;
						cmdl.Materials[i].TextureCoordiators[0].Scale = new LibEveryFileExplorer.Collections.Vector2(1, 1);
						cmdl.Materials[i].Tex0 = new CMDL.MTOB.TexInfo(MatNames[i]);
						cmdl.Materials[i].FragShader.TextureCombiners[0].SrcRgb = 0xE30;
						cmdl.Materials[i].FragShader.TextureCombiners[0].SrcAlpha = 0xE30;
						cmdl.Materials[i].FragShader.TextureCombiners[0].CombineRgb = 1;
						cmdl.Materials[i].FragShader.TextureCombiners[0].CombineAlpha = 1;
					}
				}
			}
			//Shapes
			{
				cmdl.NrShapes = (uint)MatNames.Count;
				cmdl.Shapes = new CMDL.SeparateDataShape[MatNames.Count];
				for (int i = 0; i < MatNames.Count; i++)
				{
					cmdl.Shapes[i] = new CMDL.SeparateDataShape();
					Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
					Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
					int nrfaces = 0;
					bool texcoords = false;
					foreach (var f in Model.Faces)
					{
						if (f.Material != MatNames[i]) continue;
						nrfaces++;
						if (f.TexCoordIndieces.Count > 0) texcoords = true;
						foreach (var qqq in f.VertexIndieces)
						{
							if (Model.Vertices[qqq].X < min.X) min.X = Model.Vertices[qqq].X;
							if (Model.Vertices[qqq].Y < min.Y) min.Y = Model.Vertices[qqq].Y;
							if (Model.Vertices[qqq].Z < min.Z) min.Z = Model.Vertices[qqq].Z;
							if (Model.Vertices[qqq].X > max.X) max.X = Model.Vertices[qqq].X;
							if (Model.Vertices[qqq].Y > max.Y) max.Y = Model.Vertices[qqq].Y;
							if (Model.Vertices[qqq].Z > max.Z) max.Z = Model.Vertices[qqq].Z;
						}
					}
					((OrientedBoundingBox)cmdl.Shapes[i].BoundingBox).CenterPosition = (min + max) / 2;
					((OrientedBoundingBox)cmdl.Shapes[i].BoundingBox).Size = max - min;
					cmdl.Shapes[i].NrPrimitiveSets = 1;
					cmdl.Shapes[i].PrimitiveSets = new CMDL.SeparateDataShape.PrimitiveSet[1];
					cmdl.Shapes[i].PrimitiveSets[0] = new CMDL.SeparateDataShape.PrimitiveSet();
					cmdl.Shapes[i].PrimitiveSets[0].NrPrimitives = 1;
					cmdl.Shapes[i].PrimitiveSets[0].Primitives = new CMDL.SeparateDataShape.PrimitiveSet.Primitive[1];
					cmdl.Shapes[i].PrimitiveSets[0].Primitives[0] = new CMDL.SeparateDataShape.PrimitiveSet.Primitive();
					cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].NrBufferObjects = 1;
					cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].BufferObjects = new uint[] { 0 };
					cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].NrIndexStreams = 1;
					cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].IndexStreams = new CMDL.SeparateDataShape.PrimitiveSet.Primitive.IndexStreamCtr[1];
					cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].IndexStreams[0] = new CMDL.SeparateDataShape.PrimitiveSet.Primitive.IndexStreamCtr();
					if (nrfaces * 3 > 255) cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].IndexStreams[0].FormatType = 0x1403;
					cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].IndexStreams[0].FaceDataLength = (uint)(nrfaces * 3 * ((nrfaces * 3 > 255) ? 2 : 1));
					cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].IndexStreams[0].FaceData = new byte[cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].IndexStreams[0].FaceDataLength];
					int offs = 0;
					int idx = 0;
					foreach (var f in Model.Faces)
					{
						if (f.Material != MatNames[i]) continue;
						if (nrfaces * 3 > 255)
						{
							IOUtil.WriteU16LE(cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].IndexStreams[0].FaceData, offs, (ushort)idx);
							IOUtil.WriteU16LE(cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].IndexStreams[0].FaceData, offs + 2, (ushort)(idx + 1));
							IOUtil.WriteU16LE(cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].IndexStreams[0].FaceData, offs + 4, (ushort)(idx + 2));
							offs += 2 * 3;
						}
						else
						{
							cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].IndexStreams[0].FaceData[idx] = (byte)idx;
							cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].IndexStreams[0].FaceData[idx + 1] = (byte)(idx + 1);
							cmdl.Shapes[i].PrimitiveSets[0].Primitives[0].IndexStreams[0].FaceData[idx + 2] = (byte)(idx + 2);
							offs += 3;
						}
						idx += 3;
					}
					cmdl.Shapes[i].NrVertexAttributes = 2;
					cmdl.Shapes[i].VertexAttributes = new CMDL.SeparateDataShape.VertexAttributeCtr[2];
					//interleaved
					cmdl.Shapes[i].VertexAttributes[0] = new CMDL.SeparateDataShape.InterleavedVertexStreamCtr();
					((CMDL.SeparateDataShape.InterleavedVertexStreamCtr)cmdl.Shapes[i].VertexAttributes[0]).NrVertexStreams = (texcoords ? 2u : 1u);
					((CMDL.SeparateDataShape.InterleavedVertexStreamCtr)cmdl.Shapes[i].VertexAttributes[0]).VertexStreams = new CMDL.SeparateDataShape.InterleavedVertexStreamCtr.VertexStreamCtr[texcoords ? 2 : 1];
					((CMDL.SeparateDataShape.InterleavedVertexStreamCtr)cmdl.Shapes[i].VertexAttributes[0]).VertexStreams[0] = new CMDL.SeparateDataShape.InterleavedVertexStreamCtr.VertexStreamCtr(CMDL.SeparateDataShape.VertexAttributeCtr.VertexAttributeUsage.Position, CMDL.SeparateDataShape.DataType.GL_FLOAT, 3, 0);
					if (texcoords) ((CMDL.SeparateDataShape.InterleavedVertexStreamCtr)cmdl.Shapes[i].VertexAttributes[0]).VertexStreams[1] = new CMDL.SeparateDataShape.InterleavedVertexStreamCtr.VertexStreamCtr(CMDL.SeparateDataShape.VertexAttributeCtr.VertexAttributeUsage.TextureCoordinate0, CMDL.SeparateDataShape.DataType.GL_FLOAT, 2, 12);
					((CMDL.SeparateDataShape.InterleavedVertexStreamCtr)cmdl.Shapes[i].VertexAttributes[0]).VertexDataEntrySize = (texcoords ? 20u : 12u);
					byte[] Result = new byte[nrfaces * 3 * ((CMDL.SeparateDataShape.InterleavedVertexStreamCtr)cmdl.Shapes[i].VertexAttributes[0]).VertexDataEntrySize];
					offs = 0;
					foreach (var f in Model.Faces)
					{
						if (f.Material != MatNames[i]) continue;
						for (int qqq = 0; qqq < 3; qqq++)
						{
							Vector3 Pos = Model.Vertices[f.VertexIndieces[qqq]];
							IOUtil.WriteSingleLE(Result, offs, Pos.X);
							IOUtil.WriteSingleLE(Result, offs + 4, Pos.Y);
							IOUtil.WriteSingleLE(Result, offs + 8, Pos.Z);
							offs += 12;
							if (texcoords)
							{
								Vector2 Tex = Model.TexCoords[f.TexCoordIndieces[qqq]];
								IOUtil.WriteSingleLE(Result, offs, Tex.X);
								IOUtil.WriteSingleLE(Result, offs + 4, Tex.Y);
								offs += 8;
							}
						}
					}
					((CMDL.SeparateDataShape.InterleavedVertexStreamCtr)cmdl.Shapes[i].VertexAttributes[0]).VertexStreamLength = (uint)Result.Length;
					((CMDL.SeparateDataShape.InterleavedVertexStreamCtr)cmdl.Shapes[i].VertexAttributes[0]).VertexStream = Result;
					//color
					cmdl.Shapes[i].VertexAttributes[1] = new CMDL.SeparateDataShape.VertexParamAttributeCtr(CMDL.SeparateDataShape.VertexAttributeCtr.VertexAttributeUsage.Color, 1, 1, 1, MatLib.GetMaterialByName(MatNames[i]).Alpha);
				}
			}
			if (NrTextures != 0)
			{
				c.Data.Dictionaries[1] = new DICT();
				c.Data.Textures = new TXOB[NrTextures];
				int qqq = 0;
				int idx = 0;
				foreach (Bitmap b in Textures)
				{
					if (b == null) { qqq++; continue; }
					c.Data.Dictionaries[1].Add(MatLib.Materials[qqq].Name);
					c.Data.Textures[idx] = new ImageTextureCtr(MatLib.Materials[qqq].Name, b, GPU.Textures.ImageFormat.ETC1A4);
					idx++;
					qqq++;
				}
			}
		}
	}
}
