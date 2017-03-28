using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.G2D
{
	public class NCLR:FileFormat<NCLR.NCLRIdentifier>
	{
		public class NCLRIdentifier:FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Palettes;
			}

			public override string GetFileDescription()
			{
				return "Nitro Color Palette For Runtime (NCLR)";
			}

			public override string GetFileFilter()
			{
				return "Nitro Color Palette For Runtime (*.nclr)|*.nclr";
			}

			public override Bitmap GetIcon()
			{
				return Resource.color_swatch1;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'L' && File.Data[2] == 'C' && File.Data[3] == 'N') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
