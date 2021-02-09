using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.G2D
{
	public class NCGR:FileFormat<NCGR.NCGRIdentifier>
	{
		public class NCGRIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Graphics;
            }

			public override string GetFileDescription()
			{
				return "Nitro Character Graphics For Runtime (NCGR)";
			}

			public override string GetFileFilter()
			{
				return "Nitro Character Graphics For Runtime (*.ncgr)|*.ncgr";
			}

			public override Bitmap GetIcon()
			{
				return Resource.image;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'G' && File.Data[2] == 'C' && File.Data[3] == 'N') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
