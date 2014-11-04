using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Collections
{
	public struct Vector3
	{
		public Vector3(float Value)
			: this(Value, Value, Value) { }

		public Vector3(Vector2 Vector, float Z)
			: this(Vector.X, Vector.Y, Z) { }

		public Vector3(float X, float Y, float Z)
		{
			x = X;
			y = Y;
			z = Z;
		}

		private float x, y, z;

		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		public float Z { get { return z; } set { z = value; } }

		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
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
				}
				throw new IndexOutOfRangeException();
			}
		}

		public float Length
		{
			get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z); }
		}

		public void Normalize()
		{
			this /= Length;
		}

		public float Dot(Vector3 Right)
		{
			return X * Right.X + Y * Right.Y + Z * Right.Z;
		}

		public Vector3 Cross(Vector3 Right)
		{
			return new Vector3(Y * Right.Z - Right.Y * Z, Z * Right.X - Right.Z * X, X * Right.Y - Right.X * Y);
		}

		public float Angle(Vector3 Right)
		{
			return (float)System.Math.Acos(Dot(Right) / (Length * Right.Length));
		}

		public static Vector3 operator +(Vector3 Left, Vector3 Right)
		{
			return new Vector3(Left.X + Right.X, Left.Y + Right.Y, Left.Z + Right.Z);
		}

		public static Vector3 operator +(Vector3 Left, float Right)
		{
			return new Vector3(Left.X + Right, Left.Y + Right, Left.Z + Right);
		}

		public static Vector3 operator -(Vector3 Left, Vector3 Right)
		{
			return new Vector3(Left.X - Right.X, Left.Y - Right.Y, Left.Z - Right.Z);
		}

		public static Vector3 operator -(Vector3 Left, float Right)
		{
			return new Vector3(Left.X - Right, Left.Y - Right, Left.Z - Right);
		}

		public static Vector3 operator -(Vector3 Left)
		{
			return new Vector3(-Left.X, -Left.Y, -Left.Z);
		}

		public static Vector3 operator *(Vector3 Left, Vector3 Right)
		{
			return new Vector3(Left.X * Right.X, Left.Y * Right.Y, Left.Z * Right.Z);
		}

		public static Vector3 operator *(Vector3 Left, float Right)
		{
			return new Vector3(Left.X * Right, Left.Y * Right, Left.Z * Right);
		}

		public static Vector3 operator *(float Left, Vector3 Right)
		{
			return new Vector3(Left * Right.X, Left * Right.Y, Left * Right.Z);
		}

		public static Vector3 operator /(Vector3 Left, float Right)
		{
			return new Vector3(Left.X / Right, Left.Y / Right, Left.Z / Right);
		}

		public static bool operator ==(Vector3 Left, Vector3 Right)
		{
			return Left.Equals(Right);
		}

		public static bool operator !=(Vector3 Left, Vector3 Right)
		{
			return !Left.Equals(Right);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Vector3)) return false;
			Vector3 vec = (Vector3)obj;
			return vec.X == X && vec.Y == Y && vec.Z == Z;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
