using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Collections
{
	public struct Vector4
	{
		public Vector4(float Value)
			: this(Value, Value, Value, Value) { }

		public Vector4(Vector2 Vector, float Z, float W)
			: this(Vector.X, Vector.Y, Z, W) { }

		public Vector4(Vector2 Vector1, Vector2 Vector2)
			: this(Vector1.X, Vector1.Y, Vector2.X, Vector2.Y) { }

		public Vector4(Vector3 Vector, float W)
			: this(Vector.X, Vector.Y, Vector.Z, W) { }

		public Vector4(float X, float Y, float Z, float W)
		{
			x = X;
			y = Y;
			z = Z;
			w = W;
		}

		private float x, y, z, w;

		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		public float Z { get { return z; } set { z = value; } }
		public float W { get { return w; } set { w = value; } }

		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
					case 3: return W;
				}
				throw new IndexOutOfRangeException();
			}
			set
			{
				switch (index)
				{
					case 0: X = value; return;
					case 1: Y = value; return;
					case 2: Z = value; return;
					case 3: W = value; return;
				}
				throw new IndexOutOfRangeException();
			}
		}

		public float Length
		{
			get { return (float)System.Math.Sqrt(X * X + Y * Y + Z * Z + W * W); }
		}

		public void Normalize()
		{
			this /= Length;
		}

		public float Dot(Vector4 Right)
		{
			return X * Right.X + Y * Right.Y + Z * Right.Z;
		}

		public static Vector4 operator +(Vector4 Left, Vector4 Right)
		{
			return new Vector4(Left.X + Right.X, Left.Y + Right.Y, Left.Z + Right.Z, Left.W + Right.W);
		}

		public static Vector4 operator +(Vector4 Left, float Right)
		{
			return new Vector4(Left.X + Right, Left.Y + Right, Left.Z + Right, Left.W + Right);
		}

		public static Vector4 operator -(Vector4 Left, Vector4 Right)
		{
			return new Vector4(Left.X - Right.X, Left.Y - Right.Y, Left.Z - Right.Z, Left.W - Right.W);
		}

		public static Vector4 operator -(Vector4 Left, float Right)
		{
			return new Vector4(Left.X - Right, Left.Y - Right, Left.Z - Right, Left.W - Right);
		}

		public static Vector4 operator -(Vector4 Left)
		{
			return new Vector4(-Left.X, -Left.Y, -Left.Z, - Left.W);
		}

		public static Vector4 operator *(Vector4 Left, Vector4 Right)
		{
			return new Vector4(Left.X * Right.X, Left.Y * Right.Y, Left.Z * Right.Z, Left.W * Right.W);
		}

		public static Vector4 operator *(Vector4 Left, float Right)
		{
			return new Vector4(Left.X * Right, Left.Y * Right, Left.Z * Right, Left.W * Right);
		}

		public static Vector4 operator *(float Left, Vector4 Right)
		{
			return new Vector4(Left * Right.X, Left * Right.Y, Left * Right.Z, Left * Right.W);
		}

		public static Vector4 operator /(Vector4 Left, float Right)
		{
			return new Vector4(Left.X / Right, Left.Y / Right, Left.Z / Right, Left.W / Right);
		}

		public static bool operator ==(Vector4 Left, Vector4 Right)
		{
			return Left.Equals(Right);
		}

		public static bool operator !=(Vector4 Left, Vector4 Right)
		{
			return !Left.Equals(Right);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Vector4)) return false;
			Vector4 vec = (Vector4)obj;
			return vec.X == X && vec.Y == Y && vec.Z == Z && vec.W == W;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
