using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using LibEveryFileExplorer.Collections;
using System.IO;
using System.Globalization;

namespace CommonFiles
{
	public class OBJ : FileFormat<OBJ.OBJIdentifier>, IWriteable
	{
		public OBJ()
		{
			Vertices = new List<Vector3>();
			Normals = new List<Vector3>();
			TexCoords = new List<Vector2>();
			Faces = new List<OBJFace>();
		}

		public OBJ(byte[] Data)
		{
			var enusculture = new CultureInfo("en-US");
			string curmat = null;
			Vertices = new List<Vector3>();
			Normals = new List<Vector3>();
			TexCoords = new List<Vector2>();
			Faces = new List<OBJFace>();
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
					case "mtllib":
						if (parts.Length < 2) continue;
						MTLPath = line.Substring(parts[0].Length + 1).Trim();
						break;
					case "usemtl":
						if (parts.Length < 2) continue;
						curmat = parts[1];
						break;
					case "v":
						{
							if (parts.Length < 4) continue;
							float x = float.Parse(parts[1], enusculture);
							float y = float.Parse(parts[2], enusculture);
							float z = float.Parse(parts[3], enusculture);
							Vertices.Add(new Vector3(x, y, z));
							break;
						}
					case "vn":
						{
							if (parts.Length < 4) continue;
							float x = float.Parse(parts[1], enusculture);
							float y = float.Parse(parts[2], enusculture);
							float z = float.Parse(parts[3], enusculture);
							Normals.Add(new Vector3(x, y, z));
							break;
						}
					case "vt":
						{
							if (parts.Length < 3) continue;
							float s = float.Parse(parts[1], enusculture);
							float t = float.Parse(parts[2], enusculture);
							TexCoords.Add(new Vector2(s, t));
							break;
						}
					case "f":
						{
							if (parts.Length < 4) continue;
							OBJFace f = new OBJFace();
							f.Material = curmat;
							for (int i = 0; i < parts.Length - 1; i++)
							{
								String[] Parts = parts[i + 1].Split('/');
								f.VertexIndieces.Add(int.Parse(Parts[0]) - 1);
								if (Parts.Length > 1)
								{
									if (Parts[1] != "") f.TexCoordIndieces.Add(int.Parse(Parts[1]) - 1);
									if (Parts.Length > 2 && Parts[2] != "") f.NormalIndieces.Add(int.Parse(Parts[2]) - 1);
								}
							}
							Faces.Add(f);
							break;
						}
				}
			}
			tr.Close();
		}

		public string GetSaveDefaultFileFilter()
		{
			return "Wavefront OBJ File (*.obj)|*.obj";
		}

		public byte[] Write()
		{
			StringBuilder b = new StringBuilder();
			b.AppendLine("# Created by Every File Explorer");
			b.AppendLine();
			if (MTLPath != null) b.AppendFormat("mtllib {0}\n", MTLPath);
			b.AppendLine();
			foreach (Vector3 Vertex in Vertices)
			{
				b.AppendFormat("v {0} {1} {2}\n", (Vertex.X).ToString().Replace(",", "."), (Vertex.Y).ToString().Replace(",", "."), (Vertex.Z).ToString().Replace(",", "."));
			}
			b.AppendLine();
			foreach (Vector3 Normal in Normals)
			{
				b.AppendFormat("vn {0} {1} {2}\n", (Normal.X).ToString().Replace(",", "."), (Normal.Y).ToString().Replace(",", "."), (Normal.Z).ToString().Replace(",", "."));
			}
			b.AppendLine();
			foreach (Vector2 TexCoord in TexCoords)
			{
				b.AppendFormat("vt {0} {1}\n", (TexCoord.X).ToString().Replace(",", "."), (TexCoord.Y).ToString().Replace(",", "."));
			}
			b.AppendLine();
			String CurrentMat = null;
			foreach (var c in Faces)
			{
				bool vertex = c.VertexIndieces.Count != 0;
				bool normal = c.NormalIndieces.Count != 0 && c.NormalIndieces.Count == c.VertexIndieces.Count;
				bool tex = c.TexCoordIndieces.Count != 0 && c.TexCoordIndieces.Count == c.VertexIndieces.Count;
				if (!vertex) throw new Exception("Face has no vertex entries!");
				if (CurrentMat != c.Material)
				{
					b.AppendFormat("usemtl {0}\n", c.Material);
					b.AppendLine();
					CurrentMat = c.Material;
				}

				b.Append("f");

				int count = c.VertexIndieces.Count;
				for (int i = 0; i < count; i++)
				{
					if (vertex && normal && tex) b.AppendFormat(" {0}/{1}/{2}", c.VertexIndieces[i] + 1, c.TexCoordIndieces[i] + 1, c.NormalIndieces[i] + 1);
					else if (vertex && tex) b.AppendFormat(" {0}/{1}", c.VertexIndieces[i] + 1, c.TexCoordIndieces[i] + 1);
					else if (vertex && normal) b.AppendFormat(" {0}//{1}", c.VertexIndieces[i] + 1, c.NormalIndieces[i] + 1);
					else b.AppendFormat(" {0}", c.VertexIndieces[i] + 1);
				}
				b.AppendLine();
			}
			String s = b.ToString();
			return Encoding.ASCII.GetBytes(s);
		}

		public String MTLPath;//Relative to this OBJ file

		public List<Vector3> Vertices;
		public List<Vector3> Normals;
		public List<Vector2> TexCoords;

		public List<OBJFace> Faces;
		public class OBJFace
		{
			public OBJFace()
			{
				VertexIndieces = new List<int>();
				NormalIndieces = new List<int>();
				TexCoordIndieces = new List<int>();
			}
			public List<int> VertexIndieces;
			public List<int> NormalIndieces;
			public List<int> TexCoordIndieces;
			public String Material;
		}

		public class OBJIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Models;
			}

			public override string GetFileDescription()
			{
				return "Wavefront OBJ File (OBJ)";
			}

			public override string GetFileFilter()
			{
				return "Wavefront OBJ File (*.obj)|*.obj";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Name.ToLower().EndsWith(".obj")) return FormatMatch.Extension;
				return FormatMatch.No;
			}

		}
	}
}
