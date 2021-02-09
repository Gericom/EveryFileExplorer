using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS
{
	public class VX:FileFormat<VX.VXIdentifier>
	{
		public class VXIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Movies;

            }

			public override string GetFileDescription()
			{
				return "DS Mobiclip Video Files (VX)";
			}

			public override string GetFileFilter()
			{
				return "DS Mobiclip Video Files (*.VX)|*.VX";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

			public override FormatMatch IsFormat(EFEFile File)
			{
                if (File.Data.Length > 4 && File.Data[0] == 'V' && File.Data[1] == 'X' && File.Data[2] == 'D' && File.Data[3] == 'S') return FormatMatch.Content;
                return FormatMatch.No;
			}

		}
	}
}
