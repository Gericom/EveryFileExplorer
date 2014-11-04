using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Collections;
using System.Drawing;

namespace LibEveryFileExplorer._3D
{
	public class Polygon
	{
		public PolygonType PolyType;
		public Vector3[] Normals;
		public Vector2[] TexCoords;
		public Vector2[] TexCoords2;
		public Vector2[] TexCoords3;
		public Vector3[] Vertex;
		public Color[] Colors;
		public Polygon() { }
		public Polygon(PolygonType PolyType, Vector3[] Vertex, Vector3[] Normals, Vector2[] TexCoords, Color[] Colors = null)
		{
			this.PolyType = PolyType;
			this.Normals = Normals;
			this.TexCoords = TexCoords;
			this.Vertex = Vertex;
			this.Colors = Colors;
		}
	}

	public enum PolygonType
	{
		Triangle,
		Quad,
		TriangleStrip,
		QuadStrip
	}
}
