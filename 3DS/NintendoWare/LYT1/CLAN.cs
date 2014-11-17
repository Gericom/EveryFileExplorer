using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace _3DS.NintendoWare.LYT1
{
	public class CLAN : FileFormat<CLAN.CLANIdentifier>
	{
		public class CLANIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Animations;
			}

			public override string GetFileDescription()
			{
				return "CTR Layout Animation (CLAN)";
			}

			public override string GetFileFilter()
			{
				return "CTR Layout Animation (*.bclan)|*.bclan";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'L' && File.Data[2] == 'A' && File.Data[3] == 'N') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
