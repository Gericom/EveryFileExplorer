using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace _3DS
{
    public class CBMD:FileFormat<CBMD.CBMDIdentifier>
    {
        public class CBMDIdentifier:FileFormatIdentifier
        {
             public override string GetCategory()
            {
                return "3DS";
            }

                public override string GetFileDescription()
            {
                return "CTR Banner Model Data (CBMD)";
            }

            public override string GetFileFilter()
            {
                return "CTR Banner Model Data (*.cbmd, *.bnr)|*.cbmd;*.bnr";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'B' && File.Data[2] == 'M' && File.Data[3] == 'D') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}
