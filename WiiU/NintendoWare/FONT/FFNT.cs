using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;

namespace WiiU.NintendoWare.LYT2
{
	public class FFNT : FileFormat<FFNT.FFNTIdentifier>, IViewable
    {
        public Form GetDialog()
        {
            return new Form();
        }

        public class FFNTIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
                return Category_Fonts;
            }

			public override string GetFileDescription()
			{
				return "Cafe Font (FFNT)";
			}

			public override string GetFileFilter()
			{
				return "Cafe Font (*.bffnt)|*.bffnt";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'F' && File.Data[1] == 'F' && File.Data[2] == 'N' && File.Data[3] == 'T') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
