using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Collections
{
	public struct Matrix34
	{
		public static readonly Matrix34 Identity = new Matrix34(new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0 });

		public Matrix34(params float[] Matrix)
		{
			if (Matrix == null || Matrix.Length != 12) throw new ArgumentException();
			row0 = new Vector4();
			row1 = new Vector4();
			row2 = new Vector4();
			for (int i = 0; i < 12; i++)
			{
				this[i / 4, i % 4] = Matrix[i];
			}
		}

		private Vector4 row0;
		private Vector4 row1;
		private Vector4 row2;

		public Vector4 Row0 { get { return row0; } set { row0 = value; } }
		public Vector4 Row1 { get { return row1; } set { row1 = value; } }
		public Vector4 Row2 { get { return row2; } set { row2 = value; } }

		public float this[int index]
		{
			get { return this[index / 4, index % 4]; }
			set { this[index / 4, index % 4] = value; }
		}

		public float this[int row, int column]
		{
			get
			{
				switch (row)
				{
					case 0: return row0[column];
					case 1: return row1[column];
					case 2: return row2[column];
				}
				throw new IndexOutOfRangeException();
			}
			set
			{
				switch (row)
				{
					case 0: row0[column] = value; return;
					case 1: row1[column] = value; return;
					case 2: row2[column] = value; return;
				}
				throw new IndexOutOfRangeException();
			}
		}

		public static Matrix34 operator *(Matrix34 Left, Matrix34 Right)
		{
			Matrix44 l = new Matrix44(Left, new Vector4(0, 0, 0, 1));
			Matrix44 r = new Matrix44(Right, new Vector4(0, 0, 0, 1));
			Matrix44 Result = l * r;
			Matrix34 result2 = new Matrix34();
			for (int i = 0; i < 12; i++)
			{
				result2[i] = Result[i];
			}
			return result2;
		}

		public static Vector3 operator *(Vector3 Vector, Matrix34 Matrix)
		{
			return new Vector3(((Matrix.Row0.X * Vector.X) + (Matrix.Row0.Y * Vector.Y)) + (Matrix.Row0.Z * Vector.Z), ((Matrix.Row1.X * Vector.X) + (Matrix.Row1.Y * Vector.Y)) + (Matrix.Row1.Z * Vector.Z), ((Matrix.Row2.X * Vector.X) + (Matrix.Row2.Y * Vector.Y)) + (Matrix.Row2.Z * Vector.Z)) + new Vector3(Matrix.Row0.W, Matrix.Row1.W, Matrix.Row2.W);
		}

		public static Vector3 operator *(Matrix34 Matrix, Vector3 Vector)
		{
			return Vector * Matrix;
		}

		public static Vector2 operator *(Vector2 Vector, Matrix34 Matrix)
		{
			return new Vector2(((Matrix.Row0.X * Vector.X) + (Matrix.Row0.Y * Vector.Y)) + Matrix.Row0.Z, ((Matrix.Row1.X * Vector.X) + (Matrix.Row1.Y * Vector.Y)) + Matrix.Row1.Z) + new Vector2(Matrix.Row0.W, Matrix.Row1.W);
		}

		public static Vector2 operator *(Matrix34 Matrix, Vector2 Vector)
		{
			return Vector * Matrix;
		}

		public static Matrix34 operator *(Matrix34 Left, float Right)
		{
			for (int row = 0; row < 3; row++)
			{
				for (int col = 0; col < 4; col++)
				{
					Left[row, col] *= Right;
				}
			}
			return Left;
		}

		public static Matrix34 operator *(float Left, Matrix34 Right)
		{
			for (int row = 0; row < 3; row++)
			{
				for (int col = 0; col < 4; col++)
				{
					Right[row, col] *= Left;
				}
			}
			return Right;
		}
	}
}
