using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using LibEveryFileExplorer.IO;

 namespace NDS
{
    public class SPA:FileFormat<SPA.SPAIdentifier>
    {
        public class SPAIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Particles;
            }

            public override string GetFileDescription()
            {
                return "Nitro System Particles Archive (SPA)";
            }

            public override string GetFileFilter()
            {
                return "Nitro System Particles Archive (*.spa)|*.spa";
            }

            public override Bitmap GetIcon()
            {
                return Resource.water;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 3 && File.Data[1] == 'A' && File.Data[2] == 'P' && File.Data[3] == 'S') return FormatMatch.Content;
                return FormatMatch.No;
			}

		}
	}
}

