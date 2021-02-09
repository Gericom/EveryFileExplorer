using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace WiiU.NintendoWare.SND
{
	public class FWAV:FileFormat<FWAV.FWAVIdentifier>
	{
		public class FWAVIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
                return Category_Audio;
            }

			public override string GetFileDescription()
			{
				return "Cafe Wave (FWAV)";
			}

			public override string GetFileFilter()
			{
				return "Cafe Wave (*.bfwav)|*.bfwav";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'F' && File.Data[1] == 'W' && File.Data[2] == 'A' && File.Data[3] == 'V') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
