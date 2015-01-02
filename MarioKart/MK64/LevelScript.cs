using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.IO;
using System.IO;
using CommonFiles;
using System.Drawing;

namespace MarioKart.MK64
{
	public class LevelScript
	{
		/*public enum LevelScriptCommand : byte
		{

		}*/

		//0x1A - 2 params
		//0x20 - 3 params
		//0x26 - 0 params
		//0x2A - 0 params
		//0x2B - 2 params
		//0x3A - 2 params
		//0x3E - 2 params
		//0x58 - 4 params

		public static void Parse(byte[] Data, OBJ o)
		{
			string Material = "";
			byte MatFlagS = 0;
			byte MatFlagT = 0;
			ushort VtxBase = 0;
			int offs = 0;
			while (offs + 1 < Data.Length)
			{
				byte cmd = Data[offs++];
				switch (cmd)
				{
					case 0x15: break;
					case 0x17: break;
					case 0x1A: //seems to be texture flags
					case 0x1B:
					case 0x1C:
					case 0x1D: 
						MatFlagS = Data[offs];
						MatFlagT = Data[offs + 1];
						offs += 2;
						break;
					case 0x20://seems to be used for 32x32 textures
					case 0x21://seems to be used for 64x32 textures
					case 0x22:
					case 0x23:
						{
							Material = "M" + IOUtil.ReadU16LE(Data, offs) + "M";
							if (cmd > 0x21) Material += "_swp";
							byte unk = Data[offs + 2];
							offs += 3;
							break;
						}
					case 0x26: break;
					case 0x27: break;
					case 0x29:
						{
							ushort a = IOUtil.ReadU16LE(Data, offs);
							int vtx1 = VtxBase + (a & 0x1F);
							int vtx2 = VtxBase + ((a >> 5) & 0x1F);
							int vtx3 = VtxBase + ((a >> 10) & 0x1F);
							OBJ.OBJFace f = new OBJ.OBJFace();
							f.VertexIndieces.Add(vtx1);
							f.VertexIndieces.Add(vtx2);
							f.VertexIndieces.Add(vtx3);
							f.TexCoordIndieces.Add(vtx1);
							f.TexCoordIndieces.Add(vtx2);
							f.TexCoordIndieces.Add(vtx3);
							f.Material = Material;
							o.Faces.Add(f);
							offs += 2;
							break;
						}
					case 0x2A: break;
					case 0x2B:
						{
							ushort unk = IOUtil.ReadU16LE(Data, offs);
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
							VtxBase = IOUtil.ReadU16LE(Data, offs);
							offs += 2;
							break;
						}
					case 0x58:
						{
							ushort a = IOUtil.ReadU16LE(Data, offs);
							int vtx1 = VtxBase + (a & 0x1F);
							int vtx2 = VtxBase + ((a >> 5) & 0x1F);
							int vtx3 = VtxBase + ((a >> 10) & 0x1F);
							OBJ.OBJFace f = new OBJ.OBJFace();
							f.VertexIndieces.Add(vtx1);
							f.VertexIndieces.Add(vtx2);
							f.VertexIndieces.Add(vtx3);
							f.TexCoordIndieces.Add(vtx1);
							f.TexCoordIndieces.Add(vtx2);
							f.TexCoordIndieces.Add(vtx3);
							f.Material =Material;
							o.Faces.Add(f);
							ushort b = IOUtil.ReadU16LE(Data, offs + 2);
							vtx1 = VtxBase + (b & 0x1F);
							vtx2 = VtxBase + ((b >> 5) & 0x1F);
							vtx3 = VtxBase + ((b >> 10) & 0x1F);
							f = new OBJ.OBJFace();
							f.VertexIndieces.Add(vtx1);
							f.VertexIndieces.Add(vtx2);
							f.VertexIndieces.Add(vtx3);
							f.TexCoordIndieces.Add(vtx1);
							f.TexCoordIndieces.Add(vtx2);
							f.TexCoordIndieces.Add(vtx3);
							f.Material = Material;
							o.Faces.Add(f);
							offs += 4;
							break;
						}
					case 0xFF://End
						goto end;
					default:
						break;
				}
			}
		end:
			;
		}
	}
}
