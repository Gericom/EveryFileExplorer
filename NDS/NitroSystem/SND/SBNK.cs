using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.SND
{
    public class SBNK : FileFormat<SBNK.SBNKIdentifier>
    {
        public class SBNKIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return null;
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Bank (SBNK)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Bank (*.sbnk)|*.sbnk";
            }

            public override Bitmap GetIcon()
            {
                return Resource.guitar;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'B' && File.Data[2] == 'N' && File.Data[3] == 'K') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}
