using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarioKart.MK64
{
	public class F3DEX
	{
		public enum F3DEXCommand : byte
		{
			F3D_SPNOOP = 0x00,
			F3D_MTX = 0x01,
			F3D_RESERVED0 = 0x02,
			F3D_MOVEMEM = 0x03,
			F3D_VTX = 0x04,
			F3D_RESERVED1 = 0x05,
			F3D_DL = 0x06,
			F3D_RESERVED2 = 0x07,
			F3D_RESERVED3 = 0x08,
			F3D_SPRITE2D_BASE = 0x09,

			F3D_MOVEWORD = 0xBC,
			G_RDPLOADSYNC = 0xE6,
			G_RDPTILESYNC = 0xE8,
			G_LOADBLOCK = 0xF3,
			G_SETTILESIZE = 0xF4,
			G_SETTILE = 0xF5,
			G_SETTIMG = 0xFD
		}
	}
}
