using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace _3DS
{
	public class MSBT:FileFormat<MSBT.MSBTIdentifier>
	{
		public class MSBTIdentifier:FileFormatIdentifier
		{
			public override string GetCategory()
			{
                return Category_Text;
            }

			public override string GetFileDescription()
			{
				return "Message Binary Text (MSBT)";
			}

			public override string GetFileFilter()
			{
				return "Message Binary Text (*.msbt)|*.msbt";
			}

			public override Bitmap GetIcon()
			{
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 8 && File.Data[0] == 'M' && File.Data[1] == 's' && File.Data[2] == 'g' && File.Data[3] == 'S' && File.Data[4] == 't' && File.Data[5] == 'd' && File.Data[6] == 'B' && File.Data[7] == 'N') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
