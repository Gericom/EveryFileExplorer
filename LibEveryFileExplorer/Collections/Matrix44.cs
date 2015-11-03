using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Collections
{
	public struct Matrix44
	{
		public static readonly Matrix44 Identity = new Matrix44(new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 });

		public Matrix44(params float[] Matrix)
		{
			if (Matrix == null || Matrix.Length != 16) throw new ArgumentException();
			row0 = new Vector4();
			row1 = new Vector4();
			row2 = new Vector4();
			row3 = new Vector4();
			for (int i = 0; i < 16; i++)
			{
				this[i / 4, i % 4] = Matrix[i];
			}
		}

		public Matrix44(Matrix33 Matrix)
			: this(Matrix, 0, 0, 0, new Vector4()) { }

		public Matrix44(Matrix33 Matrix, float M14, float M24, float M34, Vector4 Row3)
		{
			row0 = new Vector4();
			row1 = new Vector4();
			row2 = new Vector4();
			row3 = Row3;
			for (int i = 0; i < 9; i++)
			{
				this[i / 3, i % 3] = Matrix[i];
			}
			row0.W = M14;
			row1.W = M24;
			row2.W = M34;
		}

		public Matrix44(Matrix34 Matrix)
			: this(Matrix, new Vector4()) { }

		public Matrix44(Matrix34 Matrix, Vector4 Row3)
		{
			row0 = new Vector4();
			row1 = new Vector4();
			row2 = new Vector4();
			row3 = Row3;
			for (int i = 0; i < 12; i++)
			{
				this[i / 4, i % 4] = Matrix[i];
			}
		}

		public Matrix44(Matrix43 Matrix)
			: this(Matrix, new Vector4()) { }

		public Matrix44(Matrix43 Matrix, Vector4 Column3)
		{
			row0 = new Vector4();
			row1 = new Vector4();
			row2 = new Vector4();
			row3 = new Vector4();
			row0.W = Column3.X;
			row1.W = Column3.Y;
			row2.W = Column3.Z;
			row3.W = Column3.W;
			for (int i = 0; i < 12; i++)
			{
				this[i / 3, i % 3] = Matrix[i];
			}
		}

		private Vector4 row0;
		private Vector4 row1;
		private Vector4 row2;
		private Vector4 row3;

		public Vector4 Row0 { get { return row0; } set { row0 = value; } }
		public Vector4 Row1 { get { return row1; } set { row1 = value; } }
		public Vector4 Row2 { get { return row2; } set { row2 = value; } }
		public Vector4 Row3 { get { return row3; } set { row3 = value; } }

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

		public static Matrix44 operator *(Matrix44 Left, Matrix44 Right)
		{
			Matrix44 tmpMatrix = new Matrix44();

			tmpMatrix[0] = (Left[0] * Right[0]) + (Left[4] * Right[1]) + (Left[8] * Right[2]) + (Left[12] * Right[3]);
			tmpMatrix[1] = (Left[1] * Right[0]) + (Left[5] * Right[1]) + (Left[9] * Right[2]) + (Left[13] * Right[3]);
			tmpMatrix[2] = (Left[2] * Right[0]) + (Left[6] * Right[1]) + (Left[10] * Right[2]) + (Left[14] * Right[3]);
			tmpMatrix[3] = (Left[3] * Right[0]) + (Left[7] * Right[1]) + (Left[11] * Right[2]) + (Left[15] * Right[3]);

			tmpMatrix[4] = (Left[0] * Right[4]) + (Left[4] * Right[5]) + (Left[8] * Right[6]) + (Left[12] * Right[7]);
			tmpMatrix[5] = (Left[1] * Right[4]) + (Left[5] * Right[5]) + (Left[9] * Right[6]) + (Left[13] * Right[7]);
			tmpMatrix[6] = (Left[2] * Right[4]) + (Left[6] * Right[5]) + (Left[10] * Right[6]) + (Left[14] * Right[7]);
			tmpMatrix[7] = (Left[3] * Right[4]) + (Left[7] * Right[5]) + (Left[11] * Right[6]) + (Left[15] * Right[7]);

			tmpMatrix[8] = (Left[0] * Right[8]) + (Left[4] * Right[9]) + (Left[8] * Right[10]) + (Left[12] * Right[11]);
			tmpMatrix[9] = (Left[1] * Right[8]) + (Left[5] * Right[9]) + (Left[9] * Right[10]) + (Left[13] * Right[11]);
			tmpMatrix[10] = (Left[2] * Right[8]) + (Left[6] * Right[9]) + (Left[10] * Right[10]) + (Left[14] * Right[11]);
			tmpMatrix[11] = (Left[3] * Right[8]) + (Left[7] * Right[9]) + (Left[11] * Right[10]) + (Left[15] * Right[11]);

			tmpMatrix[12] = (Left[0] * Right[12]) + (Left[4] * Right[13]) + (Left[8] * Right[14]) + (Left[12] * Right[15]);
			tmpMatrix[13] = (Left[1] * Right[12]) + (Left[5] * Right[13]) + (Left[9] * Right[14]) + (Left[13] * Right[15]);
			tmpMatrix[14] = (Left[2] * Right[12]) + (Left[6] * Right[13]) + (Left[10] * Right[14]) + (Left[14] * Right[15]);
			tmpMatrix[15] = (Left[3] * Right[12]) + (Left[7] * Right[13]) + (Left[11] * Right[14]) + (Left[15] * Right[15]);
			return tmpMatrix;
		}

		public static Vector3 operator *(Vector3 Left, Matrix44 Right)
		{
			Vector3 result = new Vector3();
			result[0] = Left.X * Right[0] + Left.Y * Right[4] + Left.Z * Right[8] + Right[12];
			result[1] = Left.X * Right[1] + Left.Y * Right[5] + Left.Z * Right[9] + Right[13];
			result[2] = Left.X * Right[2] + Left.Y * Right[6] + Left.Z * Right[10] + Right[14];
			return result;
		}

		public static Vector3 operator *(Matrix44 Left, Vector3 Right)
		{
			return Right * Left;
		}

		public static Matrix44 operator *(Matrix44 Left, float Right)
		{
			for (int row = 0; row < 4; row++)
			{
				for (int col = 0; col < 4; col++)
				{
					Left[row, col] *= Right;
				}
			}
			return Left;
		}

		public static Matrix44 operator *(float Left, Matrix44 Right)
		{
			for (int row = 0; row < 4; row++)
			{
				for (int col = 0; col < 4; col++)
				{
					Right[row, col] *= Left;
				}
			}
			return Right;
		}

		public static Matrix44 CreateTranslation(Vector3 Translation)
		{
			Matrix44 result = Matrix44.Identity;
			result[3, 0] = Translation.X;
			result[3, 1] = Translation.Y;
			result[3, 2] = Translation.Z;
			return result;
		}

		public static Matrix44 CreateScale(Vector3 Scale)
		{
			Matrix44 result = Matrix44.Identity;
			result[0, 0] = Scale.X;
			result[1, 1] = Scale.Y;
			result[2, 2] = Scale.Z;
			return result;
		}

		public static explicit operator float[](Matrix44 Matrix)
		{
			float[] result = new float[16];
			for (int i = 0; i < 16; i++) result[i] = Matrix[i];
			return result;
		}
	}
}
