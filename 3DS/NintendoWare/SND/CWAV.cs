using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.SND;
using CommonFiles;
using LibEveryFileExplorer.IO;
using _3DS.UI;
using System.Windows.Forms;

namespace _3DS.NintendoWare.SND
{
    public class CWAV : FileFormat<CWAV.CWAVIdentifier>, IViewable
    {
        public Form GetDialog()
        {
            return new Form();
        }

        public class CWAVIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
                return Category_Audio;
            }

			public override string GetFileDescription()
			{
				return "CTR Wave (CWAV)";
			}

			public override string GetFileFilter()
			{
				return "CTR Wave (*.bcwav)|*.bcwav";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'W' && File.Data[2] == 'A' && File.Data[3] == 'V') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
