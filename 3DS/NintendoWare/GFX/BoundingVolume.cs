using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.Collections;

namespace _3DS.NintendoWare.GFX
{
	public class BoundingVolume
	{
		public BoundingVolume(EndianBinaryReader er)
		{
			Type = er.ReadUInt32();
		}
		public virtual void Write(EndianBinaryWriter er)
		{
			er.Write(Type);
		}
		public UInt32 Type;
		public static BoundingVolume FromStream(EndianBinaryReader er)
		{
			uint type = er.ReadUInt32();
			er.BaseStream.Position -= 4;
			switch (type)
			{
				case 0x80000000:
					return new OrientedBoundingBox(er);
			}
			return new BoundingVolume(er);
		}
	}

	public class AxisAlignedBoundingBox : BoundingVolume
	{
		public AxisAlignedBoundingBox(EndianBinaryReader er)
			: base(er)
		{
			CenterPosition = er.ReadVector3();
			Size = er.ReadVector3();
		}
		public override void Write(EndianBinaryWriter er)
		{
			base.Write(er);
			er.WriteVector3(CenterPosition);
			er.WriteVector3(Size);
		}
		public Vector3 CenterPosition;
		public Vector3 Size;
	}

	public class OrientedBoundingBox : BoundingVolume
	{
		public OrientedBoundingBox(EndianBinaryReader er)
			: base(er)
		{
			CenterPosition = er.ReadVector3();
			OrientationMatrix = er.ReadSingles(3 * 3);
			Size = er.ReadVector3();
		}
		public override void Write(EndianBinaryWriter er)
		{
			base.Write(er);
			er.WriteVector3(CenterPosition);
			er.Write(OrientationMatrix, 0, 3 * 3);
			er.WriteVector3(Size);
		}
		public Vector3 CenterPosition;
		public Single[] OrientationMatrix;//3x3
		public Vector3 Size;
	}
}
