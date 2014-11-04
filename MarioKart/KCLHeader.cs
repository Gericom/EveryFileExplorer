using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Collections;

namespace MarioKart
{
	public abstract class KCLHeader
	{
		public UInt32 VerticesOffset;
		public UInt32 NormalsOffset;
		public UInt32 PlanesOffset;//-0x10
		public UInt32 OctreeOffset;
		public Single Unknown1;
		public Vector3 OctreeOrigin;
		public UInt32 XMask;
		public UInt32 YMask;
		public UInt32 ZMask;
		public UInt32 CoordShift;
		public UInt32 YShift;
		public UInt32 ZShift;
		public Single Unknown2;
	}
}
