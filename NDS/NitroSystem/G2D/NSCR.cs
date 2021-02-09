using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.G2D
{
	public class NSCR: FileFormat<NSCR.NSCRIdentifier>
	{
		public class NSCRIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Screens;
			}

			public override string GetFileDescription()
			{
				return "Nitro Screen For Runtime (NSCR)";
			}

			public override string GetFileFilter()
			{
				return "Nitro Screen For Runtime (*.nscr)|*.nscr";
			}

			public override Bitmap GetIcon()
			{
				return Resource.map;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'C' && File.Data[2] == 'S' && File.Data[3] == 'N') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
