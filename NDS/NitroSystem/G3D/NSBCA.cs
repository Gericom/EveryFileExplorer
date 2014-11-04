using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.G3D
{
	public class NSBCA : FileFormat<NSBCA.NSBCAIdentifier>
	{
		public class NSBCAIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Animations;
			}

			public override string GetFileDescription()
			{
				return "Nitro System Binary Character Animation (NSBCA)";
			}

			public override string GetFileFilter()
			{
				return "Nitro System Binary Character Animation (*.nsbca)|*.nsbca";
			}

			public override Bitmap GetIcon()
			{
				return Resource.lollypopanim;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'B' && File.Data[1] == 'C' && File.Data[2] == 'A' && File.Data[3] == '0') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
