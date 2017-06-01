using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.Windows.Forms;

namespace _3DS
{
	public class DVLB : FileFormat<DVLB.DVLBIdentifier>, IViewable
    {
        public Form GetDialog()
        {
            return new Form();
        }

        public class DVLBIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Shaders;
			}

			public override string GetFileDescription()
			{
				return "DMP Vertex Linker Binary (DVLB)";
			}

			public override string GetFileFilter()
			{
				return "DMP Vertex Linker Binary (*.shbin)|*.shbin";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'D' && File.Data[1] == 'V' && File.Data[2] == 'L' && File.Data[3] == 'B') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
