using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Collections
{
	public struct Matrix33
	{
		public static readonly Matrix33 Identity = new Matrix33(new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 });

		public Matrix33(params float[] Matrix)
		{
			if (Matrix == null || Matrix.Length != 9) throw new ArgumentException();
			row0 = new Vector3();
			row1 = new Vector3();
			row2 = new Vector3();
			for (int i = 0; i < 9; i++)
			{
				this[i / 3, i % 3] = Matrix[i];
			}
		}

		private Vector3 row0;
		private Vector3 row1;
		private Vector3 row2;

		public float this[int index]
		{
			get { return this[index / 3, index % 3]; }
			set { this[index / 3, index % 3] = value; }
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

		public static Matrix33 operator *(Matrix33 Left, Matrix33 Right)
		{
			Matrix33 result = new Matrix33();
			for (int row = 0; row < 3; row++)
			{
				for (int col = 0; col < 3; col++)
				{
					for (int inner = 0; inner < 3; inner++)
					{
						result[row, col] += Left[inner, col] * Right[row, inner];
					}
				}
			}
			return result;
		}

		public static Matrix33 operator *(Matrix33 Left, float Right)
		{
			for (int row = 0; row < 3; row++)
			{
				for (int col = 0; col < 3; col++)
				{
					Left[row, col] *= Right;
				}
			}
			return Left;
		}

		public static Matrix33 operator *(float Left, Matrix33 Right)
		{
			for (int row = 0; row < 3; row++)
			{
				for (int col = 0; col < 3; col++)
				{
					Right[row, col] *= Left;
				}
			}
			return Right;
		}

		public static Matrix33 CreateScale(Vector3 Scale)
		{
			Matrix33 result = Matrix33.Identity;
			result[0, 0] = Scale.X;
			result[1, 1] = Scale.Y;
			result[2, 2] = Scale.Z;
			return result;
		}

		public static explicit operator float[](Matrix33 Matrix)
		{
			float[] result = new float[9];
			for (int i = 0; i < 9; i++) result[i] = Matrix[i];
			return result;
		}
	}
}
