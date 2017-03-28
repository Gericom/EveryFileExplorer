using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace WiiU.NintendoWare.LYT2
{
	public class FLAN : FileFormat<FLAN.FLANIdentifier>
	{
		public class FLANIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Animations;
			}

			public override string GetFileDescription()
			{
				return "Cafe Layout Animation (FLAN)";
			}

			public override string GetFileFilter()
			{
				return "Cafe Layout Animation (*.bflan)|*.bflan";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'F' && File.Data[1] == 'L' && File.Data[2] == 'A' && File.Data[3] == 'N') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
