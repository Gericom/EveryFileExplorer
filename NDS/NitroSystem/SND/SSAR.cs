using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.SND
{
    public class SSAR : FileFormat<SSAR.SSARIdentifier>
    {
        public class SSARIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return null;
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Archive (SSAR)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Archive (*.ssar)|*.ssar";
            }

            public override Bitmap GetIcon()
            {
                return Resource.note_box;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'S' && File.Data[2] == 'A' && File.Data[3] == 'R') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}
