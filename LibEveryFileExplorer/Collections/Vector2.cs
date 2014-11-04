using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Collections
{
	public struct Vector2
	{
		public Vector2(float Value)
			: this(Value, Value) { }

		public Vector2(float X, float Y)
		{
			x = X;
			y = Y;
		}

		private float x, y;

		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }

		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new IndexOutOfRangeException();
			}
			set
			{
				switch (index)
				{
					case 0: X = value; return;
					case 1: Y = value; return;
				}
				throw new IndexOutOfRangeException();
			}
		}

		public float Length
		{
			get { return (float)System.Math.Sqrt(X * X + Y * Y); }
		}

		public void Normalize()
		{
			this /= Length;
		}

		public float Dot(Vector2 Right)
		{
			return X * Right.X + Y * Right.Y;
		}

		public static Vector2 operator +(Vector2 Left, Vector2 Right)
		{
			return new Vector2(Left.X + Right.X, Left.Y + Right.Y);
		}

		public static Vector2 operator +(Vector2 Left, float Right)
		{
			return new Vector2(Left.X + Right, Left.Y + Right);
		}

		public static Vector2 operator -(Vector2 Left, Vector2 Right)
		{
			return new Vector2(Left.X - Right.X, Left.Y - Right.Y);
		}

		public static Vector2 operator -(Vector2 Left, float Right)
		{
			return new Vector2(Left.X - Right, Left.Y - Right);
		}

		public static Vector2 operator -(Vector2 Left)
		{
			return new Vector2(-Left.X, -Left.Y);
		}

		public static Vector2 operator *(Vector2 Left, Vector2 Right)
		{
			return new Vector2(Left.X * Right.X, Left.Y * Right.Y);
		}

		public static Vector2 operator *(Vector2 Left, float Right)
		{
			return new Vector2(Left.X * Right, Left.Y * Right);
		}

		public static Vector2 operator *(float Left, Vector2 Right)
		{
			return new Vector2(Left * Right.X, Left * Right.Y);
		}

		public static Vector2 operator /(Vector2 Left, float Right)
		{
			return new Vector2(Left.X / Right, Left.Y / Right);
		}

		public static bool operator ==(Vector2 Left, Vector2 Right)
		{
			return Left.Equals(Right);
		}

		public static bool operator !=(Vector2 Left, Vector2 Right)
		{
			return !Left.Equals(Right);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Vector2)) return false;
			Vector2 vec = (Vector2)obj;
			return vec.X == X && vec.Y == Y;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
