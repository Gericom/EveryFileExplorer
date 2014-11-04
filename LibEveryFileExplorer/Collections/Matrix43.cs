using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Collections
{
	public struct Matrix43
	{
		public static readonly Matrix43 Identity = new Matrix43(new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 });

		public Matrix43(params float[] Matrix)
		{
			if (Matrix == null || Matrix.Length != 12) throw new ArgumentException();
			row0 = new Vector3();
			row1 = new Vector3();
			row2 = new Vector3();
			row3 = new Vector3();
			for (int i = 0; i < 12; i++)
			{
				this[i / 3, i % 3] = Matrix[i];
			}
		}

		private Vector3 row0;
		private Vector3 row1;
		private Vector3 row2;
		private Vector3 row3;

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
					case 3: return row3[column];
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
					case 3: row3[column] = value; return;
				}
				throw new IndexOutOfRangeException();
			}
		}

		public static Matrix43 operator *(Matrix43 Left, Matrix43 Right)
		{
			Matrix44 l = new Matrix44(Left, new Vector4(0, 0, 0, 1));
			Matrix44 r = new Matrix44(Right, new Vector4(0, 0, 0, 1));
			Matrix44 Result = l * r;
			Matrix43 result2 = new Matrix43();
			for (int row = 0; row < 4; row++)
			{
				for (int col = 0; col < 3; col++)
				{
					result2[row, col] = Result[row,col];
				}
			}
			return result2;
		}

		public static Matrix43 operator *(Matrix43 Left, float Right)
		{
			for (int row = 0; row < 4; row++)
			{
				for (int col = 0; col < 3; col++)
				{
					Left[row, col] *= Right;
				}
			}
			return Left;
		}

		public static Matrix43 operator *(float Left, Matrix43 Right)
		{
			for (int row = 0; row < 4; row++)
			{
				for (int col = 0; col < 3; col++)
				{
					Right[row, col] *= Left;
				}
			}
			return Right;
		}
	}
}
