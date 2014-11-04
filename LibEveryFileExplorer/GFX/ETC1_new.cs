using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LibEveryFileExplorer.GFX
{
	public class ETC1_new
	{
		private static void GenBaseColor(Color Color1, Color Color2, ref ulong Data)
		{
			int R1 = Color1.R;
			int G1 = Color1.G;
			int B1 = Color1.B;
			int R2 = Color2.R;
			int G2 = Color2.G;
			int B2 = Color2.B;
			//First look if differencial is possible.
			int RDiff = (R2 - R1) / 8;
			int GDiff = (G2 - G1) / 8;
			int BDiff = (B2 - B1) / 8;
			if (RDiff > -4 && RDiff < 3 && GDiff > -4 && GDiff < 3 && BDiff > -4 && BDiff < 3)
			{
				Data |= 1ul << 32;
				R1 /= 8;
				G1 /= 8;
				B1 /= 8;
				Data |= (ulong)R1 << 59;
				Data |= (ulong)G1 << 51;
				Data |= (ulong)B1 << 43;
				Data |= (ulong)(RDiff & 0x7) << 56;
				Data |= (ulong)(GDiff & 0x7) << 48;
				Data |= (ulong)(BDiff & 0x7) << 40;
			}
			else
			{
				Data |= (ulong)(R1 / 0x11) << 60;
				Data |= (ulong)(G1 / 0x11) << 52;
				Data |= (ulong)(B1 / 0x11) << 44;

				Data |= (ulong)(R2 / 0x11) << 56;
				Data |= (ulong)(G2 / 0x11) << 48;
				Data |= (ulong)(B2 / 0x11) << 40;

				/*r1 = (int)((data >> 60) & 0xF) * 0x11;
				g1 = (int)((data >> 52) & 0xF) * 0x11;
				b1 = (int)((data >> 44) & 0xF) * 0x11;
				r2 = (int)((data >> 56) & 0xF) * 0x11;
				g2 = (int)((data >> 48) & 0xF) * 0x11;
				b2 = (int)((data >> 40) & 0xF) * 0x11;*/
			}
		}
	}
}
