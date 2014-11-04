using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Collections;

namespace LibEveryFileExplorer._3D
{
	public class Triangle
	{
		public Triangle(Vector3 A, Vector3 B, Vector3 C)
		{
			PointA = A;
			PointB = B;
			PointC = C;
		}

		public Vector3 PointA { get; set; }
		public Vector3 PointB { get; set; }
		public Vector3 PointC { get; set; }

		public Vector3 Normal
		{
			get
			{
				Vector3 a = (PointB - PointA).Cross(PointC - PointA);
				return a / (float)Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
			}
		}
	}
}
