using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;

/*namespace CommonFiles
{
	public class PNG:FileFormat<PNG.PNGIdentifier>
	{
		public class PNGIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Graphics;
            }

			public override string GetFileDescription()
			{
				return "PNG Image (PNG)";
			}

			public override string GetFileFilter()
			{
				return "Portable Network Graphics (*.png)|*.png";
            }

            public override Bitmap GetIcon()
            {
                return Resource.image;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Name.ToLower().EndsWith(".png")) return FormatMatch.Extension;
                return FormatMatch.No;
            }

        }
    }
}*/

