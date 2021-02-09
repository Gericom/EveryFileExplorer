using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.G3D
{
	public class NSBTA : FileFormat<NSBTA.NSBTAIdentifier>
	{
		public class NSBTAIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Animations;
			}

			public override string GetFileDescription()
			{
				return "Nitro System Binary Texture Animation (NSBTA)";
			}

			public override string GetFileFilter()
			{
				return "Nitro System Binary Texture Animation (*.nsbta)|*.nsbta";
			}

			public override Bitmap GetIcon()
			{
				return Resource.sunset_vertical;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'B' && File.Data[1] == 'T' && File.Data[2] == 'A' && File.Data[3] == '0') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
