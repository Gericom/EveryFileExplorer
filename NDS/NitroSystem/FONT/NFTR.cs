using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;

namespace NDS.NitroSystem.FONT
{
	public class NFTR:FileFormat<NFTR.NFTRIdentifier>, IViewable
    {
        public Form GetDialog()
        {
            return new Form();
        }

        public class NFTRIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Fonts;
			}

			public override string GetFileDescription()
			{
				return "Nitro Fonts Resources (NFTR)";
			}

			public override string GetFileFilter()
			{
				return "Nitro Fonts Resources (*.nftr)|*.nftr";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'T' && File.Data[2] == 'F' && File.Data[3] == 'N') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
