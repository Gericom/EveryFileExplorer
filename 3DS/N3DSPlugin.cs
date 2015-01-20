using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer;
using LibEveryFileExplorer.Script;
using _3DS.NintendoWare.GFX;
using System.IO;
using CommonFiles;

namespace _3DS
{
	public class N3DSPlugin : EFEPlugin
	{
		public override void OnLoad()
		{
			EFEScript.RegisterCommand("N3DS.NW4C.GFX.ToOBJ", (Action<String, int, String>)N3DS_NW4C_GFX_ToOBJ);
		}

		public static void N3DS_NW4C_GFX_ToOBJ(String BCMDLPath, int ModelIdx, String OBJPath)
		{
			CGFX c = new CGFX(File.ReadAllBytes(BCMDLPath));
			if (c.Data.Models == null || c.Data.Models.Length <= ModelIdx) throw new Exception("Model does not exist in the specified CGFX file!");
			CMDL Model = c.Data.Models[ModelIdx];
			OBJ o = Model.ToOBJ();
			o.MTLPath = Path.GetFileNameWithoutExtension(OBJPath) + ".mtl";
			MTL m = Model.ToMTL(Path.GetFileNameWithoutExtension(OBJPath) + "_Tex");
			byte[] d = o.Write();
			byte[] d2 = m.Write();
			File.Create(OBJPath).Close();
			File.WriteAllBytes(OBJPath, d);
			File.Create(Path.ChangeExtension(OBJPath, "mtl")).Close();
			File.WriteAllBytes(Path.ChangeExtension(OBJPath, "mtl"), d2);
			Directory.CreateDirectory(Path.GetDirectoryName(OBJPath) + "\\" + Path.GetFileNameWithoutExtension(OBJPath) + "_Tex");
			foreach (var v in c.Data.Textures)
			{
				if (!(v is ImageTextureCtr)) continue;
				((ImageTextureCtr)v).GetBitmap().Save(Path.GetDirectoryName(OBJPath) + "\\" + Path.GetFileNameWithoutExtension(OBJPath) + "_Tex\\" + v.Name + ".png");
			}
		}
	}
}
