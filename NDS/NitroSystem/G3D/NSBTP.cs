using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.G3D
{
	public class NSBTP: FileFormat<NSBTP.NSBTPIdentifier>
	{
		public class NSBTPIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
                return Category_Animations;
			}

			public override string GetFileDescription()
			{
				return "Nitro System Binary Texture Particles (NSBTP)";
			}

			public override string GetFileFilter()
			{
				return "Nitro System Binary Texture Particles (*.nsbtp)|*.nsbtp";
			}

			public override Bitmap GetIcon()
			{
				return Resource.stack;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'B' && File.Data[1] == 'T' && File.Data[2] == 'P' && File.Data[3] == '0') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
