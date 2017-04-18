using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.G2D
{
	public class NANR:FileFormat<NANR.NANRIdentifier>
	{
		public class NANRIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Animations;
			}

			public override string GetFileDescription()
			{
				return "Nitro Animation Resources (NANR)";
			}

			public override string GetFileFilter()
			{
				return "Nitro Animation Resources (*.nanr)|*.nanr";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'N' && File.Data[2] == 'A' && File.Data[3] == 'N') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
