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
    public class CGRP : FileFormat<CGRP.CGRPIdentifier>, IViewable
    {
        public Form GetDialog()
        {
            return new Form();
        }

        public class CGRPIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
                return Category_Audio;
            }

			public override string GetFileDescription()
			{
				return "CTR Group (CGRP)";
			}

			public override string GetFileFilter()
			{
				return "CTR Group (*.bcgrp)|*.bcgrp";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 45 && File.Data[0] == 'C' && File.Data[1] == 'G' && File.Data[2] == 'R' && File.Data[3] == 'P' && File.Data[0x40] == 'I' && File.Data[0x41] == 'N' && File.Data[0x42] == 'F' && File.Data[0x43] == 'O') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
