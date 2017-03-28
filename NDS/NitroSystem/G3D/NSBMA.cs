using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.G3D
{
	public class NSBMA : FileFormat<NSBMA.NSBMAIdentifier>
	{
		public class NSBMAIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Animations;
			}

			public override string GetFileDescription()
			{
				return "Nitro System Binary Material Animation (NSBMA)";
			}

			public override string GetFileFilter()
			{
				return "Nitro System Binary Material Animation (*.nsbma)|*.nsbma";
			}

			public override Bitmap GetIcon()
			{
				return Resource.traffic_light_anim;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'B' && File.Data[1] == 'M' && File.Data[2] == 'A' && File.Data[3] == '0') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
