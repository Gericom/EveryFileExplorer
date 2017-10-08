using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace GCNWii.JSystem
{
	public class RLAN:FileFormat<RLAN.RLANIdentifier>
	{
		public class RLANIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Animations;
			}

			public override string GetFileDescription()
			{
				return "JSystem Resource Layout Animation (RLAN)";
			}

			public override string GetFileFilter()
			{
				return "JSystem Resource Layout Animation (*.brlan)|*.brlan";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'L' && File.Data[2] == 'A' && File.Data[3] == 'N') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
