using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace WiiU.NintendoWare.SND
{
	public class FSTM:FileFormat<FSTM.FSTMIdentifier>
	{
		public class FSTMIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
                return Category_Audio;
            }

			public override string GetFileDescription()
			{
				return "Cafe Stream (FSTM)";
			}

			public override string GetFileFilter()
			{
				return "Cafe Stream (*.bfstm)|*.bfstm";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'F' && File.Data[1] == 'S' && File.Data[2] == 'T' && File.Data[3] == 'M') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
