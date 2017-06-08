using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;

namespace WiiU.NintendoWare.SND
{
    public class FSAR : FileFormat<FSAR.FSARIdentifier>, IViewable
    {
        public Form GetDialog()
        {
            return new Form();
        }

        public class FSARIdentifier : FileFormatIdentifier
        {

            public override string GetCategory()
			{
                return Category_Sound;
            }

			public override string GetFileDescription()
			{
				return "Cafe Sound Archive (FSAR)";
			}

			public override string GetFileFilter()
			{
				return "Cafe Sound Archive (*.bfsar)|*.bfsar";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 10 && File.Data[0] == 'F' && File.Data[1] == 'S' && File.Data[2] == 'A' && File.Data[3] == 'R') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
