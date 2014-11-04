using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;

namespace CommonFiles
{
	public class MTL : FileFormat<MTL.MTLIdentifier>, IWriteable
	{
		public MTL()
		{
			Materials = new List<MTLMaterial>();
		}

		public MTL(byte[] Data)
		{
			MTLMaterial CurrentMaterial = null;
			TextReader tr = new StreamReader(new MemoryStream(Data));
			String line;
			while ((line = tr.ReadLine()) != null)
			{
				line = line.Trim();
				if (line.Length < 1 || line.StartsWith("#")) continue;

				string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 1) continue;
				switch (parts[0])
				{
					case "newmtl":
						if (parts.Length < 2) continue;
						if (CurrentMaterial != null) Materials.Add(CurrentMaterial);
						CurrentMaterial = new MTLMaterial(parts[1]);
						break;
					case "Ka":
						{
							if (parts.Length < 4) continue;
							float r = float.Parse(parts[1]);
							float g = float.Parse(parts[2]);
							float b = float.Parse(parts[3]);
							CurrentMaterial.AmbientColor = Color.FromArgb((int)(r * 255f), (int)(g * 255f), (int)(b * 255f));
							break;
						}
					case "Kd":
						{
							if (parts.Length < 4) continue;
							float r = float.Parse(parts[1]);
							float g = float.Parse(parts[2]);
							float b = float.Parse(parts[3]);
							CurrentMaterial.DiffuseColor = Color.FromArgb((int)(r * 255f), (int)(g * 255f), (int)(b * 255f));
							break;
						}
					case "Ks":
						{
							if (parts.Length < 4) continue;
							float r = float.Parse(parts[1]);
							float g = float.Parse(parts[2]);
							float b = float.Parse(parts[3]);
							CurrentMaterial.SpecularColor = Color.FromArgb((int)(r * 255f), (int)(g * 255f), (int)(b * 255f));
							break;
						}
					case "d":
						if (parts.Length < 2) continue;
						CurrentMaterial.Alpha = float.Parse(parts[1]);
						break;
					case "map_Kd":
						CurrentMaterial.DiffuseMapPath = line.Substring(parts[0].Length + 1).Trim();
						break;
				}
			}
			tr.Close();
		}

		public string GetSaveDefaultFileFilter()
		{
			return "Material Library (*.mtl)|*.mtl";
		}

		public byte[] Write()
		{
			StringBuilder b = new StringBuilder();
			b.AppendLine("# Created by Every File Explorer");
			b.AppendLine();
			foreach (MTLMaterial m in Materials)
			{
				b.AppendFormat("newmtl {0}\n", m.Name);
				b.AppendFormat("Ka {0} {1} {2}\n", (m.AmbientColor.R / 255f).ToString().Replace(",", "."), (m.AmbientColor.G / 255f).ToString().Replace(",", "."), (m.AmbientColor.B / 255f).ToString().Replace(",", "."));
				b.AppendFormat("Kd {0} {1} {2}\n", (m.DiffuseColor.R / 255f).ToString().Replace(",", "."), (m.DiffuseColor.G / 255f).ToString().Replace(",", "."), (m.DiffuseColor.B / 255f).ToString().Replace(",", "."));
				b.AppendFormat("Ks {0} {1} {2}\n", (m.SpecularColor.R / 255f).ToString().Replace(",", "."), (m.SpecularColor.G / 255f).ToString().Replace(",", "."), (m.SpecularColor.B / 255f).ToString().Replace(",", "."));
				b.AppendFormat("d {0}\n", m.Alpha.ToString().Replace(",", "."));
				if (m.DiffuseMapPath != null) b.AppendFormat("map_Kd {0}\n", m.DiffuseMapPath);
				b.AppendLine();
			}
			String s = b.ToString();
			return Encoding.ASCII.GetBytes(s);
		}

		public List<MTLMaterial> Materials;
		public MTLMaterial GetMaterialByName(String Name)
		{
			foreach (MTLMaterial m in Materials)
			{
				if (m.Name == Name) return m;
			}
			return null;
		}
		public class MTLMaterial
		{
			public MTLMaterial(String Name)
			{
				this.Name = Name;
			}
			public String Name;
			public Color DiffuseColor;
			public Color AmbientColor;
			public Color SpecularColor;
			public float Alpha = 1;

			public String DiffuseMapPath;

			public override string ToString()
			{
				return Name;
			}
		}


		public class MTLIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Textures;
			}

			public override string GetFileDescription()
			{
				return "Material Library (MTL)";
			}

			public override string GetFileFilter()
			{
				return "Material Library (*.mtl)|*.mtl";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Name.ToLower().EndsWith(".mtl")) return FormatMatch.Extension;
				return FormatMatch.No;
			}
		}
	}
}
