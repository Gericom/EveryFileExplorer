using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.G2D
{
	public class NCER: FileFormat<NCER.NCERIdentifier>
	{
		public class NCERIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Cells;
            }

			public override string GetFileDescription()
			{
				return "Nitro Cells For Runtime (NCER)";
			}

			public override string GetFileFilter()
			{
				return "Nitro Cells For Runtime (*.NCER)|*.NCER";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'E' && File.Data[2] == 'C' && File.Data[3] == 'N') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
