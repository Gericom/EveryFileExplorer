using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;

namespace NDS
{
	public class MODS:FileFormat<MODS.MODSIdentifier>, IViewable
    {
        public Form GetDialog()
        {
            return new Form();
        }

        public class MODSIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Movies;

            }

			public override string GetFileDescription()
			{
				return "DS Mobiclip Video Files (MODS)";
			}

			public override string GetFileFilter()
			{
				return "DS Mobiclip Video Files (*.mods)|*.mods";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

			public override FormatMatch IsFormat(EFEFile File)
			{
                if (File.Data.Length > 6 && File.Data[0] == 'M' && File.Data[1] == 'O' && File.Data[2] == 'D' && File.Data[3] == 'S' && File.Data[4] == 'N' && File.Data[5] == '3') return FormatMatch.Content;
                return FormatMatch.No;
			}

		}
	}
}
